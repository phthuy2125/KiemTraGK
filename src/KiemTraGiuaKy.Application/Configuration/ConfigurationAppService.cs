using Abp.Authorization;
using Abp.Runtime.Session;
using KiemTraGiuaKy.Configuration.Dto;
using System.Threading.Tasks;

namespace KiemTraGiuaKy.Configuration;

[AbpAuthorize]
public class ConfigurationAppService : KiemTraGiuaKyAppServiceBase, IConfigurationAppService
{
    public async Task ChangeUiTheme(ChangeUiThemeInput input)
    {
        await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
    }
}
