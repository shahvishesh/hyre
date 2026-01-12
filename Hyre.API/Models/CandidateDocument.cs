using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyre.API.Models
{
    public class CandidateDocument
    {
        [Key]
        public int DocumentId { get; set; }
        public int VerificationId { get; set; }
        public int DocumentTypeId { get; set; }

        [ForeignKey(nameof(VerificationId))]
        public CandidateDocumentVerification Verification { get; set; }

        [ForeignKey(nameof(DocumentTypeId))]
        public DocumentType DocumentType { get; set; }

        // File info
        [Required]
        public string FilePath { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Status { get; set; } = "NotUploaded";
        // Uploaded, Approved, ReuploadRequired, Rejected

        public DateTime? UploadedAt { get; set; }

        public int UploadedBy { get; set; }
        // CandidateId

        public DateTime? VerifiedAt { get; set; }

        public string? VerifiedBy { get; set; }
        // AspNetUsers.Id (HR)

        public string? HrComment { get; set; }

        public int Version { get; set; } = 1;
    }
}
