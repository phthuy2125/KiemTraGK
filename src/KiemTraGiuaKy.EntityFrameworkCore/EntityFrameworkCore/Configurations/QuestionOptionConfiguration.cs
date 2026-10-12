using KiemTraGiuaKy.Exams.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiemTraGiuaKy.EntityFrameworkCore.Configurations;

/// <summary>
/// EF Core fluent configuration for the QuestionOption entity.
/// </summary>
public class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.ToTable("QuestionOptions");

        builder.Property(o => o.Content)
            .IsRequired()
            .HasMaxLength(QuestionOption.MaxContentLength);

        builder.Property(o => o.Label)
            .IsRequired();

        // FK: cascade delete when question is deleted
        builder.HasOne(o => o.Question)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Each question has exactly one option per label — uniqueness enforced
        builder.HasIndex(o => new { o.QuestionId, o.Label })
            .IsUnique();
    }
}
