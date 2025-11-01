using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyre.API.Models
{
    public class JobReviewer
    {
        [Key]
        public int JobReviewerId { get; set; }

        [Required]
        public int JobId { get; set; }

        [ForeignKey(nameof(JobId))]
        public Job Job { get; set; }

        [Required]
        public string ReviewerId { get; set; } 

        [ForeignKey(nameof(ReviewerId))]
        public ApplicationUser Reviewer { get; set; }

        public string AssignedBy { get; set; }  
        [ForeignKey(nameof(AssignedBy))]
        public ApplicationUser AssignedByUser { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
