using KiemTraGiuaKy.Exams.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiemTraGiuaKy.EntityFrameworkCore.Configurations;

/// <summary>
/// EF Core fluent configuration for the Exam entity.
/// </summary>
public class ExamConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.ToTable("Exams");

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(Exam.MaxTitleLength);

        builder.Property(e => e.Description)
            .HasMaxLength(Exam.MaxDescriptionLength);

        builder.Property(e => e.DurationMinutes)
            .HasDefaultValue(75);

        builder.Property(e => e.IsPublished)
            .HasDefaultValue(false);

        // Index on title for fast lookups
        builder.HasIndex(e => e.Title);
    }
}
