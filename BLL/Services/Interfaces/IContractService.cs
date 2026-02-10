using BLL.Common;
using BLL.DTOs.Common;
using BLL.DTOs.Contract;

namespace BLL.Services.Interfaces;

public interface IContractService
{
    Task<ContractDto> CreateAsync(CreateContractDto dto, CancellationToken ct = default);
    Task<ContractDto> RenewAsync(RenewContractDto dto, CancellationToken ct = default);
    Task TerminateAsync(TerminateContractDto dto, CancellationToken ct = default);

    Task<PagedResultDto<ContractDto>> GetContractsAsync(PagedRequestDto req, CancellationToken ct = default);
    Task<ContractDto> GetByIdAsync(int id, CancellationToken ct = default);

    // Deposit + attachment + versioning + reminders
    Task UpdateDepositAsync(int contractId, decimal newDeposit, CancellationToken ct = default);
    Task AddAttachmentStubAsync(int contractId, string fileName, string url, CancellationToken ct = default);
    Task CreateVersionSnapshotAsync(int contractId, string changedByUserId, CancellationToken ct = default);
    Task CreateReminderAsync(int contractId, DateTime remindAt, string type, CancellationToken ct = default);

    // Export stubs
    Task<byte[]> ExportPdfStubAsync(int contractId, CancellationToken ct = default);
}
