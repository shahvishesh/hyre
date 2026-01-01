using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyre.API.Models
{
    public class InterviewSkillRating
    {
        [Key]
        public int RatingID { get; set; }

        [Required]
        public int FeedbackID { get; set; }

        [ForeignKey(nameof(FeedbackID))]
        public CandidateInterviewFeedback Feedback { get; set; }

        [Required]
        public int SkillID { get; set; }

        [ForeignKey(nameof(SkillID))]
        public Skill Skill { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }
    }
}
