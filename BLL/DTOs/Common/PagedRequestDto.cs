namespace BLL.DTOs.Common;

public class PagedRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string? SortBy { get; set; }
    public string? Keyword { get; internal set; }
    public int PageIndex { get; internal set; }
}
