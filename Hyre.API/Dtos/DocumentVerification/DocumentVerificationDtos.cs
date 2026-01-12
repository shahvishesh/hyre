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
    }
}
