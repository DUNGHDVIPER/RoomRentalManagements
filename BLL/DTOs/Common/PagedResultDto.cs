using BLL.DTOs.Contract;
using BLL.DTOs.Room.Public;

namespace BLL.Common;

public class PagedResultDto<T>
{
    private List<ContractDto> items;
    private int total;
    private int page;
    private List<RoomPublicDto> items1;

    public PagedResultDto(List<ContractDto> items, int total, int page, int pageSize)
    {
        this.items = items;
        this.total = total;
        this.page = page;
        PageSize = pageSize;
    }

    public PagedResultDto(List<RoomPublicDto> items1, int total, int pageNumber, int pageSize)
    {
        this.items1 = items1;
        this.total = total;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int PageIndex { get; internal set; }
}
