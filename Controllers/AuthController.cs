using ecommerce_api.Models.DTOs;
using ecommerce_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ecommerce_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthenticationService _authService;
        private readonly JwtService _jwtService;

        public AuthController(AuthenticationService authService, JwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message, user) = await _authService.RegisterUser(request);

            if (!success)
            {
                return BadRequest(new { message });
            }

            var token = _jwtService.GenerateToken(user!);
            var response = new AuthResponse
            {
                Token = token,
                Username = user!.Username,
                Email = user.Email,
                ExpiresAt = _jwtService.GetTokenExpiration()
            };

            return CreatedAtAction(nameof(Register), new { id = user.Id }, response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message, user) = await _authService.ValidateCredentials(request);

            if (!success)
            {
                return Unauthorized(new { message });
            }

            var token = _jwtService.GenerateToken(user!);
            var response = new AuthResponse
            {
                Token = token,
                Username = user!.Username,
                Email = user.Email,
                ExpiresAt = _jwtService.GetTokenExpiration()
            };

            return Ok(response);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var (success, message) = await _authService.ChangePassword(userId, request.CurrentPassword, request.NewPassword);

            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // In a stateless JWT system, logout is handled client-side by removing the token
            // This endpoint is provided for consistency but doesn't need server-side logic
            return Ok(new { message = "Logout successful. Please remove the token from your client." });
        }
    }
}
