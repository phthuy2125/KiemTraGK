using Abp.Zero.EntityFrameworkCore;
using KiemTraGiuaKy.Authorization.Roles;
using KiemTraGiuaKy.Authorization.Users;
using KiemTraGiuaKy.Exams.Entities;
using KiemTraGiuaKy.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace KiemTraGiuaKy.EntityFrameworkCore;

public class KiemTraGiuaKyDbContext : AbpZeroDbContext<Tenant, Role, User, KiemTraGiuaKyDbContext>
{
    // ── TOEIC Entities ──────────────────────────────────────────────────────
    public DbSet<Exam> Exams { get; set; }
    public DbSet<QuestionGroup> QuestionGroups { get; set; }
    public DbSet<PassageItem> PassageItems { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionOption> QuestionOptions { get; set; }
    public DbSet<ExamAttempt> ExamAttempts { get; set; }
    public DbSet<AttemptAnswer> AttemptAnswers { get; set; }

    public KiemTraGiuaKyDbContext(DbContextOptions<KiemTraGiuaKyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration<T> classes in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KiemTraGiuaKyDbContext).Assembly);
    }
}

