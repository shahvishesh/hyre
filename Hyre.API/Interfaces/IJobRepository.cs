using Hyre.API.Models;

namespace Hyre.API.Interfaces
{
    public interface IJobRepository
    {
        Task<Job> AddAsync(Job job);
        Task<Job?> GetByIdAsync(int jobId);
        Task<IEnumerable<Job>> GetAllAsync();
        Task UpdateAsync(Job job);
        Task DeleteAsync(int jobId);
    }
}
