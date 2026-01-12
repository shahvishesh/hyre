using DocumentFormat.OpenXml.Spreadsheet;
using Hyre.API.Models;
using System.Runtime.Intrinsics.X86;

namespace Hyre.API.Dtos.DocumentVerification
{
    public class DocumentVerificationDtos
    {
        public record RequiredDocumentDto(
            int DocumentTypeId,
            string DocumentName,
            bool IsMandatory,
            string AllowedFormats,
            int DisplayOrder,

            // Candidate-specific
            string Status,            // NotUploaded, Uploaded, Approved, ReuploadRequired
            string? UploadedFilePath
        );

        public record UploadDocumentDto(int JobId, int DocumentTypeId, IFormFile File);

        public record UploadResponseDto(
            bool Success,
            string Message,
            int DocumentTypeId,
            string Status
        );

        public record ApiResponse(
            bool Success,
            string Message
        );

        public record SubmitForVerificationDto(int JobId);

        public record DocumentJobDto(
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
            DateTime CreatedAt,
            int PendingProfilesCount
        );

        public record CandidateDetailDto(
        int CandidateID,
        int verificationID,
        string FirstName,
        string? LastName,
        string? Email,
        string? Phone,
        decimal? ExperienceYears,
        string? ResumePath,
        string Status,
        List<CandidateSkillDto> Skills
    );

        public record CandidateSkillDto(
            int SkillID,
            string SkillName,
            decimal? YearsOfExperience
        );

        public record HrDocumentDto(
            int DocumentId,
            int DocumentTypeId,
            string DocumentName,
            string Status,          // Uploaded, Approved, ReuploadRequired, Rejected
            string FilePath,
            DateTime? UploadedAt,
            int UploadedBy,         // CandidateId
            DateTime? VerifiedAt,
            string? VerifiedBy,     // HR UserId
            string? HrComment
        );

        public record HrVerificationDetailDto(
            int VerificationId,
            string VerificationStatus,   // UnderVerification, ReuploadRequired
            List<HrDocumentDto> Documents
        );

        public record HrVerificationActionDto(
            int VerificationId,
            string Action,          // Approve | Reupload | Reject
            string? Comment,
            List<HrDocumentActionDto>? Documents
        );

        public record HrDocumentActionDto(
            int DocumentTypeId,
            string Status           // Approved | ReuploadRequired | Rejected
        );

    }
}
