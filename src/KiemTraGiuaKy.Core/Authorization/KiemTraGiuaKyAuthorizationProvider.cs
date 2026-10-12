using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace KiemTraGiuaKy.Authorization;

public class KiemTraGiuaKyAuthorizationProvider : AuthorizationProvider
{
    public override void SetPermissions(IPermissionDefinitionContext context)
    {
        context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
        context.CreatePermission(PermissionNames.Pages_Users_Activation, L("UsersActivation"));
        context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
        context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);

        // ── TOEIC Exam Management ──────────────────────────────────────────
        var examsPermission = context.CreatePermission(PermissionNames.Pages_Exams, L("Exams"));
        examsPermission.CreateChildPermission(PermissionNames.Pages_Exams_Import, L("ExamsImport"));
        examsPermission.CreateChildPermission(PermissionNames.Pages_Exams_Edit, L("ExamsEdit"));
        examsPermission.CreateChildPermission(PermissionNames.Pages_Exams_Publish, L("ExamsPublish"));
        examsPermission.CreateChildPermission(PermissionNames.Pages_Exams_Delete, L("ExamsDelete"));
    }

    private static ILocalizableString L(string name)
    {
        return new LocalizableString(name, KiemTraGiuaKyConsts.LocalizationSourceName);
    }
}
