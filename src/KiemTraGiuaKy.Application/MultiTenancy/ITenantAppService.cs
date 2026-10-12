using Abp.Application.Services;
using KiemTraGiuaKy.MultiTenancy.Dto;

namespace KiemTraGiuaKy.MultiTenancy;

public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
{
}

