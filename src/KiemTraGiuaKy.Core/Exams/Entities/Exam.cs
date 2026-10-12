using Abp.Domain.Entities.Auditing;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KiemTraGiuaKy.Exams.Entities;

/// <summary>
/// Represents a TOEIC Reading exam with its metadata.
/// Contains 100 questions across Parts 5, 6, and 7.
/// Duration defaults to 75 minutes.
/// </summary>
public class Exam : FullAuditedEntity<long>
{
    /// <summary>Maximum length for the exam title.</summary>
    public const int MaxTitleLength = 256;

    /// <summary>Maximum length for the description.</summary>
    public const int MaxDescriptionLength = 2000;

    /// <summary>Title of the exam, e.g. "TOEIC Reading - Đề số 1".</summary>
    [Required]
    [MaxLength(MaxTitleLength)]
    public string Title { get; set; }

    /// <summary>Optional description shown to learners.</summary>
    [MaxLength(MaxDescriptionLength)]
    public string Description { get; set; }

    /// <summary>Allowed time in minutes. Default is 75 for TOEIC Reading.</summary>
    public int DurationMinutes { get; set; } = 75;

    /// <summary>Total number of questions (should be 100 for a standard TOEIC Reading).</summary>
    public int TotalQuestions { get; set; }

    /// <summary>
    /// Whether this exam is visible to learners.
    /// Teacher must explicitly publish after review.
    /// </summary>
    public bool IsPublished { get; set; } = false;

    // Navigation properties
    public virtual ICollection<QuestionGroup> QuestionGroups { get; set; }
    public virtual ICollection<Question> Questions { get; set; }
    public virtual ICollection<ExamAttempt> Attempts { get; set; }
}
