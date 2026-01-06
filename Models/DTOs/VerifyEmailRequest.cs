using System.ComponentModel.DataAnnotations;

namespace ecommerce_api.Models.DTOs
{
    public class VerifyEmailRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Token { get; set; }
    }
}
