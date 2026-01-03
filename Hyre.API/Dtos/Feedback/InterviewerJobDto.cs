namespace Hyre.API.Dtos.Feedback
{
    public record InterviewerJobDto(
        int JobID,
        string Title,
        string Description,
        string CompanyName,
        string Location,
        string JobType,
        string WorkplaceType,
        string Status,
        int? MinExperience,
        int? MaxExperience,
        DateTime CreatedAt
    );
}