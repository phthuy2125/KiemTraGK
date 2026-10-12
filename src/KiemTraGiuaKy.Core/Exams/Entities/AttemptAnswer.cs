using Abp.Domain.Entities.Auditing;
using KiemTraGiuaKy.Exams.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace KiemTraGiuaKy.Exams.Entities;

/// <summary>
/// Records the learner's selected option for one question within an attempt.
/// SelectedOption = null means the question was skipped.
/// IsCorrect is computed during grading (Submit).
/// </summary>
public class AttemptAnswer : FullAuditedEntity<long>
{
    /// <summary>Foreign key to the parent attempt.</summary>
    public long AttemptId { get; set; }

    /// <summary>Foreign key to the question being answered.</summary>
    public long QuestionId { get; set; }

    /// <summary>
    /// The option the learner selected.
    /// Null = skipped/not answered.
    /// </summary>
    public AnswerOption? SelectedOption { get; set; }

    /// <summary>
    /// Whether the selected option is correct.
    /// Null until the attempt is graded (submitted).
    /// </summary>
    public bool? IsCorrect { get; set; }

    // Navigation
    [ForeignKey(nameof(AttemptId))]
    public virtual ExamAttempt Attempt { get; set; }

    [ForeignKey(nameof(QuestionId))]
    public virtual Question Question { get; set; }
}
