using BLL.DTOs.Billing;
using BLL.Common;
using BLL.DTOs.Common;

namespace BLL.Services.Interfaces;

public interface IBillingService
{
    Task GenerateBillsBatchAsync(GenerateBillsRequestDto req, CancellationToken ct = default); // tháng đã có bill không tạo trùng
    Task<BillDto> GetBillDetailAsync(int billId, CancellationToken ct = default);
    Task<PagedResultDto<BillDto>> GetBillsAsync(PagedRequestDto req, CancellationToken ct = default);

    Task RecordPaymentAsync(RecordPaymentDto dto, CancellationToken ct = default);
    Task UpdateBillStatusAsync(int billId, int status, CancellationToken ct = default);

    // Preview wizard stub
    Task<BillDto> PreviewBillStubAsync(int contractId, int period, CancellationToken ct = default);
}
