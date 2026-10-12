using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using KiemTraGiuaKy.Authorization;
using KiemTraGiuaKy.Authorization.Users;
using KiemTraGiuaKy.Exams.Dto;
using KiemTraGiuaKy.Exams.Entities;
using KiemTraGiuaKy.Exams.Enums;
using KiemTraGiuaKy.Exams.Parsing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace KiemTraGiuaKy.Exams;

public class ExamAppService : KiemTraGiuaKyAppServiceBase, IExamAppService
{
    private readonly IRepository<Exam, long> _examRepository;
    private readonly IRepository<QuestionGroup, long> _groupRepository;
    private readonly IRepository<Question, long> _questionRepository;
    private readonly IRepository<QuestionOption, long> _optionRepository;
    private readonly IRepository<PassageItem, long> _passageItemRepository;
    private readonly IDocxParser _docxParser;
    private readonly IWebHostEnvironment _env;

    public ExamAppService(
        IRepository<Exam, long> examRepository,
        IRepository<QuestionGroup, long> groupRepository,
        IRepository<Question, long> questionRepository,
        IRepository<QuestionOption, long> optionRepository,
        IRepository<PassageItem, long> passageItemRepository,
        IDocxParser docxParser,
        IWebHostEnvironment env)
    {
        _examRepository = examRepository;
        _groupRepository = groupRepository;
        _questionRepository = questionRepository;
        _optionRepository = optionRepository;
        _passageItemRepository = passageItemRepository;
        _docxParser = docxParser;
        _env = env;
    }

    public async Task<PagedResultDto<ExamListDto>> GetAll(PagedAndSortedResultRequestDto input)
    {
        var query = _examRepository.GetAll();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(e => e.CreationTime)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
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

        return new PagedResultDto<ExamListDto>(totalCount, items);
    }

    public async Task<ExamDetailDto> GetForEdit(long id)
    {
        return await GetExamDetailInternal(id);
    }

    public async Task<ExamDetailDto> GetForPreview(long id)
    {
        return await GetExamDetailInternal(id);
    }

    public async Task Update(UpdateExamDto input)
    {
        var exam = await _examRepository.GetAsync(input.Id);
        exam.Title = input.Title;
        exam.Description = input.Description;
        exam.DurationMinutes = input.DurationMinutes;
        await _examRepository.UpdateAsync(exam);
    }

    public async Task UpdateQuestion(UpdateQuestionDto input)
    {
        var question = await _questionRepository.GetAllIncluding(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == input.Id);

        if (question == null)
        {
            throw new UserFriendlyException("Question not found.");
        }

        question.Content = input.Content;
        question.CorrectOption = input.CorrectOption;
        question.Explanation = input.Explanation;

        foreach (var optDto in input.Options)
        {
            var existingOpt = question.Options.FirstOrDefault(o => o.Label == optDto.Label);
            if (existingOpt != null)
            {
                existingOpt.Content = optDto.Content;
            }
        }

        await _questionRepository.UpdateAsync(question);
    }

    public async Task<bool> TogglePublish(long id)
    {
        var exam = await _examRepository.GetAsync(id);
        exam.IsPublished = !exam.IsPublished;
        await _examRepository.UpdateAsync(exam);
        return exam.IsPublished;
    }

    public async Task Delete(long id)
    {
        await _examRepository.DeleteAsync(id);
    }

    public async Task<ImportExamResultDto> ImportFromDocx(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new UserFriendlyException("Vui lòng chọn file .docx hợp lệ.");
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        return await ImportFromStreamInternal(memoryStream, Path.GetFileNameWithoutExtension(file.FileName));
    }

    public async Task<ImportExamResultDto> ImportFromBytes(byte[] fileBytes, string fileName)
    {
        if (fileBytes == null || fileBytes.Length == 0)
        {
            throw new UserFriendlyException("File rỗng.");
        }

        using var memoryStream = new MemoryStream(fileBytes);
        return await ImportFromStreamInternal(memoryStream, Path.GetFileNameWithoutExtension(fileName));
    }

    private async Task<ImportExamResultDto> ImportFromStreamInternal(Stream stream, string defaultTitle)
    {
        var parseResult = _docxParser.Parse(stream);
        if (!parseResult.IsValid)
        {
            var errorMessages = string.Join("\n", parseResult.Errors.Select(e => e.ToString()));
            throw new UserFriendlyException("File .docx không hợp lệ:", errorMessages);
        }

        var parsed = parseResult.ParsedExam;

        var exam = new Exam
        {
            Title = string.IsNullOrWhiteSpace(parsed.Title) ? defaultTitle : parsed.Title,
            Description = parsed.Description,
            DurationMinutes = parsed.DurationMinutes > 0 ? parsed.DurationMinutes : 75,
            TotalQuestions = 100,
            IsPublished = false
        };

        var examId = await _examRepository.InsertAndGetIdAsync(exam);

        var webRoot = _env.WebRootPath ?? AppContext.BaseDirectory;
        var uploadDir = Path.Combine(webRoot, "uploads", "exams", examId.ToString());
        if (!Directory.Exists(uploadDir))
        {
            Directory.CreateDirectory(uploadDir);
        }

        int imageCounter = 0;

        // 1. Process Standalone Part 5 Questions
        foreach (var pq in parsed.StandaloneQuestions)
        {
            var question = new Question
            {
                ExamId = examId,
                QuestionGroupId = null,
                PartNumber = pq.PartNumber,
                QuestionNumber = pq.QuestionNumber,
                Content = pq.Content,
                CorrectOption = pq.CorrectOption,
                Explanation = pq.Explanation
            };

            var qId = await _questionRepository.InsertAndGetIdAsync(question);

            foreach (var opt in pq.Options)
            {
                await _optionRepository.InsertAsync(new QuestionOption
                {
                    QuestionId = qId,
                    Label = opt.Label,
                    Content = opt.Content
                });
            }
        }

        // 2. Process Part 6 & Part 7 Groups
        foreach (var pgroup in parsed.Groups)
        {
            var group = new QuestionGroup
            {
                ExamId = examId,
                PartNumber = pgroup.PartNumber,
                OrderIndex = pgroup.OrderIndex,
                Instruction = pgroup.Instruction,
                FromQuestionNumber = pgroup.FromQuestionNumber,
                ToQuestionNumber = pgroup.ToQuestionNumber
            };

            var groupId = await _groupRepository.InsertAndGetIdAsync(group);

            // Save Passage Items
            foreach (var pItem in pgroup.PassageItems)
            {
                string savedUrl = null;
                if (pItem.Type == PassageItemType.Image && pItem.ImageBytes != null && pItem.ImageBytes.Length > 0)
                {
                    imageCounter++;
                    var ext = string.IsNullOrWhiteSpace(pItem.ImageExtension) ? "png" : pItem.ImageExtension.TrimStart('.');
                    var fileName = $"img_{imageCounter}_{pItem.OrderIndex}.{ext}";
                    var fullPath = Path.Combine(uploadDir, fileName);
                    await File.WriteAllBytesAsync(fullPath, pItem.ImageBytes);
                    savedUrl = $"/uploads/exams/{examId}/{fileName}";
                }

                await _passageItemRepository.InsertAsync(new PassageItem
                {
                    QuestionGroupId = groupId,
                    OrderIndex = pItem.OrderIndex,
                    Type = pItem.Type,
                    TextContent = pItem.Type == PassageItemType.Text ? pItem.TextContent : null,
                    ImageUrl = savedUrl
                });
            }

            // Save Questions in Group
            foreach (var pq in pgroup.Questions)
            {
                var question = new Question
                {
                    ExamId = examId,
                    QuestionGroupId = groupId,
                    PartNumber = pq.PartNumber,
                    QuestionNumber = pq.QuestionNumber,
                    Content = pq.Content,
                    CorrectOption = pq.CorrectOption,
                    Explanation = pq.Explanation
                };

                var qId = await _questionRepository.InsertAndGetIdAsync(question);

                foreach (var opt in pq.Options)
                {
                    await _optionRepository.InsertAsync(new QuestionOption
                    {
                        QuestionId = qId,
                        Label = opt.Label,
                        Content = opt.Content
                    });
                }
            }
        }

        return new ImportExamResultDto
        {
            ExamId = examId,
            Title = exam.Title,
            DurationMinutes = exam.DurationMinutes,
            Part5Count = parsed.StandaloneQuestions.Count,
            Part6Count = parsed.Groups.Where(g => g.PartNumber == 6).Sum(g => g.Questions.Count),
            Part7Count = parsed.Groups.Where(g => g.PartNumber == 7).Sum(g => g.Questions.Count),
            GroupCount = parsed.Groups.Count,
            ImageCount = imageCounter
        };
    }

    private async Task<ExamDetailDto> GetExamDetailInternal(long id)
    {
        var exam = await _examRepository.GetAsync(id);

        var groups = await _groupRepository.GetAll()
            .Where(g => g.ExamId == id)
            .Include(g => g.PassageItems)
            .Include(g => g.Questions)
                .ThenInclude(q => q.Options)
            .OrderBy(g => g.PartNumber)
            .ThenBy(g => g.OrderIndex)
            .ToListAsync();

        var standaloneQuestions = await _questionRepository.GetAll()
            .Where(q => q.ExamId == id && q.QuestionGroupId == null)
            .Include(q => q.Options)
            .OrderBy(q => q.QuestionNumber)
            .ToListAsync();

        var result = new ExamDetailDto
        {
            Id = exam.Id,
            Title = exam.Title,
            Description = exam.Description,
            DurationMinutes = exam.DurationMinutes,
            TotalQuestions = exam.TotalQuestions,
            IsPublished = exam.IsPublished,
            StandaloneQuestions = standaloneQuestions.Select(q => MapQuestionDetail(q)).ToList(),
            Groups = groups.Select(g => new QuestionGroupDetailDto
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
                Questions = g.Questions.OrderBy(q => q.QuestionNumber).Select(q => MapQuestionDetail(q)).ToList()
            }).ToList()
        };

        return result;
    }

    private static QuestionDetailDto MapQuestionDetail(Question q)
    {
        return new QuestionDetailDto
        {
            Id = q.Id,
            PartNumber = q.PartNumber,
            QuestionNumber = q.QuestionNumber,
            Content = q.Content,
            CorrectOption = q.CorrectOption,
            Explanation = q.Explanation,
            Options = q.Options.OrderBy(o => o.Label).Select(o => new QuestionOptionDto
            {
                Id = o.Id,
                Label = o.Label,
                Content = o.Content
            }).ToList()
        };
    }
}
