using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyre.API.Models
{
    public class CandidateInterviewRound
    {
        [Key]
        public int CandidateRoundID { get; set; }

        [Required]
        public int CandidateID { get; set; }

        [ForeignKey(nameof(CandidateID))]
        public Candidate Candidate { get; set; }

        [Required]
        public int JobID { get; set; }

        [ForeignKey(nameof(JobID))]
        public Job Job { get; set; }

        [Required]
        public int SequenceNo { get; set; } // round order

        [Required, MaxLength(100)]
        public string RoundName { get; set; }

        [MaxLength(50)]
        public string RoundType { get; set; } // "Technical", "HR", "Panel"

        [Required]
        public string RecruiterID { get; set; }

        [ForeignKey(nameof(RecruiterID))]
        public ApplicationUser Recruiter { get; set; }

        public string? InterviewerID { get; set; } // For non-panel rounds

        [ForeignKey(nameof(InterviewerID))]
        public ApplicationUser? Interviewer { get; set; }

        public DateTime? ScheduledDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        public int? DurationMinutes { get; set; }

        [MaxLength(50)]
        public string? InterviewMode { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Scheduled, Completed, Cancelled

        [MaxLength(255)]
        public string? MeetingLink { get; set; }

        public bool IsPanelRound { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<CandidatePanelMember> PanelMembers { get; set; } = new List<CandidatePanelMember>();

        public ICollection<CandidateInterviewFeedback> Feedbacks { get; set; }

    }
}
