using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using KiemTraGiuaKy.Exams.Dto;

namespace KiemTraGiuaKy.Exams;

public interface IExamAttemptAppService : IApplicationService
{
    /// <summary>
    /// Gets all published exams available for learners.
    /// </summary>
    Task<List<ExamListDto>> GetAvailableExams();

    /// <summary>
    /// Retrieves full exam contents for taking the test.
    /// Does NOT include correct answers or explanations.
    /// </summary>
    Task<ExamForTakingDto> GetExamForTaking(long examId);

    /// <summary>
    /// Starts an attempt, returning start and deadline times.
    /// </summary>
    Task<StartAttemptResultDto> StartAttempt(StartAttemptInput input);

    /// <summary>
    /// Records a learner's answer to a single question during the attempt.
    /// Validates deadline on the server.
    /// </summary>
    Task<SubmitAnswerResultDto> SubmitAnswer(SubmitAnswerInput input);

    /// <summary>
    /// Finalizes and grades the attempt, computing raw counts, scaled TOEIC score, and breakdown.
    /// </summary>
    Task<AttemptResultDetailDto> FinishAttempt(FinishAttemptInput input);

    /// <summary>
    /// Gets full results, part breakdown, and answer key review after attempt submission.
    /// </summary>
    Task<AttemptResultDetailDto> GetResult(long attemptId);

    /// <summary>
    /// Gets historical attempts of the currently logged-in learner.
    /// </summary>
    Task<List<AttemptSummaryDto>> GetMyAttempts();
}
