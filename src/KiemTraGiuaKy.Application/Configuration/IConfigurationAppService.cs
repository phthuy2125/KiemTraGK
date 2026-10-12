using KiemTraGiuaKy.Configuration.Dto;
using System.Threading.Tasks;

namespace KiemTraGiuaKy.Configuration;

public interface IConfigurationAppService
{
    Task ChangeUiTheme(ChangeUiThemeInput input);
}
