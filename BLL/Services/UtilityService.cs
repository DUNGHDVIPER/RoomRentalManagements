using BLL.DTOs.Utility;
using BLL.Services.Interfaces;

namespace BLL.Services;

public class UtilityService : IUtilityService
{
    public Task<UtilityPriceDto> GetCurrentPriceAsync(CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task SetPriceAsync(UtilityPriceDto dto, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task BulkUpsertReadingsAsync(BulkUtilityReadingDto dto, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<UtilityChargeResultDto> CalculateChargesAsync(int roomId, int period, CancellationToken ct = default)
        => throw new NotImplementedException();
}
