using Abp.Domain.Entities.Auditing;
using KiemTraGiuaKy.Exams.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace KiemTraGiuaKy.Exams.Entities;

/// <summary>
/// Records one learner's attempt at an exam.
/// The server is the source of truth for timing:
///   EndTime = StartTime + DurationMinutes.
/// Answers cannot be saved or submitted after EndTime.
/// </summary>
public class ExamAttempt : FullAuditedEntity<long>
{
    /// <summary>Foreign key to the exam being attempted.</summary>
    public long ExamId { get; set; }

    /// <summary>
    /// The ABP User ID of the learner.
    /// Stored as long to match AbpUser.Id.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>Server-side UTC time when attempt was started.</summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Server-computed deadline = StartTime + DurationMinutes.
    /// Client uses this to display countdown.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// UTC time of actual submission (manual or auto).
    /// Null if still in progress.
    /// </summary>
    public DateTime? SubmittedTime { get; set; }

    /// <summary>Current status of the attempt.</summary>
    public AttemptStatus Status { get; set; } = AttemptStatus.InProgress;

    /// <summary>Graded count of correct answers. Populated on submission.</summary>
    public int CorrectCount { get; set; }

    /// <summary>Graded count of wrong answers. Populated on submission.</summary>
    public int WrongCount { get; set; }

    /// <summary>Count of skipped (unanswered) questions. Populated on submission.</summary>
    public int SkippedCount { get; set; }

    // Navigation
    [ForeignKey(nameof(ExamId))]
    public virtual Exam Exam { get; set; }

    public virtual ICollection<AttemptAnswer> Answers { get; set; }
}
