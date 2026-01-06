using ecommerce_api.Models;
using ecommerce_api.Models.DTOs;
using BCrypt.Net;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace ecommerce_api.Services
{
    public class AuthenticationService
    {
        private readonly ConcurrentDictionary<int, User> _users = new();
        private int _nextUserId = 1;

        public async Task<(bool Success, string Message, User? User)> RegisterUser(RegisterRequest request)
        {
            // Check if username already exists
            if (_users.Values.Any(u => u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "Username already exists", null);
            }

            // Check if email already exists
            if (_users.Values.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "Email already exists", null);
            }

            // Generate activation token
            var activationToken = GenerateActivationToken();

            // Create new user with hashed password
            var user = new User
            {
                Id = _nextUserId++,
                Username = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                ActivationToken = activationToken,
                EmailVerified = false,
                IsActive = false, // Account is inactive until email is verified
                CreatedAt = DateTime.UtcNow
            };

            _users[user.Id] = user;

            // In a real application, you would send an email here with the activation link
            // For now, we'll just return success with a message
            return await Task.FromResult((true, $"User registered successfully. Please verify your email using the activation token: {activationToken}", user));
        }

        public async Task<(bool Success, string Message)> VerifyEmail(string email, string token)
        {
            var user = _users.Values.FirstOrDefault(u => 
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                return (false, "User not found");
            }

            if (user.EmailVerified)
            {
                return (false, "Email already verified");
            }

            if (user.ActivationToken != token)
            {
                return (false, "Invalid activation token");
            }

            user.EmailVerified = true;
            user.IsActive = true;
            user.ActivationToken = null;

            return await Task.FromResult((true, "Email verified successfully. Your account is now active."));
        }

        private string GenerateActivationToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<(bool Success, string Message, User? User)> ValidateCredentials(LoginRequest request)
        {
            var user = _users.Values.FirstOrDefault(u => 
                u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                return (false, "Invalid username or password", (User?)null);
            }

            if (!user.IsActive)
            {
                return (false, "Account is inactive", (User?)null);
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return (false, "Invalid username or password", (User?)null);
            }

            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;

            return await Task.FromResult((true, "Login successful", user));
        }

        public async Task<User?> GetUserById(int userId)
        {
            _users.TryGetValue(userId, out var user);
            return await Task.FromResult(user);
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            var user = _users.Values.FirstOrDefault(u => 
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            return await Task.FromResult(user);
        }

        public async Task<(bool Success, string Message)> ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var user = await GetUserById(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                return (false, "Current password is incorrect");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            return (true, "Password changed successfully");
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await Task.FromResult(_users.Values.ToList());
        }
    }
}
