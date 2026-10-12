using KiemTraGiuaKy.Exams.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiemTraGiuaKy.EntityFrameworkCore.Configurations;

/// <summary>
/// EF Core fluent configuration for the ExamAttempt entity.
/// </summary>
public class ExamAttemptConfiguration : IEntityTypeConfiguration<ExamAttempt>
{
    public void Configure(EntityTypeBuilder<ExamAttempt> builder)
    {
        builder.ToTable("ExamAttempts");

        builder.Property(a => a.Status)
            .IsRequired();

        builder.Property(a => a.StartTime)
            .IsRequired();

        builder.Property(a => a.EndTime)
            .IsRequired();

        // FK to Exam: restrict delete (don't lose attempt history if exam is deleted)
        builder.HasOne(a => a.Exam)
            .WithMany(e => e.Attempts)
            .HasForeignKey(a => a.ExamId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index: find all attempts by a user
        builder.HasIndex(a => a.UserId);

        // Index: find all attempts for an exam
        builder.HasIndex(a => new { a.ExamId, a.UserId });
    }
}
