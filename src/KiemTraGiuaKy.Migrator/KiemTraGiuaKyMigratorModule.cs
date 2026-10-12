using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using KiemTraGiuaKy.Configuration;
using KiemTraGiuaKy.EntityFrameworkCore;
using KiemTraGiuaKy.Migrator.DependencyInjection;
using Castle.MicroKernel.Registration;
using Microsoft.Extensions.Configuration;

namespace KiemTraGiuaKy.Migrator;

[DependsOn(typeof(KiemTraGiuaKyEntityFrameworkModule))]
public class KiemTraGiuaKyMigratorModule : AbpModule
{
    private readonly IConfigurationRoot _appConfiguration;

    public KiemTraGiuaKyMigratorModule(KiemTraGiuaKyEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbSeed = true;

        _appConfiguration = AppConfigurations.Get(
            typeof(KiemTraGiuaKyMigratorModule).GetAssembly().GetDirectoryPathOrNull()
        );
    }

    public override void PreInitialize()
    {
        Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
            KiemTraGiuaKyConsts.ConnectionStringName
        );

        Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
        Configuration.ReplaceService(
            typeof(IEventBus),
            () => IocManager.IocContainer.Register(
                Component.For<IEventBus>().Instance(NullEventBus.Instance)
            )
        );
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(KiemTraGiuaKyMigratorModule).GetAssembly());
        ServiceCollectionRegistrar.Register(IocManager);
    }
}
