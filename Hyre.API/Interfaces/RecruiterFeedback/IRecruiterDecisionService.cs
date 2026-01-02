using Hyre.API.Dtos.RecruiterRoundDecesion;

namespace Hyre.API.Interfaces.RecruiterFeedback
{
    public interface IRecruiterDecisionService
    {
        Task ApplyDecisionAsync(
            RecruiterRoundDecisionDto dto, string recruiterId);
    }
}
