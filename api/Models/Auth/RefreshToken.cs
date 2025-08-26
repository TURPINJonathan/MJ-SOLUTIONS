using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
				[Required]
        public required string Token { get; set; }
				[Required]
        public required int UserId { get; set; }
				[Required]
        public required DateTime ExpiryDate { get; set; }
    }
}