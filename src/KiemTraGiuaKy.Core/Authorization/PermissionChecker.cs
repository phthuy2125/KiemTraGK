using Abp.Authorization;
using KiemTraGiuaKy.Authorization.Roles;
using KiemTraGiuaKy.Authorization.Users;

namespace KiemTraGiuaKy.Authorization;

public class PermissionChecker : PermissionChecker<Role, User>
{
    public PermissionChecker(UserManager userManager)
        : base(userManager)
    {
    }
}
