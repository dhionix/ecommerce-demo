using System.ComponentModel.DataAnnotations;

namespace ecommerce_api.Models.DTOs
{
    public class LoginRequest
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}
