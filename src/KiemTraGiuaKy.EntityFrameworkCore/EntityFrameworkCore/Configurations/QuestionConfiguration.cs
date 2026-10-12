using KiemTraGiuaKy.Exams.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiemTraGiuaKy.EntityFrameworkCore.Configurations;

/// <summary>
/// EF Core fluent configuration for the Question entity.
/// 
/// Key constraint: (ExamId, QuestionNumber) must be unique — enforced by unique index.
/// </summary>
public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");

        builder.Property(q => q.Content)
            .HasMaxLength(Question.MaxContentLength);

        builder.Property(q => q.Explanation)
            .HasMaxLength(Question.MaxExplanationLength);

        builder.Property(q => q.CorrectOption)
            .IsRequired();

        // Unique constraint: no duplicate question number within an exam
        builder.HasIndex(q => new { q.ExamId, q.QuestionNumber })
            .IsUnique();

        // FK to Exam: cascade delete
        builder.HasOne(q => q.Exam)
            .WithMany(e => e.Questions)
            .HasForeignKey(q => q.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK to QuestionGroup: nullable (Part 5 questions have no group)
        // Use Restrict to avoid cascade path conflict (Exam already cascades)
        builder.HasOne(q => q.QuestionGroup)
            .WithMany(g => g.Questions)
            .HasForeignKey(q => q.QuestionGroupId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
