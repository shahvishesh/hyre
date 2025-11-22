using Hyre.API.Dtos.Scheduling;

namespace Hyre.API.Interfaces.Scheduling
{
    public interface IPanelSchedulingService
    {
        Task<List<AvailableSlotDto>> GetAvailablePanelSlotsAsync(PanelAvailabilityRequestDto request);
    }
}
