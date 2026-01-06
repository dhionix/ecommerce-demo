using System.ComponentModel.DataAnnotations;

namespace ecommerce_api.Models.DTOs
{
    public class RegisterRequest : IValidatableObject
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public required string Username { get; set; }

        [Required]
        [StringLength(50)]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public required string LastName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [Phone]
        [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Phone number must be in valid format")]
        public required string PhoneNumber { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")]
        public required string Password { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public required string ConfirmPassword { get; set; }

        public Role Role { get; set; } = Role.Member;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Password != ConfirmPassword)
            {
                yield return new ValidationResult(
                    "Password and Confirm Password must match",
                    new[] { nameof(ConfirmPassword) });
            }

            // Admin role can only be assigned manually, not during registration
            if (Role == Role.Admin)
            {
                yield return new ValidationResult(
                    "Admin role cannot be assigned during registration",
                    new[] { nameof(Role) });
            }
        }
    }
}
