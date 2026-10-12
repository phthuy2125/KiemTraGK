using KiemTraGiuaKy.Exams.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiemTraGiuaKy.EntityFrameworkCore.Configurations;

/// <summary>
/// EF Core fluent configuration for the PassageItem entity.
/// </summary>
public class PassageItemConfiguration : IEntityTypeConfiguration<PassageItem>
{
    public void Configure(EntityTypeBuilder<PassageItem> builder)
    {
        builder.ToTable("PassageItems");

        // Store TextContent as nvarchar(max) for long passages
        builder.Property(p => p.TextContent)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(PassageItem.MaxImageUrlLength);

        builder.Property(p => p.Type)
            .IsRequired();

        // FK: cascade delete when group is deleted
        builder.HasOne(p => p.QuestionGroup)
            .WithMany(g => g.PassageItems)
            .HasForeignKey(p => p.QuestionGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.QuestionGroupId, p.OrderIndex });
    }
}
