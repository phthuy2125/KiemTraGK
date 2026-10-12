using Abp.Domain.Entities.Auditing;
using KiemTraGiuaKy.Exams.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KiemTraGiuaKy.Exams.Entities;

/// <summary>
/// Represents a single TOEIC question (Q101–Q200).
/// 
/// Part 5 (Q101–130): QuestionGroupId = null. Content is required (the full sentence).
/// Part 6 (Q131–146): QuestionGroupId set. Content is nullable because the blank is
///   embedded in the passage text; only the question number appears in the Word file.
/// Part 7 (Q147–200): QuestionGroupId set. Content contains the actual question text.
/// </summary>
public class Question : FullAuditedEntity<long>
{
    public const int MaxContentLength = 2000;
    public const int MaxExplanationLength = 2000;

    /// <summary>Foreign key to the parent exam (denormalized for fast queries).</summary>
    public long ExamId { get; set; }

    /// <summary>
    /// Foreign key to the question group.
    /// NULL for Part 5 standalone questions.
    /// </summary>
    public long? QuestionGroupId { get; set; }

    /// <summary>TOEIC part number: 5, 6, or 7.</summary>
    public int PartNumber { get; set; }

    /// <summary>
    /// Official TOEIC question number (101–200).
    /// Unique within an exam (enforced by unique index).
    /// </summary>
    public int QuestionNumber { get; set; }

    /// <summary>
    /// The question text.
    /// For Part 5: the full sentence with "-------" blank.
    /// For Part 6: null (the question is just the number; blank is in the passage).
    /// For Part 7: the actual question text.
    /// </summary>
    [MaxLength(MaxContentLength)]
    public string Content { get; set; }

    /// <summary>The correct answer option (A, B, C, or D).</summary>
    public AnswerOption CorrectOption { get; set; }

    /// <summary>Optional explanation shown after submission.</summary>
    [MaxLength(MaxExplanationLength)]
    public string Explanation { get; set; }

    // Navigation
    [ForeignKey(nameof(ExamId))]
    public virtual Exam Exam { get; set; }

    [ForeignKey(nameof(QuestionGroupId))]
    public virtual QuestionGroup QuestionGroup { get; set; }

    public virtual ICollection<QuestionOption> Options { get; set; }
    public virtual ICollection<AttemptAnswer> AttemptAnswers { get; set; }
}
