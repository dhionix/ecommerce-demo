using ecommerce_api.Models;
using ecommerce_api.Models.DTOs;
using ecommerce_api.Services;
using FluentAssertions;
using Xunit;

namespace ecommerce_api.Tests.UnitTests
{
    public class RegistrationValidationTests
    {
        private readonly AuthenticationService _authService;

        public RegistrationValidationTests()
        {
            _authService = new AuthenticationService();
        }

        [Fact]
        public async Task RegisterUser_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "newuser",
                FirstName = "New",
                LastName = "User",
                Email = "newuser@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234",
                Address = "123 Test St",
                Role = Role.Member
            };

            // Act
            var (success, message, user) = await _authService.RegisterUser(registerRequest);

            // Assert
            success.Should().BeTrue();
            message.Should().Contain("User registered successfully");
            user.Should().NotBeNull();
            user!.FirstName.Should().Be("New");
            user.LastName.Should().Be("User");
            user.Email.Should().Be("newuser@example.com");
            user.PhoneNumber.Should().Be("+1234567890");
            user.Address.Should().Be("123 Test St");
            user.Role.Should().Be(Role.Member);
            user.EmailVerified.Should().BeFalse();
            user.IsActive.Should().BeFalse();
            user.ActivationToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task RegisterUser_WithDuplicateEmail_ReturnsFailure()
        {
            // Arrange
            var firstRequest = new RegisterRequest
            {
                Username = "user1",
                FirstName = "First",
                LastName = "User",
                Email = "duplicate@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };
            await _authService.RegisterUser(firstRequest);

            var duplicateRequest = new RegisterRequest
            {
                Username = "user2",
                FirstName = "Second",
                LastName = "User",
                Email = "duplicate@example.com",
                PhoneNumber = "+1234567891",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            var (success, message, user) = await _authService.RegisterUser(duplicateRequest);

            // Assert
            success.Should().BeFalse();
            message.Should().Be("Email already exists");
            user.Should().BeNull();
        }

        [Fact]
        public async Task RegisterUser_WithDuplicateUsername_ReturnsFailure()
        {
            // Arrange
            var firstRequest = new RegisterRequest
            {
                Username = "duplicateuser",
                FirstName = "First",
                LastName = "User",
                Email = "user1@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };
            await _authService.RegisterUser(firstRequest);

            var duplicateRequest = new RegisterRequest
            {
                Username = "duplicateuser",
                FirstName = "Second",
                LastName = "User",
                Email = "user2@example.com",
                PhoneNumber = "+1234567891",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            var (success, message, user) = await _authService.RegisterUser(duplicateRequest);

            // Assert
            success.Should().BeFalse();
            message.Should().Be("Username already exists");
            user.Should().BeNull();
        }

        [Fact]
        public async Task RegisterUser_WithLibrarianRole_CreatesLibrarian()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "librarian1",
                FirstName = "Library",
                LastName = "Staff",
                Email = "librarian@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234",
                Role = Role.Librarian
            };

            // Act
            var (success, message, user) = await _authService.RegisterUser(registerRequest);

            // Assert
            success.Should().BeTrue();
            user.Should().NotBeNull();
            user!.Role.Should().Be(Role.Librarian);
        }

        [Fact]
        public async Task RegisterUser_DefaultsToMemberRole()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "defaultuser",
                FirstName = "Default",
                LastName = "User",
                Email = "default@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            var (success, message, user) = await _authService.RegisterUser(registerRequest);

            // Assert
            success.Should().BeTrue();
            user.Should().NotBeNull();
            user!.Role.Should().Be(Role.Member);
        }

        [Fact]
        public async Task VerifyEmail_WithValidToken_ActivatesAccount()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "verifyuser",
                FirstName = "Verify",
                LastName = "User",
                Email = "verify@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };
            var (_, _, user) = await _authService.RegisterUser(registerRequest);
            var token = user!.ActivationToken!;

            // Act
            var (success, message) = await _authService.VerifyEmail("verify@example.com", token);

            // Assert
            success.Should().BeTrue();
            message.Should().Be("Email verified successfully. Your account is now active.");
            
            // Verify user is now active
            var verifiedUser = await _authService.GetUserByUsername("verifyuser");
            verifiedUser!.EmailVerified.Should().BeTrue();
            verifiedUser.IsActive.Should().BeTrue();
            verifiedUser.ActivationToken.Should().BeNull();
        }

        [Fact]
        public async Task VerifyEmail_WithInvalidToken_ReturnsFailure()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };
            await _authService.RegisterUser(registerRequest);

            // Act
            var (success, message) = await _authService.VerifyEmail("test@example.com", "invalid-token");

            // Assert
            success.Should().BeFalse();
            message.Should().Be("Invalid activation token");
        }

        [Fact]
        public async Task VerifyEmail_WithNonExistentEmail_ReturnsFailure()
        {
            // Act
            var (success, message) = await _authService.VerifyEmail("nonexistent@example.com", "some-token");

            // Assert
            success.Should().BeFalse();
            message.Should().Be("User not found");
        }

        [Fact]
        public async Task VerifyEmail_AlreadyVerified_ReturnsFailure()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "alreadyverified",
                FirstName = "Already",
                LastName = "Verified",
                Email = "already@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };
            var (_, _, user) = await _authService.RegisterUser(registerRequest);
            var token = user!.ActivationToken!;
            
            // Verify once
            await _authService.VerifyEmail("already@example.com", token);

            // Act - Try to verify again
            var (success, message) = await _authService.VerifyEmail("already@example.com", token);

            // Assert
            success.Should().BeFalse();
            message.Should().Be("Email already verified");
        }

        [Fact]
        public async Task RegisterUser_CaseInsensitiveEmailCheck_ReturnsFailure()
        {
            // Arrange
            var firstRequest = new RegisterRequest
            {
                Username = "user1",
                FirstName = "First",
                LastName = "User",
                Email = "Test@Example.Com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };
            await _authService.RegisterUser(firstRequest);

            var duplicateRequest = new RegisterRequest
            {
                Username = "user2",
                FirstName = "Second",
                LastName = "User",
                Email = "test@example.com",
                PhoneNumber = "+1234567891",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            var (success, message, user) = await _authService.RegisterUser(duplicateRequest);

            // Assert
            success.Should().BeFalse();
            message.Should().Be("Email already exists");
        }

        [Fact]
        public async Task RegisterUser_CaseInsensitiveUsernameCheck_ReturnsFailure()
        {
            // Arrange
            var firstRequest = new RegisterRequest
            {
                Username = "TestUser",
                FirstName = "First",
                LastName = "User",
                Email = "user1@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };
            await _authService.RegisterUser(firstRequest);

            var duplicateRequest = new RegisterRequest
            {
                Username = "testuser",
                FirstName = "Second",
                LastName = "User",
                Email = "user2@example.com",
                PhoneNumber = "+1234567891",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            var (success, message, user) = await _authService.RegisterUser(duplicateRequest);

            // Assert
            success.Should().BeFalse();
            message.Should().Be("Username already exists");
        }
    }
}
