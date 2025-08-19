using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class BlacklistedToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime BlacklistedAt { get; set; }
    }
}