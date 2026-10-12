using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using KiemTraGiuaKy.Authorization;
using KiemTraGiuaKy.Exams.Dto;
using KiemTraGiuaKy.Exams.Entities;
using KiemTraGiuaKy.Exams.Enums;
using KiemTraGiuaKy.Exams.Services;
using Microsoft.EntityFrameworkCore;

namespace KiemTraGiuaKy.Exams;

public class ExamAttemptAppService : KiemTraGiuaKyAppServiceBase, IExamAttemptAppService
{
    private readonly IRepository<Exam, long> _examRepository;
    private readonly IRepository<QuestionGroup, long> _groupRepository;
    private readonly IRepository<Question, long> _questionRepository;
    private readonly IRepository<ExamAttempt, long> _attemptRepository;
    private readonly IRepository<AttemptAnswer, long> _answerRepository;
    private readonly IToeicScoringService _scoringService;

    public ExamAttemptAppService(
        IRepository<Exam, long> examRepository,
        IRepository<QuestionGroup, long> groupRepository,
        IRepository<Question, long> questionRepository,
        IRepository<ExamAttempt, long> attemptRepository,
        IRepository<AttemptAnswer, long> answerRepository,
        IToeicScoringService scoringService)
    {
        _examRepository = examRepository;
        _groupRepository = groupRepository;
        _questionRepository = questionRepository;
        _attemptRepository = attemptRepository;
        _answerRepository = answerRepository;
        _scoringService = scoringService;
    }

    public async Task<List<ExamListDto>> GetAvailableExams()
    {
        return await _examRepository.GetAll()
            .Where(e => e.IsPublished)
            .OrderByDescending(e => e.CreationTime)
            .Select(e => new ExamListDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                DurationMinutes = e.DurationMinutes,
                TotalQuestions = e.TotalQuestions,
                IsPublished = e.IsPublished,
                CreationTime = e.CreationTime
            })
            .ToListAsync();
    }

    public async Task<ExamForTakingDto> GetExamForTaking(long examId)
    {
        var exam = await _examRepository.GetAsync(examId);
        if (!exam.IsPublished)
        {
            throw new UserFriendlyException("Đề thi chưa được công bố hoặc không tồn tại.");
        }

        var groups = await _groupRepository.GetAll()
            .Where(g => g.ExamId == examId)
            .Include(g => g.PassageItems)
            .Include(g => g.Questions)
                .ThenInclude(q => q.Options)
            .OrderBy(g => g.PartNumber)
            .ThenBy(g => g.OrderIndex)
            .ToListAsync();

        var standaloneQuestions = await _questionRepository.GetAll()
            .Where(q => q.ExamId == examId && q.QuestionGroupId == null)
            .Include(q => q.Options)
            .OrderBy(q => q.QuestionNumber)
            .ToListAsync();

        return new ExamForTakingDto
        {
            Id = exam.Id,
            Title = exam.Title,
            DurationMinutes = exam.DurationMinutes,
            TotalQuestions = exam.TotalQuestions,
            StandaloneQuestions = standaloneQuestions.Select(q => new QuestionForTakingDto
            {
                Id = q.Id,
                PartNumber = q.PartNumber,
                QuestionNumber = q.QuestionNumber,
                Content = q.Content,
                Options = q.Options.OrderBy(o => o.Label).Select(o => new QuestionOptionDto
                {
                    Id = o.Id,
                    Label = o.Label,
                    Content = o.Content
                }).ToList()
            }).ToList(),
            Groups = groups.Select(g => new QuestionGroupForTakingDto
            {
                Id = g.Id,
                PartNumber = g.PartNumber,
                OrderIndex = g.OrderIndex,
                Instruction = g.Instruction,
                FromQuestionNumber = g.FromQuestionNumber,
                ToQuestionNumber = g.ToQuestionNumber,
                PassageItems = g.PassageItems.OrderBy(pi => pi.OrderIndex).Select(pi => new PassageItemDto
                {
                    Id = pi.Id,
                    OrderIndex = pi.OrderIndex,
                    Type = pi.Type,
                    TextContent = pi.TextContent,
                    ImageUrl = pi.ImageUrl
                }).ToList(),
                Questions = g.Questions.OrderBy(q => q.QuestionNumber).Select(q => new QuestionForTakingDto
                {
                    Id = q.Id,
                    PartNumber = q.PartNumber,
                    QuestionNumber = q.QuestionNumber,
                    Content = q.Content,
                    Options = q.Options.OrderBy(o => o.Label).Select(o => new QuestionOptionDto
                    {
                        Id = o.Id,
                        Label = o.Label,
                        Content = o.Content
                    }).ToList()
                }).ToList()
            }).ToList()
        };
    }

    public async Task<StartAttemptResultDto> StartAttempt(StartAttemptInput input)
    {
        var exam = await _examRepository.GetAsync(input.ExamId);
        if (!exam.IsPublished)
        {
            throw new UserFriendlyException("Đề thi chưa được công bố.");
        }

        var userId = AbpSession.UserId ?? 1;
        var startTime = DateTime.UtcNow;
        var endTime = startTime.AddMinutes(exam.DurationMinutes);

        var attempt = new ExamAttempt
        {
            ExamId = exam.Id,
            UserId = userId,
            StartTime = startTime,
            EndTime = endTime,
            Status = AttemptStatus.InProgress,
            CorrectCount = 0,
            WrongCount = 0,
            SkippedCount = 0
        };

        var attemptId = await _attemptRepository.InsertAndGetIdAsync(attempt);

        return new StartAttemptResultDto
        {
            AttemptId = attemptId,
            ExamId = exam.Id,
            ExamTitle = exam.Title,
            StartTime = startTime,
            EndTime = endTime,
            DurationMinutes = exam.DurationMinutes,
            TotalQuestions = exam.TotalQuestions
        };
    }

    public async Task<SubmitAnswerResultDto> SubmitAnswer(SubmitAnswerInput input)
    {
        var attempt = await _attemptRepository.GetAsync(input.AttemptId);
        var userId = AbpSession.UserId ?? 1;

        if (attempt.UserId != userId)
        {
            throw new UserFriendlyException("Bạn không có quyền truy cập lượt thi này.");
        }

        if (attempt.Status != AttemptStatus.InProgress)
        {
            throw new UserFriendlyException("Bài thi đã được nộp hoặc đã kết thúc.");
        }

        // Server-side strict deadline enforcement
        if (DateTime.UtcNow > attempt.EndTime)
        {
            await GradeAndFinalizeAttempt(attempt, AttemptStatus.AutoSubmitted);
            throw new UserFriendlyException("Thời gian làm bài đã hết! Hệ thống đã tự động thu bài.");
        }

        var existingAnswer = await _answerRepository.FirstOrDefaultAsync(
            a => a.AttemptId == input.AttemptId && a.QuestionId == input.QuestionId);

        if (existingAnswer != null)
        {
            existingAnswer.SelectedOption = input.SelectedOption;
            await _answerRepository.UpdateAsync(existingAnswer);
        }
        else
        {
            await _answerRepository.InsertAsync(new AttemptAnswer
            {
                AttemptId = input.AttemptId,
                QuestionId = input.QuestionId,
                SelectedOption = input.SelectedOption
            });
        }

        return new SubmitAnswerResultDto
        {
            AttemptId = input.AttemptId,
            QuestionId = input.QuestionId,
            SelectedOption = input.SelectedOption,
            SavedAt = DateTime.UtcNow
        };
    }

    public async Task<AttemptResultDetailDto> FinishAttempt(FinishAttemptInput input)
    {
        var attempt = await _attemptRepository.GetAsync(input.AttemptId);
        var userId = AbpSession.UserId ?? 1;

        if (attempt.UserId != userId)
        {
            throw new UserFriendlyException("Bạn không có quyền truy cập lượt thi này.");
        }

        if (attempt.Status == AttemptStatus.InProgress)
        {
            var status = DateTime.UtcNow > attempt.EndTime ? AttemptStatus.AutoSubmitted : AttemptStatus.Submitted;
            await GradeAndFinalizeAttempt(attempt, status);
        }

        return await BuildResultDetailDto(attempt);
    }

    public async Task<AttemptResultDetailDto> GetResult(long attemptId)
    {
        var attempt = await _attemptRepository.GetAsync(attemptId);
        var userId = AbpSession.UserId ?? 1;

        if (attempt.UserId != userId && !IsGranted(PermissionNames.Pages_Exams))
        {
            throw new UserFriendlyException("Bạn không có quyền xem kết quả này.");
        }

        if (attempt.Status == AttemptStatus.InProgress)
        {
            throw new UserFriendlyException("Bài thi đang diễn ra, chưa có kết quả.");
        }

        return await BuildResultDetailDto(attempt);
    }

    public async Task<List<AttemptSummaryDto>> GetMyAttempts()
    {
        var userId = AbpSession.UserId ?? 1;

        var attempts = await _attemptRepository.GetAll()
            .Where(a => a.UserId == userId)
            .Include(a => a.Exam)
            .OrderByDescending(a => a.StartTime)
            .ToListAsync();

        return attempts.Select(a =>
        {
            var timeSpent = a.SubmittedTime.HasValue
                ? (int)(a.SubmittedTime.Value - a.StartTime).TotalSeconds
                : (int)(a.EndTime - a.StartTime).TotalSeconds;

            return new AttemptSummaryDto
            {
                AttemptId = a.Id,
                ExamId = a.ExamId,
                ExamTitle = a.Exam?.Title ?? "N/A",
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                SubmittedTime = a.SubmittedTime,
                Status = a.Status,
                TotalQuestions = a.Exam?.TotalQuestions ?? 100,
                CorrectCount = a.CorrectCount,
                WrongCount = a.WrongCount,
                SkippedCount = a.SkippedCount,
                ScaledScore = _scoringService.CalculateReadingScore(a.CorrectCount),
                TimeSpentSeconds = Math.Max(0, timeSpent)
            };
        }).ToList();
    }

    private async Task GradeAndFinalizeAttempt(ExamAttempt attempt, AttemptStatus targetStatus)
    {
        var questions = await _questionRepository.GetAll()
            .Where(q => q.ExamId == attempt.ExamId)
            .ToListAsync();

        var userAnswers = await _answerRepository.GetAll()
            .Where(a => a.AttemptId == attempt.Id)
            .ToListAsync();

        var answerDict = userAnswers.ToDictionary(a => a.QuestionId);

        int correctCount = 0;
        int wrongCount = 0;
        int skippedCount = 0;

        foreach (var q in questions)
        {
            if (answerDict.TryGetValue(q.Id, out var ans))
            {
                if (ans.SelectedOption.HasValue)
                {
                    bool isCorrect = ans.SelectedOption.Value == q.CorrectOption;
                    ans.IsCorrect = isCorrect;
                    if (isCorrect) correctCount++;
                    else wrongCount++;
                    await _answerRepository.UpdateAsync(ans);
                }
                else
                {
                    ans.IsCorrect = false;
                    skippedCount++;
                    await _answerRepository.UpdateAsync(ans);
                }
            }
            else
            {
                // Unanswered question => record as skipped
                skippedCount++;
                await _answerRepository.InsertAsync(new AttemptAnswer
                {
                    AttemptId = attempt.Id,
                    QuestionId = q.Id,
                    SelectedOption = null,
                    IsCorrect = false
                });
            }
        }

        attempt.CorrectCount = correctCount;
        attempt.WrongCount = wrongCount;
        attempt.SkippedCount = skippedCount;
        attempt.Status = targetStatus;
        attempt.SubmittedTime = DateTime.UtcNow;

        await _attemptRepository.UpdateAsync(attempt);
    }

    private async Task<AttemptResultDetailDto> BuildResultDetailDto(ExamAttempt attempt)
    {
        var exam = await _examRepository.GetAsync(attempt.ExamId);

        var questions = await _questionRepository.GetAll()
            .Where(q => q.ExamId == attempt.ExamId)
            .Include(q => q.Options)
            .OrderBy(q => q.QuestionNumber)
            .ToListAsync();

        var userAnswers = await _answerRepository.GetAll()
            .Where(a => a.AttemptId == attempt.Id)
            .ToDictionaryAsync(a => a.QuestionId);

        var reviews = new List<QuestionReviewDto>();

        int p5Total = 0, p5Correct = 0, p5Wrong = 0, p5Skipped = 0;
        int p6Total = 0, p6Correct = 0, p6Wrong = 0, p6Skipped = 0;
        int p7Total = 0, p7Correct = 0, p7Wrong = 0, p7Skipped = 0;

        foreach (var q in questions)
        {
            userAnswers.TryGetValue(q.Id, out var ans);
            var selectedOpt = ans?.SelectedOption;
            bool isCorrect = selectedOpt.HasValue && selectedOpt.Value == q.CorrectOption;
            bool isSkipped = !selectedOpt.HasValue;

            if (q.PartNumber == 5)
            {
                p5Total++;
                if (isCorrect) p5Correct++;
                else if (isSkipped) p5Skipped++;
                else p5Wrong++;
            }
            else if (q.PartNumber == 6)
            {
                p6Total++;
                if (isCorrect) p6Correct++;
                else if (isSkipped) p6Skipped++;
                else p6Wrong++;
            }
            else
            {
                p7Total++;
                if (isCorrect) p7Correct++;
                else if (isSkipped) p7Skipped++;
                else p7Wrong++;
            }

            reviews.Add(new QuestionReviewDto
            {
                QuestionId = q.Id,
                PartNumber = q.PartNumber,
                QuestionNumber = q.QuestionNumber,
                Content = q.Content,
                Explanation = q.Explanation,
                SelectedOption = selectedOpt,
                CorrectOption = q.CorrectOption,
                IsCorrect = isCorrect,
                Options = q.Options.OrderBy(o => o.Label).Select(o => new QuestionOptionDto
                {
                    Id = o.Id,
                    Label = o.Label,
                    Content = o.Content
                }).ToList()
            });
        }

        var timeSpent = attempt.SubmittedTime.HasValue
            ? (int)(attempt.SubmittedTime.Value - attempt.StartTime).TotalSeconds
            : (int)(attempt.EndTime - attempt.StartTime).TotalSeconds;

        return new AttemptResultDetailDto
        {
            Summary = new AttemptSummaryDto
            {
                AttemptId = attempt.Id,
                ExamId = exam.Id,
                ExamTitle = exam.Title,
                StartTime = attempt.StartTime,
                EndTime = attempt.EndTime,
                SubmittedTime = attempt.SubmittedTime,
                Status = attempt.Status,
                TotalQuestions = exam.TotalQuestions,
                CorrectCount = attempt.CorrectCount,
                WrongCount = attempt.WrongCount,
                SkippedCount = attempt.SkippedCount,
                ScaledScore = _scoringService.CalculateReadingScore(attempt.CorrectCount),
                TimeSpentSeconds = Math.Max(0, timeSpent)
            },
            Part5 = new PartScoreDto
            {
                PartNumber = 5,
                PartName = "Part 5: Incomplete Sentences",
                TotalQuestions = p5Total,
                CorrectCount = p5Correct,
                WrongCount = p5Wrong,
                SkippedCount = p5Skipped
            },
            Part6 = new PartScoreDto
            {
                PartNumber = 6,
                PartName = "Part 6: Text Completion",
                TotalQuestions = p6Total,
                CorrectCount = p6Correct,
                WrongCount = p6Wrong,
                SkippedCount = p6Skipped
            },
            Part7 = new PartScoreDto
            {
                PartNumber = 7,
                PartName = "Part 7: Reading Comprehension",
                TotalQuestions = p7Total,
                CorrectCount = p7Correct,
                WrongCount = p7Wrong,
                SkippedCount = p7Skipped
            },
            QuestionReviews = reviews
        };
    }
}
