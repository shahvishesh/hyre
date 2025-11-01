using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyre.API.Models
{
    public class CandidateSkillReview
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CandidateReviewId { get; set; } 

        [Required]
        public int SkillId { get; set; } 

        public bool IsVerified { get; set; } = false;

        [Precision(5, 2)]
        public decimal? VerifiedYearsOfExperience { get; set; }

        [ForeignKey(nameof(CandidateReviewId))]
        public CandidateReview CandidateReview { get; set; }

        [ForeignKey(nameof(SkillId))]
        public Skill Skill { get; set; }
    }
}
