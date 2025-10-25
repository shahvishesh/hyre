namespace Hyre.API.Dtos.CandidateMatching
{
    public record CandidateMatchDto(
        int CandidateID,
        string FullName,
        string? Email,
        decimal? TotalExperience,
        double MatchScore,
        List<string> MatchedRequiredSkills,
        List<string> MissingRequiredSkills,
        List<string> MatchedPreferredSkills,
        List<string> MissingPreferredSkills
    );
}
