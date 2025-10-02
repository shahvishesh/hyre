namespace Hyre.API.Dtos
{
    public record UpdateJobDto
    {
        public string? Title { get; init; }
        public string? Description { get; init; }
        public int? MinExperience { get; init; }
        public int? MaxExperience { get; init; }
        public string? CompanyName { get; init; }
        public string? Location { get; init; }
        public string? JobType { get; init; }
        public string? WorkplaceType { get; init; }
        public string? Status { get; init; }   // Open, On Hold, Closed
        public string? ClosedReason { get; init; }

        public int? SelectedCandidateID { get; init; }
        public List<JobSkillDto>? Skills { get; init; }  // optional skill update
    }
}
