using ecommerce_api.Models;
using ecommerce_api.Models.DTOs;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ecommerce_api.Tests.IntegrationTests
{
    public class AuthControllerRegistrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public AuthControllerRegistrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_WithValidData_Returns201Created()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "newuser123",
                FirstName = "New",
                LastName = "User",
                Email = "newuser123@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("message").GetString().Should().Contain("User registered successfully");
            result.GetProperty("email").GetString().Should().Be("newuser123@example.com");
            result.GetProperty("username").GetString().Should().Be("newuser123");
            result.GetProperty("activationToken").GetString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Register_WithMissingFirstName_Returns400BadRequest()
        {
            // Arrange
            var invalidRequest = new
            {
                Username = "testuser",
                LastName = "User",
                Email = "test@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", invalidRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_WithMissingLastName_Returns400BadRequest()
        {
            // Arrange
            var invalidRequest = new
            {
                Username = "testuser",
                FirstName = "Test",
                Email = "test@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", invalidRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_WithMissingPhoneNumber_Returns400BadRequest()
        {
            // Arrange
            var invalidRequest = new
            {
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", invalidRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_WithMismatchedPasswords_Returns400BadRequest()
        {
            // Arrange
            var invalidRequest = new RegisterRequest
            {
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Different@1234"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", invalidRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_WithWeakPassword_Returns400BadRequest()
        {
            // Arrange
            var invalidRequest = new RegisterRequest
            {
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PhoneNumber = "+1234567890",
                Password = "weak",
                ConfirmPassword = "weak"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", invalidRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_Returns400BadRequest()
        {
            // Arrange - Register first user
            var firstRequest = new RegisterRequest
            {
                Username = "user1dup",
                FirstName = "First",
                LastName = "User",
                Email = "duplicate123@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };
            await _client.PostAsJsonAsync("/api/auth/register", firstRequest);

            // Try to register with same email
            var duplicateRequest = new RegisterRequest
            {
                Username = "user2dup",
                FirstName = "Second",
                LastName = "User",
                Email = "duplicate123@example.com",
                PhoneNumber = "+1234567891",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", duplicateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("message").GetString().Should().Be("Email already exists");
        }

        [Fact]
        public async Task Register_WithDuplicateUsername_Returns400BadRequest()
        {
            // Arrange - Register first user
            var firstRequest = new RegisterRequest
            {
                Username = "duplicateuser123",
                FirstName = "First",
                LastName = "User",
                Email = "user1dup@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };
            await _client.PostAsJsonAsync("/api/auth/register", firstRequest);

            // Try to register with same username
            var duplicateRequest = new RegisterRequest
            {
                Username = "duplicateuser123",
                FirstName = "Second",
                LastName = "User",
                Email = "user2dup@example.com",
                PhoneNumber = "+1234567891",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", duplicateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("message").GetString().Should().Be("Username already exists");
        }

        [Fact]
        public async Task VerifyEmail_WithValidToken_Returns200OK()
        {
            // Arrange - Register a user
            var registerRequest = new RegisterRequest
            {
                Username = "verifytest",
                FirstName = "Verify",
                LastName = "Test",
                Email = "verifytest@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            var registerResult = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
            var token = registerResult.GetProperty("activationToken").GetString();

            // Act
            var verifyRequest = new VerifyEmailRequest
            {
                Email = "verifytest@example.com",
                Token = token!
            };
            var response = await _client.PostAsJsonAsync("/api/auth/verify-email", verifyRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("message").GetString().Should().Be("Email verified successfully. Your account is now active.");
        }

        [Fact]
        public async Task VerifyEmail_WithInvalidToken_Returns400BadRequest()
        {
            // Arrange - Register a user
            var registerRequest = new RegisterRequest
            {
                Username = "invalidtoken",
                FirstName = "Invalid",
                LastName = "Token",
                Email = "invalidtoken@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Act - Try to verify with wrong token
            var verifyRequest = new VerifyEmailRequest
            {
                Email = "invalidtoken@example.com",
                Token = "wrong-token"
            };
            var response = await _client.PostAsJsonAsync("/api/auth/verify-email", verifyRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("message").GetString().Should().Be("Invalid activation token");
        }

        [Fact]
        public async Task Register_ThenLoginWithoutVerification_Returns401Unauthorized()
        {
            // Arrange - Register a user
            var registerRequest = new RegisterRequest
            {
                Username = "unverified",
                FirstName = "Unverified",
                LastName = "User",
                Email = "unverified@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Act - Try to login without verifying
            var loginRequest = new LoginRequest
            {
                Username = "unverified",
                Password = "Test@1234"
            };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("message").GetString().Should().Be("Account is inactive");
        }

        [Fact]
        public async Task Register_VerifyEmail_ThenLogin_Returns200OK()
        {
            // Arrange - Register a user
            var registerRequest = new RegisterRequest
            {
                Username = "fullflow",
                FirstName = "Full",
                LastName = "Flow",
                Email = "fullflow@example.com",
                PhoneNumber = "+1234567890",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            var registerResult = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
            var token = registerResult.GetProperty("activationToken").GetString();

            // Verify email
            var verifyRequest = new VerifyEmailRequest
            {
                Email = "fullflow@example.com",
                Token = token!
            };
            await _client.PostAsJsonAsync("/api/auth/verify-email", verifyRequest);

            // Act - Login after verification
            var loginRequest = new LoginRequest
            {
                Username = "fullflow",
                Password = "Test@1234"
            };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            authResponse.Should().NotBeNull();
            authResponse!.Token.Should().NotBeNullOrEmpty();
        }
    }
}
