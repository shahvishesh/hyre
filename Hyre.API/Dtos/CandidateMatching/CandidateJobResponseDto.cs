namespace Hyre.API.Dtos.CandidateMatching
{
    public record CandidateJobResponseDto(
        int CandidateJobID,
        int CandidateID,
        int JobID,
        string Stage,
        DateTime CreatedAt
    );
}
