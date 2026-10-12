using Abp.Application.Services;
using KiemTraGiuaKy.Authorization.Accounts.Dto;
using System.Threading.Tasks;

namespace KiemTraGiuaKy.Authorization.Accounts;

public interface IAccountAppService : IApplicationService
{
    Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

    Task<RegisterOutput> Register(RegisterInput input);
}
