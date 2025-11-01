using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyre.API.Models
{
    public class CandidateReviewComment
    {
        [Key]
        public int CommentID { get; set; }

        [Required]
        public int CandidateReviewID { get; set; }

        [Required]
        public string CommenterId { get; set; }

        [ForeignKey(nameof(CommenterId))]
        public ApplicationUser Commenter { get; set; }

        [Required, MaxLength(1000)]
        public string CommentText { get; set; }

        public DateTime CommentedAt { get; set; } = DateTime.UtcNow;
    }
}
