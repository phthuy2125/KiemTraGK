using Abp.Domain.Entities.Auditing;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KiemTraGiuaKy.Exams.Entities;

/// <summary>
/// Represents a passage group in Part 6 or Part 7.
/// 
/// Design decision for Part 5:
/// Each Part 5 question (Q101-130) is STANDALONE (no passage), so we do NOT
/// force them into a group. QuestionGroupId on Question is nullable.
/// Part 5 questions have GroupId = null. This keeps the model clean and avoids
/// creating 30 dummy groups with no passage content.
/// 
/// Part 6: groups of 4 questions, sharing a single passage with blanks.
/// Part 7: groups of 2-5 questions, sharing potentially multi-component passages.
/// </summary>
public class QuestionGroup : FullAuditedEntity<long>
{
    public const int MaxInstructionLength = 512;
    public const int MaxTitleLength = 256;

    /// <summary>Foreign key to the parent exam.</summary>
    public long ExamId { get; set; }

    /// <summary>TOEIC part number: 6 or 7 (Part 5 questions have no group).</summary>
    public int PartNumber { get; set; }

    /// <summary>Display order within the exam part.</summary>
    public int OrderIndex { get; set; }

    /// <summary>
    /// Optional title, e.g. "Questions 131-134 refer to the following advertisement."
    /// This is what the learner sees above the passage.
    /// </summary>
    [MaxLength(MaxInstructionLength)]
    public string Instruction { get; set; }

    /// <summary>
    /// First question number in this group (e.g., 131).
    /// Allows quick lookup and validation.
    /// </summary>
    public int FromQuestionNumber { get; set; }

    /// <summary>
    /// Last question number in this group (e.g., 134).
    /// </summary>
    public int ToQuestionNumber { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ExamId))]
    public virtual Exam Exam { get; set; }

    public virtual ICollection<PassageItem> PassageItems { get; set; }
    public virtual ICollection<Question> Questions { get; set; }
}
