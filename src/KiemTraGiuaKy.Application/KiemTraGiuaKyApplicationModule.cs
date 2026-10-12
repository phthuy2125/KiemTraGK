using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using KiemTraGiuaKy.Authorization;

namespace KiemTraGiuaKy;

[DependsOn(
    typeof(KiemTraGiuaKyCoreModule),
    typeof(AbpAutoMapperModule))]
public class KiemTraGiuaKyApplicationModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Authorization.Providers.Add<KiemTraGiuaKyAuthorizationProvider>();
    }

    public override void Initialize()
    {
        var thisAssembly = typeof(KiemTraGiuaKyApplicationModule).GetAssembly();

        IocManager.RegisterAssemblyByConvention(thisAssembly);

        Configuration.Modules.AbpAutoMapper().Configurators.Add(
            // Scan the assembly for classes which inherit from AutoMapper.Profile
            cfg => cfg.AddMaps(thisAssembly)
        );
    }
}
