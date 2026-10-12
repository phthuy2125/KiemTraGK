using Abp.Application.Services;
using KiemTraGiuaKy.Sessions.Dto;
using System.Threading.Tasks;

namespace KiemTraGiuaKy.Sessions;

public interface ISessionAppService : IApplicationService
{
    Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
}
