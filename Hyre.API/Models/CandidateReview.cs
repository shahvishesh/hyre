using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyre.API.Models
{
    public class CandidateReview
    {
        [Key]
        public int ReviewID { get; set; }

        [Required]
        public int CandidateJobID { get; set; }

        [ForeignKey(nameof(CandidateJobID))]
        public CandidateJob CandidateJob { get; set; }


        [Required]
        public string ReviewerId { get; set; } 

        [ForeignKey(nameof(ReviewerId))]
        public ApplicationUser Reviewer { get; set; }

        public string? Comment { get; set; }

        [MaxLength(20)]
        public string Decision { get; set; } = "Pending"; // Pending, Shortlisted, Rejected

        public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;

        // Recruiter  fields
        public string? RecruiterId { get; set; }

        [ForeignKey(nameof(RecruiterId))]
        public ApplicationUser? Recruiter { get; set; }

        public string? RecruiterDecision { get; set; } // Shortlisted, Rejected, null

        public DateTime? RecruiterActionAt { get; set; }

        public ICollection<CandidateSkillReview> SkillReviews { get; set; } = new List<CandidateSkillReview>();

        public ICollection<CandidateReviewComment> Comments { get; set; } = new List<CandidateReviewComment>();
    }
}
