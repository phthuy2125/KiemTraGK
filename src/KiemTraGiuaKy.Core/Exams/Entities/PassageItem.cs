using Abp.Domain.Entities.Auditing;
using KiemTraGiuaKy.Exams.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KiemTraGiuaKy.Exams.Entities;

/// <summary>
/// A single component of a passage — either a text block or an image.
/// Supports multi-component passages in Part 7 (e.g., email + chart + article).
/// Multiple PassageItems are ordered by OrderIndex within the same group.
/// </summary>
public class PassageItem : FullAuditedEntity<long>
{
    public const int MaxImageUrlLength = 512;

    /// <summary>Foreign key to the parent question group.</summary>
    public long QuestionGroupId { get; set; }

    /// <summary>Display order within the group's passage.</summary>
    public int OrderIndex { get; set; }

    /// <summary>Whether this component is text or an image.</summary>
    public PassageItemType Type { get; set; }

    /// <summary>
    /// The text content of this passage component.
    /// For Part 6, contains blanks marked as "-------131" etc.
    /// Stored as nvarchar(max) to support long passages.
    /// Null if Type = Image.
    /// </summary>
    public string TextContent { get; set; }

    /// <summary>
    /// URL path to the image file (relative to wwwroot).
    /// E.g., "/uploads/exams/5/img001.png".
    /// Null if Type = Text.
    /// </summary>
    [MaxLength(MaxImageUrlLength)]
    public string ImageUrl { get; set; }

    // Navigation
    [ForeignKey(nameof(QuestionGroupId))]
    public virtual QuestionGroup QuestionGroup { get; set; }
}
