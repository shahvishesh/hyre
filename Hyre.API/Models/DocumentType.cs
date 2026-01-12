using System.ComponentModel.DataAnnotations;

namespace Hyre.API.Models
{
    public class DocumentType
    {
        [Key]
        public int DocumentTypeId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;
        // 10th Marksheet, Aadhar, Passport, etc.

        public bool IsMandatory { get; set; }

        [MaxLength(100)]
        public string AllowedFormats { get; set; } = "pdf,jpg,png";

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }

        public ICollection<CandidateDocument> CandidateDocuments { get; set; }
            = new List<CandidateDocument>();
    }
}
