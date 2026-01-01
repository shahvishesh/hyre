using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyre.API.Models
{
    public class CandidateInterviewFeedback
    {
        [Key]
        public int FeedbackID { get; set; }

        [Required]
        public int CandidateRoundID { get; set; }

        [ForeignKey(nameof(CandidateRoundID))]
        public CandidateInterviewRound Round { get; set; }

        [Required]
        public string InterviewerID { get; set; }

        [ForeignKey(nameof(InterviewerID))]
        public ApplicationUser Interviewer { get; set; }

        [MaxLength(2000)]
        public string? OverallComment { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public ICollection<InterviewSkillRating> SkillRatings { get; set; }
            = new List<InterviewSkillRating>();
    }
}
