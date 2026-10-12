using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using KiemTraGiuaKy.MultiTenancy;

namespace KiemTraGiuaKy.Sessions.Dto;

[AutoMapFrom(typeof(Tenant))]
public class TenantLoginInfoDto : EntityDto
{
    public string TenancyName { get; set; }

    public string Name { get; set; }
}
