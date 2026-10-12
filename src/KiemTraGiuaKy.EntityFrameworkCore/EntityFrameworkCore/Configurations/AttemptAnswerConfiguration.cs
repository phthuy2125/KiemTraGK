using KiemTraGiuaKy.Exams.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiemTraGiuaKy.EntityFrameworkCore.Configurations;

/// <summary>
/// EF Core fluent configuration for the AttemptAnswer entity.
/// </summary>
public class AttemptAnswerConfiguration : IEntityTypeConfiguration<AttemptAnswer>
{
    public void Configure(EntityTypeBuilder<AttemptAnswer> builder)
    {
        builder.ToTable("AttemptAnswers");

        // SelectedOption is nullable (null = skipped)
        builder.Property(a => a.SelectedOption)
            .IsRequired(false);

        // IsCorrect is nullable until grading
        builder.Property(a => a.IsCorrect)
            .IsRequired(false);

        // FK to Attempt: cascade delete
        builder.HasOne(a => a.Attempt)
            .WithMany(att => att.Answers)
            .HasForeignKey(a => a.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK to Question: restrict (keep answers even if question is hidden)
        builder.HasOne(a => a.Question)
            .WithMany(q => q.AttemptAnswers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique: one answer row per question per attempt
        builder.HasIndex(a => new { a.AttemptId, a.QuestionId })
            .IsUnique();
    }
}
