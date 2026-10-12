using System;
using System.Collections.Generic;
using KiemTraGiuaKy.Exams.Enums;

namespace KiemTraGiuaKy.Exams.Dto;

/// <summary>
/// Input for starting an exam attempt.
/// </summary>
public class StartAttemptInput
{
    public long ExamId { get; set; }
}

/// <summary>
/// Response after an attempt is successfully started.
/// Contains server-side deadline so the client cannot tamper with the timer.
/// </summary>
public class StartAttemptResultDto
{
    public long AttemptId { get; set; }
    public long ExamId { get; set; }
    public string ExamTitle { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public int TotalQuestions { get; set; }
    public int RemainingSeconds => (int)Math.Max(0, (EndTime - DateTime.UtcNow).TotalSeconds);
}

/// <summary>
/// Input for submitting a single answer in real-time.
/// </summary>
public class SubmitAnswerInput
{
    public long AttemptId { get; set; }
    public long QuestionId { get; set; }
    public AnswerOption? SelectedOption { get; set; }
}

/// <summary>
/// Result for a single answer submission.
/// </summary>
public class SubmitAnswerResultDto
{
    public long AttemptId { get; set; }
    public long QuestionId { get; set; }
    public AnswerOption? SelectedOption { get; set; }
    public DateTime SavedAt { get; set; }
}

/// <summary>
/// Input for finishing / submitting the entire attempt.
/// </summary>
public class FinishAttemptInput
{
    public long AttemptId { get; set; }
}

/// <summary>
/// Summary information for an attempt (history list and result header).
/// </summary>
public class AttemptSummaryDto
{
    public long AttemptId { get; set; }
    public long ExamId { get; set; }
    public string ExamTitle { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime? SubmittedTime { get; set; }
    public AttemptStatus Status { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }
    public int SkippedCount { get; set; }
    public int ScaledScore { get; set; }
    public int TimeSpentSeconds { get; set; }
}

/// <summary>
/// Performance breakdown by TOEIC part (Part 5, Part 6, Part 7).
/// </summary>
public class PartScoreDto
{
    public int PartNumber { get; set; }
    public string PartName { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }
    public int SkippedCount { get; set; }
    public double AccuracyPercentage => TotalQuestions > 0 ? Math.Round((double)CorrectCount / TotalQuestions * 100, 1) : 0;
}

/// <summary>
/// Full question review including learner's selection, correct answer, and explanation.
/// Only exposed AFTER the attempt is finished.
/// </summary>
public class QuestionReviewDto
{
    public long QuestionId { get; set; }
    public int PartNumber { get; set; }
    public int QuestionNumber { get; set; }
    public string Content { get; set; }
    public string Explanation { get; set; }
    public AnswerOption? SelectedOption { get; set; }
    public AnswerOption CorrectOption { get; set; }
    public bool IsCorrect { get; set; }
    public bool IsSkipped => !SelectedOption.HasValue;
    public List<QuestionOptionDto> Options { get; set; } = new();
}

/// <summary>
/// Full attempt result payload with summary, part breakdown, and question review.
/// </summary>
public class AttemptResultDetailDto
{
    public AttemptSummaryDto Summary { get; set; }
    public PartScoreDto Part5 { get; set; }
    public PartScoreDto Part6 { get; set; }
    public PartScoreDto Part7 { get; set; }
    public List<QuestionReviewDto> QuestionReviews { get; set; } = new();
}
