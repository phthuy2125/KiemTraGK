using KiemTraGiuaKy.Exams.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiemTraGiuaKy.EntityFrameworkCore.Configurations;

/// <summary>
/// EF Core fluent configuration for the QuestionGroup entity.
/// </summary>
public class QuestionGroupConfiguration : IEntityTypeConfiguration<QuestionGroup>
{
    public void Configure(EntityTypeBuilder<QuestionGroup> builder)
    {
        builder.ToTable("QuestionGroups");

        builder.Property(g => g.Instruction)
            .HasMaxLength(QuestionGroup.MaxInstructionLength);

        // FK: cascade delete when exam is deleted
        builder.HasOne(g => g.Exam)
            .WithMany(e => e.QuestionGroups)
            .HasForeignKey(g => g.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for ordering groups within an exam part
        builder.HasIndex(g => new { g.ExamId, g.PartNumber, g.OrderIndex });
    }
}
