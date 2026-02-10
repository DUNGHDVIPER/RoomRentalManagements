using BLL.DTOs.Billing;
using BLL.DTOs.Common;
using BLL.Common;
using BLL.Services.Interfaces;

namespace BLL.Services;

public class BillingService : IBillingService
{
    public BillingService()
    {
    }

    public Task GenerateBillsBatchAsync(GenerateBillsRequestDto req, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<BillDto> GetBillDetailAsync(int billId, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<PagedResultDto<BillDto>> GetBillsAsync(PagedRequestDto req, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task RecordPaymentAsync(RecordPaymentDto dto, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task UpdateBillStatusAsync(int billId, int status, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<BillDto> PreviewBillStubAsync(int contractId, int period, CancellationToken ct = default)
        => throw new NotImplementedException();
}
