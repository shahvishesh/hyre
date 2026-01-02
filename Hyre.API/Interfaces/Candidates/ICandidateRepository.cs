using Hyre.API.Models;

namespace Hyre.API.Interfaces.Candidates
{
    public interface ICandidateRepository
    {
        Task AddCandidateAsync(Candidate candidate);
        Task AddCandidatesAsync(IEnumerable<Candidate> candidates);
        Task<Candidate?> GetCandidateByIdAsync(int candidateId);
        Task UpdateResumePathAsync(int candidateId, string resumePath);

        Task<List<Candidate>> GetAllCandidatesAsync();
        Task<bool> IsCandidateLinkedToJobAsync(int candidateId, int jobId);
 
    }
}
