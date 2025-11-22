namespace Hyre.API.Dtos.Scheduling
{
    public record PanelAvailabilityRequestDto(
        DateTime Date,                  
        int DurationMinutes,            
        List<string> InterviewerIds,    
        int CandidateId                 
    );

    public record AvailableSlotDto(
        DateTime Start,
        DateTime End
    );
}
