using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyre.API.Models
{
    public class CandidateDocumentVerification
    {
        [Key]
        public int VerificationId { get; set; }
        public int CandidateId { get; set; }
        public int JobId { get; set; }

        [ForeignKey(nameof(CandidateId))]
        public Candidate Candidate { get; set; }

        [ForeignKey(nameof(JobId))]
        public Job Job { get; set; }

        // Overall status
        [MaxLength(50)]
        public string Status { get; set; } = "ActionRequired";
        // ActionRequired, ReuploadRequired, UnderVerification, Completed

        public DateTime Deadline { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        [MaxLength(50)]
        public string? FinalDecision { get; set; }
        // Accepted, Rejected

        public string? HrComment { get; set; }

        // Navigation
        public ICollection<CandidateDocument> Documents { get; set; }
            = new List<CandidateDocument>();
    }
}
