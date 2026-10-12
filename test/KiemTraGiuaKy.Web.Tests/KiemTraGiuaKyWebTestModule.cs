using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using KiemTraGiuaKy.EntityFrameworkCore;
using KiemTraGiuaKy.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace KiemTraGiuaKy.Web.Tests;

[DependsOn(
    typeof(KiemTraGiuaKyWebMvcModule),
    typeof(AbpAspNetCoreTestBaseModule)
)]
public class KiemTraGiuaKyWebTestModule : AbpModule
{
    public KiemTraGiuaKyWebTestModule(KiemTraGiuaKyEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
    }

    public override void PreInitialize()
    {
        Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(KiemTraGiuaKyWebTestModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        IocManager.Resolve<ApplicationPartManager>()
            .AddApplicationPartsIfNotAddedBefore(typeof(KiemTraGiuaKyWebMvcModule).Assembly);
    }
}