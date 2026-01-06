using ecommerce_api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ecommerce_api.Services
{
    public class JwtService
    {
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationMinutes;

        public JwtService(IConfiguration configuration)
        {
            _secret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
            _issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
            _audience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
            _expirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");
        }

        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public DateTime GetTokenExpiration()
        {
            return DateTime.UtcNow.AddMinutes(_expirationMinutes);
        }
    }
}
