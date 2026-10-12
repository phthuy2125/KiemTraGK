using Abp.MultiTenancy;
using KiemTraGiuaKy.Authorization.Users;

namespace KiemTraGiuaKy.MultiTenancy;

public class Tenant : AbpTenant<User>
{
    public Tenant()
    {
    }

    public Tenant(string tenancyName, string name)
        : base(tenancyName, name)
    {
    }
}
