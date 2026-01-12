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

    }
}
