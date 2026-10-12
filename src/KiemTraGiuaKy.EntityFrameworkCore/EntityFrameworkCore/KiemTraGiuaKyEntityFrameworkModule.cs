using Abp.EntityFrameworkCore.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Zero.EntityFrameworkCore;
using KiemTraGiuaKy.EntityFrameworkCore.Seed;

namespace KiemTraGiuaKy.EntityFrameworkCore;

[DependsOn(
    typeof(KiemTraGiuaKyCoreModule),
    typeof(AbpZeroCoreEntityFrameworkCoreModule))]
public class KiemTraGiuaKyEntityFrameworkModule : AbpModule
{
    /* Used it tests to skip dbcontext registration, in order to use in-memory database of EF Core */
    public bool SkipDbContextRegistration { get; set; }

    public bool SkipDbSeed { get; set; }

    public override void PreInitialize()
    {
        if (!SkipDbContextRegistration)
        {
            Configuration.Modules.AbpEfCore().AddDbContext<KiemTraGiuaKyDbContext>(options =>
            {
                if (options.ExistingConnection != null)
                {
                    KiemTraGiuaKyDbContextConfigurer.Configure(options.DbContextOptions, options.ExistingConnection);
                }
                else
                {
                    KiemTraGiuaKyDbContextConfigurer.Configure(options.DbContextOptions, options.ConnectionString);
                }
            });
        }
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(KiemTraGiuaKyEntityFrameworkModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        if (!SkipDbSeed)
        {
            SeedHelper.SeedHostDb(IocManager);
        }
    }
}
