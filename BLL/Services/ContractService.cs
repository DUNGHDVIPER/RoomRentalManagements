using System.Text.Json;
using BLL.Common;
using BLL.DTOs.Common;
using BLL.DTOs.Contract;
using BLL.Services.Interfaces;
using DAL.Data;
using DAL.Entities.Common;
using DAL.Entities.Contracts;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class ContractService : IContractService
{
    public Task AddAttachmentStubAsync(int contractId, string fileName, string url, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<ContractDto> CreateAsync(CreateContractDto dto, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task CreateReminderAsync(int contractId, DateTime remindAt, string type, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task CreateVersionSnapshotAsync(int contractId, string changedByUserId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> ExportPdfStubAsync(int contractId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<ContractDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResultDto<ContractDto>> GetContractsAsync(PagedRequestDto req, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<ContractDto> RenewAsync(RenewContractDto dto, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task TerminateAsync(TerminateContractDto dto, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateDepositAsync(int contractId, decimal newDeposit, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    // XONG CHUC NANG CONTRACT THI THEM VAO
//    await _notificationService.BroadcastAsync(
//    new BroadcastNotificationDto
//    {
//        Title = "Hợp đồng sắp hết hạn",
//        Content = "Hợp đồng của bạn sẽ hết hạn trong 7 ngày.",
//        ContractId = contract.Id,
//        SourceType = SourceType.Contract
//});
}
