using System.ComponentModel.DataAnnotations;

namespace ecommerce_api.Models.DTOs
{
    public class ChangePasswordRequest
    {
        [Required]
        public required string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")]
        public required string NewPassword { get; set; }
    }
}
