namespace Hyre.API.Dtos.CandidateMatching
{
    public record MatchResultDto(
        int JobID,
        string JobTitle,
        List<CandidateMatchDto> Matches
    );
}
