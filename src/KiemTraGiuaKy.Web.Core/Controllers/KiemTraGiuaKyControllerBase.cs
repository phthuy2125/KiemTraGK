using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace KiemTraGiuaKy.Controllers
{
    public abstract class KiemTraGiuaKyControllerBase : AbpController
    {
        protected KiemTraGiuaKyControllerBase()
        {
            LocalizationSourceName = KiemTraGiuaKyConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
