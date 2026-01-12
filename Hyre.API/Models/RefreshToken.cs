using System.ComponentModel.DataAnnotations;

namespace Hyre.API.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public string Token { get; set; }
        
        [Required]
        public DateTime Expiry { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsRevoked { get; set; } = false;
    }
}
