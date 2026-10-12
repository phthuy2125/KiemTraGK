using System;
using System.Collections.Generic;
using KiemTraGiuaKy.Exams.Enums;

namespace KiemTraGiuaKy.Exams.Dto;

// ─────────────────────────────────────────────────────────────────────────────
// Exam DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Summary DTO for the exam list page (admin and mobile).</summary>
public class ExamListDto
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int DurationMinutes { get; set; }
    public int TotalQuestions { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreationTime { get; set; }
    public string CreatorUserName { get; set; }
}

/// <summary>DTO for updating exam metadata (title, duration, description).</summary>
public class UpdateExamDto
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int DurationMinutes { get; set; }
}

/// <summary>
/// Full exam structure for teacher preview and edit page.
/// Includes correct answers.
/// </summary>
public class ExamDetailDto
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int DurationMinutes { get; set; }
    public int TotalQuestions { get; set; }
    public bool IsPublished { get; set; }
    public List<QuestionGroupDetailDto> Groups { get; set; } = new();
    public List<QuestionDetailDto> StandaloneQuestions { get; set; } = new(); // Part 5
}

/// <summary>
/// Full exam structure returned to the mobile learner for taking the exam.
/// DOES NOT include correct answers.
/// </summary>
public class ExamForTakingDto
{
    public long Id { get; set; }
    public string Title { get; set; }
    public int DurationMinutes { get; set; }
    public int TotalQuestions { get; set; }
    public List<QuestionGroupForTakingDto> Groups { get; set; } = new();
    public List<QuestionForTakingDto> StandaloneQuestions { get; set; } = new(); // Part 5
}

// ─────────────────────────────────────────────────────────────────────────────
// Question Group DTOs
// ─────────────────────────────────────────────────────────────────────────────

public class QuestionGroupDetailDto
{
    public long Id { get; set; }
    public int PartNumber { get; set; }
    public int OrderIndex { get; set; }
    public string Instruction { get; set; }
    public int FromQuestionNumber { get; set; }
    public int ToQuestionNumber { get; set; }
    public List<PassageItemDto> PassageItems { get; set; } = new();
    public List<QuestionDetailDto> Questions { get; set; } = new();
}

public class QuestionGroupForTakingDto
{
    public long Id { get; set; }
    public int PartNumber { get; set; }
    public int OrderIndex { get; set; }
    public string Instruction { get; set; }
    public int FromQuestionNumber { get; set; }
    public int ToQuestionNumber { get; set; }
    public List<PassageItemDto> PassageItems { get; set; } = new();
    public List<QuestionForTakingDto> Questions { get; set; } = new();
}

// ─────────────────────────────────────────────────────────────────────────────
// Passage Item DTOs
// ─────────────────────────────────────────────────────────────────────────────

public class PassageItemDto
{
    public long Id { get; set; }
    public int OrderIndex { get; set; }
    public PassageItemType Type { get; set; }
    public string TextContent { get; set; }
    public string ImageUrl { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// Question DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Full question DTO with correct answer — for admin/teacher only.</summary>
public class QuestionDetailDto
{
    public long Id { get; set; }
    public int PartNumber { get; set; }
    public int QuestionNumber { get; set; }
    public string Content { get; set; }
    public AnswerOption CorrectOption { get; set; }
    public string Explanation { get; set; }
    public List<QuestionOptionDto> Options { get; set; } = new();
}

/// <summary>Question DTO for learners — no correct answer exposed.</summary>
public class QuestionForTakingDto
{
    public long Id { get; set; }
    public int PartNumber { get; set; }
    public int QuestionNumber { get; set; }
    public string Content { get; set; }
    public List<QuestionOptionDto> Options { get; set; } = new();
}

public class QuestionOptionDto
{
    public long Id { get; set; }
    public AnswerOption Label { get; set; }
    public string Content { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// Edit DTOs
// ─────────────────────────────────────────────────────────────────────────────

public class UpdateQuestionDto
{
    public long Id { get; set; }
    public string Content { get; set; }
    public AnswerOption CorrectOption { get; set; }
    public string Explanation { get; set; }
    public List<UpdateOptionDto> Options { get; set; } = new();
}

public class UpdateOptionDto
{
    public long Id { get; set; }
    public AnswerOption Label { get; set; }
    public string Content { get; set; }
}

public class UpdatePassageItemDto
{
    public long Id { get; set; }
    public PassageItemType Type { get; set; }
    public string TextContent { get; set; }
    public string ImageUrl { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// Import DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Summary returned after a successful import.</summary>
public class ImportExamResultDto
{
    public long ExamId { get; set; }
    public string Title { get; set; }
    public int DurationMinutes { get; set; }
    public int Part5Count { get; set; }
    public int Part6Count { get; set; }
    public int Part7Count { get; set; }
    public int GroupCount { get; set; }
    public int ImageCount { get; set; }
    public int TotalQuestions => Part5Count + Part6Count + Part7Count;
}

/// <summary>Result returned when import fails (validation errors).</summary>
public class ImportValidationErrorDto
{
    public List<string> Errors { get; set; } = new();
}
