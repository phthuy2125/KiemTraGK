namespace KiemTraGiuaKy.Authorization;

public static class PermissionNames
{
    public const string Pages_Tenants = "Pages.Tenants";

    public const string Pages_Users = "Pages.Users";
    public const string Pages_Users_Activation = "Pages.Users.Activation";

    public const string Pages_Roles = "Pages.Roles";

    // ── TOEIC Exam Management ──────────────────────────────────────────────
    /// <summary>Top-level: can view the exam list page.</summary>
    public const string Pages_Exams = "Pages.Exams";

    /// <summary>Can import exams from .docx files.</summary>
    public const string Pages_Exams_Import = "Pages.Exams.Import";

    /// <summary>Can edit exam content (title, passage, questions, options).</summary>
    public const string Pages_Exams_Edit = "Pages.Exams.Edit";

    /// <summary>Can publish / unpublish an exam.</summary>
    public const string Pages_Exams_Publish = "Pages.Exams.Publish";

    /// <summary>Can delete an exam.</summary>
    public const string Pages_Exams_Delete = "Pages.Exams.Delete";
}
