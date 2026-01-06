using ecommerce_api.Models;
using ecommerce_api.Models.DTOs;
using ecommerce_api.Services;
using FluentAssertions;
using Xunit;

namespace ecommerce_api.Tests.UnitTests
{
    public class AuthenticationServiceTests
    {
        private readonly AuthenticationService _authService;

        public AuthenticationServiceTests()
        {
            _authService = new AuthenticationService();
        }

        [Fact]
        public async Task ValidateCredentials_ValidCredentials_ReturnsSuccessWithUser()
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
            
            // Activate account by verifying email
            var user = await _authService.GetUserByUsername("testuser");
            await _authService.VerifyEmail(user!.Email, user.ActivationToken!);

            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "Test@1234"
            };

            // Act
            var (success, message, resultUser) = await _authService.ValidateCredentials(loginRequest);

            // Assert
            success.Should().BeTrue();
            message.Should().Be("Login successful");
            resultUser.Should().NotBeNull();
            resultUser!.Username.Should().Be("testuser");
            resultUser.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task ValidateCredentials_NonExistentUsername_ReturnsFailure()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "nonexistentuser",
                Password = "Test@1234"
            };

            // Act
            var (success, message, user) = await _authService.ValidateCredentials(loginRequest);

            // Assert
            success.Should().BeFalse();
            message.Should().Be("Invalid username or password");
            user.Should().BeNull();
        }

        [Fact]
        public async Task ValidateCredentials_IncorrectPassword_ReturnsFailure()
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
            
            // Activate account
            var user = await _authService.GetUserByUsername("testuser");
            await _authService.VerifyEmail(user!.Email, user.ActivationToken!);

            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "WrongPassword@123"
            };

            // Act
            var (success, message, resultUser) = await _authService.ValidateCredentials(loginRequest);

            // Assert
            success.Should().BeFalse();
            message.Should().Be("Invalid username or password");
            resultUser.Should().BeNull();
        }

        [Fact]
        public async Task ValidateCredentials_InactiveAccount_ReturnsFailure()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "inactiveuser",
                FirstName = "Inactive",
                LastName = "User",
                Email = "inactive@example.com",
                PhoneNumber = "+1234567891",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };
            await _authService.RegisterUser(registerRequest);
            
            // Get the registered user and mark as inactive
            var registeredUser = await _authService.GetUserByUsername("inactiveuser");
            registeredUser!.IsActive = false;

            var loginRequest = new LoginRequest
            {
                Username = "inactiveuser",
                Password = "Test@1234"
            };

            // Act
            var (success, message, user) = await _authService.ValidateCredentials(loginRequest);

            // Assert
            success.Should().BeFalse();
            message.Should().Be("Account is inactive");
            user.Should().BeNull();
        }

        [Fact]
        public async Task ValidateCredentials_SuccessfulLogin_UpdatesLastLoginTime()
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
            
            // Activate account
            var userBeforeLogin = await _authService.GetUserByUsername("testuser");
            await _authService.VerifyEmail(userBeforeLogin!.Email, userBeforeLogin.ActivationToken!);
            
            var lastLoginBefore = userBeforeLogin!.LastLoginAt;

            var timestampBeforeLogin = DateTime.UtcNow;

            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "Test@1234"
            };

            // Act
            var (success, message, user) = await _authService.ValidateCredentials(loginRequest);

            // Assert
            success.Should().BeTrue();
            user!.LastLoginAt.Should().NotBeNull();
            user.LastLoginAt.Should().BeOnOrAfter(timestampBeforeLogin);
            user.LastLoginAt.Should().BeAfter(lastLoginBefore ?? DateTime.MinValue);
        }

        [Fact]
        public async Task ValidateCredentials_CaseInsensitiveUsername_ReturnsSuccess()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "TestUser",
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };
            await _authService.RegisterUser(registerRequest);
            
            // Activate account
            var user = await _authService.GetUserByUsername("TestUser");
            await _authService.VerifyEmail(user!.Email, user.ActivationToken!);

            var loginRequest = new LoginRequest
            {
                Username = "testuser", // lowercase
                Password = "Test@1234"
            };

            // Act
            var (success, message, resultUser) = await _authService.ValidateCredentials(loginRequest);

            // Assert
            success.Should().BeTrue();
            message.Should().Be("Login successful");
            resultUser.Should().NotBeNull();
            resultUser!.Username.Should().Be("TestUser");
        }

        [Fact]
        public async Task ValidateCredentials_CaseInsensitiveUsername_UpperCase_ReturnsSuccess()
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
            
            // Activate account
            var user = await _authService.GetUserByUsername("testuser");
            await _authService.VerifyEmail(user!.Email, user.ActivationToken!);

            var loginRequest = new LoginRequest
            {
                Username = "TESTUSER", // uppercase
                Password = "Test@1234"
            };

            // Act
            var (success, message, resultUser) = await _authService.ValidateCredentials(loginRequest);

            // Assert
            success.Should().BeTrue();
            message.Should().Be("Login successful");
            resultUser.Should().NotBeNull();
            resultUser!.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task ValidateCredentials_ReturnsProperErrorMessages()
        {
            // Arrange - Test with non-existent user
            var loginRequest1 = new LoginRequest
            {
                Username = "nonexistent",
                Password = "Test@1234"
            };

            // Act & Assert - Non-existent user
            var (success1, message1, _) = await _authService.ValidateCredentials(loginRequest1);
            success1.Should().BeFalse();
            message1.Should().Be("Invalid username or password");

            // Arrange - Register a user and mark as inactive
            var registerRequest = new RegisterRequest
            {
                Username = "inactiveuser",
                FirstName = "Inactive",
                LastName = "User",
                Email = "inactive@example.com",
                PhoneNumber = "+1234567891",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };
            await _authService.RegisterUser(registerRequest);
            var user = await _authService.GetUserByUsername("inactiveuser");
            user!.IsActive = false;

            var loginRequest2 = new LoginRequest
            {
                Username = "inactiveuser",
                Password = "Test@1234"
            };

            // Act & Assert - Inactive account
            var (success2, message2, _) = await _authService.ValidateCredentials(loginRequest2);
            success2.Should().BeFalse();
            message2.Should().Be("Account is inactive");

            // Arrange - Test with wrong password
            await _authService.RegisterUser(new RegisterRequest
            {
                Username = "validuser",
                FirstName = "Valid",
                LastName = "User",
                Email = "valid@example.com",
                PhoneNumber = "+1234567892",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            });
            
            // Activate account
            var validUser = await _authService.GetUserByUsername("validuser");
            await _authService.VerifyEmail(validUser!.Email, validUser.ActivationToken!);

            var loginRequest3 = new LoginRequest
            {
                Username = "validuser",
                Password = "WrongPassword"
            };

            // Act & Assert - Wrong password
            var (success3, message3, _) = await _authService.ValidateCredentials(loginRequest3);
            success3.Should().BeFalse();
            message3.Should().Be("Invalid username or password");
        }
    }
}
