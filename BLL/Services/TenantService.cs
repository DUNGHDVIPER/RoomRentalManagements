using BLL.DTOs.Tenant;
using BLL.DTOs.Common;
using BLL.Common;
using BLL.Services.Interfaces;

namespace BLL.Services;

public class TenantService : ITenantService
{
    public Task<PagedResultDto<TenantDto>> GetTenantsAsync(PagedRequestDto req, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<TenantDto> GetByIdAsync(int id, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<TenantDto> CreateAsync(CreateTenantDto dto, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<TenantDto> UpdateAsync(int id, UpdateTenantDto dto, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task DeleteAsync(int id, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<List<TenantIdDocDto>> GetIdDocsAsync(int tenantId, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<TenantIdDocDto> AddIdDocAsync(int tenantId, TenantIdDocDto dto, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task RemoveIdDocAsync(int docId, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task BlacklistAsync(int tenantId, string reason, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task UnBlacklistAsync(int tenantId, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<List<StayHistoryDto>> GetStayHistoryAsync(int tenantId, CancellationToken ct = default)
        => throw new NotImplementedException();
}
