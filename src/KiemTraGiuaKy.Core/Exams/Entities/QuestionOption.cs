using Abp.Domain.Entities.Auditing;
using KiemTraGiuaKy.Exams.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KiemTraGiuaKy.Exams.Entities;

/// <summary>
/// Represents one answer option (A, B, C, or D) for a question.
/// Each question must have exactly 4 options.
/// </summary>
public class QuestionOption : FullAuditedEntity<long>
{
    public const int MaxContentLength = 2000;

    /// <summary>Foreign key to the parent question.</summary>
    public long QuestionId { get; set; }

    /// <summary>The label: A, B, C, or D.</summary>
    public AnswerOption Label { get; set; }

    /// <summary>The text of this option. Must be non-empty.</summary>
    [Required]
    [MaxLength(MaxContentLength)]
    public string Content { get; set; }

    // Navigation
    [ForeignKey(nameof(QuestionId))]
    public virtual Question Question { get; set; }
}
