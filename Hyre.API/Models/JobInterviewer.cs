using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyre.API.Models
{
    public class JobInterviewer
    {
        [Key]
        public int JobInterviewerID { get; set; }

        [Required]
        public int JobID { get; set; }

        [Required]
        public string InterviewerID { get; set; } 

        [ForeignKey(nameof(JobID))]
        public Job Job { get; set; }

        [ForeignKey(nameof(InterviewerID))]
        public ApplicationUser Interviewer { get; set; }

        [MaxLength(50)]
        public string? Role { get; set; } // e.g., "Technical", "HR", "Panel"

        [MaxLength(100)]
        public string? SkillArea { get; set; } // e.g., "Backend", "Frontend"

        public bool IsActive { get; set; } = true;

        public string? AssignedBy { get; set; }

        [ForeignKey(nameof(AssignedBy))]
        public ApplicationUser? AssignedByUser { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.Now;
    }
}
