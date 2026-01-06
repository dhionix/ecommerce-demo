using ecommerce_api.Models;
using ecommerce_api.Models.DTOs;
using BCrypt.Net;
using System.Collections.Concurrent;

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

            // Create new user with hashed password
            var user = new User
            {
                Id = _nextUserId++,
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _users[user.Id] = user;

            return await Task.FromResult((true, "User registered successfully", user));
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
