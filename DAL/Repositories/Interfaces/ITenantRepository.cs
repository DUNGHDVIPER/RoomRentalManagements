using DAL.Entities.Tenanting;

namespace DAL.Repositories
{
    public interface ITenantRepository
    {
        Task<List<Tenant>> GetAllAsync();
        Task<Tenant?> GetByIdAsync(int id);
        Task AddAsync(Tenant tenant);
        Task UpdateAsync(Tenant tenant);
        Task DeleteAsync(int id);
    }
}