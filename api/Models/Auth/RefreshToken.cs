using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public int UserId { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}