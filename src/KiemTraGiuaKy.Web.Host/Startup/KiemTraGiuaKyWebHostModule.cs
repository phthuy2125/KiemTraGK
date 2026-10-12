using Abp.Modules;
using Abp.Reflection.Extensions;
using KiemTraGiuaKy.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace KiemTraGiuaKy.Web.Host.Startup
{
    [DependsOn(
       typeof(KiemTraGiuaKyWebCoreModule))]
    public class KiemTraGiuaKyWebHostModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public KiemTraGiuaKyWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(KiemTraGiuaKyWebHostModule).GetAssembly());
        }
    }
}
