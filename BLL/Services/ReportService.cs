using BLL.DTOs.Report;
using BLL.Services.Interfaces;

namespace BLL.Services;

public class ReportService : IReportService
{
    public Task<RevenueReportDto> GetRevenueDashboardAsync(int year, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<ProfitReportDto> GetProfitDashboardAsync(int year, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<byte[]> ExportExcelStubAsync(string reportName, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<byte[]> ExportPdfStubAsync(string reportName, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task GetRevenueAsync()
    {
        throw new NotImplementedException();
    }
}
