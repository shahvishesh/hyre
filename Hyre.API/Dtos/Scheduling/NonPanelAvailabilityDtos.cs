namespace Hyre.API.Dtos.Scheduling
{
    public record NonPanelAvailabilityRequestDto(
        DateTime Date,
        int DurationMinutes,
        string InterviewerId,
        int CandidateId
    );

    /*public record AvailableSlotDto(
        DateTime Start,
        DateTime End
    );*/
}
