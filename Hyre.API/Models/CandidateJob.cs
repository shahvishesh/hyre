using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyre.API.Models
{
    public class CandidateJob
    {
        [Key]
        public int CandidateJobID { get; set; }

        [Required]
        public int CandidateID { get; set; }

        [Required]
        public int JobID { get; set; }

        [MaxLength(50)]
        public string Stage { get; set; } = "Screening";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Required]
        public string? CreatedBy { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public ApplicationUser? CreatedByUser { get; set; }

        [ForeignKey(nameof(CandidateID))]
        public Candidate? Candidate { get; set; }

        [ForeignKey(nameof(JobID))]
        public Job? Job { get; set; }
    }

}
