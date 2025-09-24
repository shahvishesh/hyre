using Hyre.API.Models;

namespace Hyre.API.Interfaces
{
    public interface IJobRepository
    {
        Task<IEnumerable<Job>> GetAllAsync();
        Task<Job> GetByIdAsync(int jobId);
        Task AddAsync(Job job);
        Task UpdateAsync(Job job);
        Task DeleteAsync(int jobId);
    }
}
