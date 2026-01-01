using Hyre.API.Dtos.RecruiterRoundDecesion;

namespace Hyre.API.Interfaces.RecruiterFeedback
{
    public interface IRecruiterDecisionService
    {
        Task ApplyDecisionAsync(
            RecruiterDecisionDto dto, string recruiterId);
    }
}
