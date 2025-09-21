using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyre.API.Models
{
    public class Job
    {
        [Key]
        public int JobID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        public string Description { get; set; }

        // Experience
        public int? MinExperience { get; set; }
        public int? MaxExperience { get; set; }

        [Required]
        [MaxLength(150)]
        public string CompanyName { get; set; }

        [MaxLength(150)]
        public string Location { get; set; }

        [MaxLength(50)]
        public string JobType { get; set; } 

        [MaxLength(50)]
        public string WorkplaceType { get; set; } 

        [MaxLength(20)]
        public string Status { get; set; } = "Open";

        // Foreign Keys
        [Required]
        public int CreatedBy { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public User Creator { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [MaxLength(255)]
        public string ClosedReason { get; set; }

        public int? SelectedCandidateID { get; set; }

        [ForeignKey(nameof(SelectedCandidateID))]
        public Candidate SelectedCandidate { get; set; }
    }
}
