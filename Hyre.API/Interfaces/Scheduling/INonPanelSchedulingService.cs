using Hyre.API.Dtos.Scheduling;

namespace Hyre.API.Interfaces.Scheduling
{
    public interface INonPanelSchedulingService
    {
        Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(NonPanelAvailabilityRequestDto request);

    }
}
