using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace TravelGuideWebAdmin.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthApiController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("token")]
        public IActionResult Token([FromBody] ApiAuthRequest request)
        {
            var clientId     = _config["Api:ClientId"];
            var clientSecret = _config["Api:ClientSecret"];
            var key          = _config["Jwt:Key"];
            var issuer       = _config["Jwt:Issuer"]   ?? "TravelGuideWebAdmin";
            var audience     = _config["Jwt:Audience"] ?? "TravelGuideMobileApp";

            // Từ chối phục vụ nếu config chưa được thiết lập đúng
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(clientSecret))
                return StatusCode(500, "Server configuration error.");

            if (request == null || request.ClientId != clientId || request.ClientSecret != clientSecret)
                return Unauthorized();

            if (key.Length < 32)
                return StatusCode(500, "JWT key is too short (minimum 32 characters).");

            var expiryDays = _config.GetValue<int>("Jwt:TokenExpiryDays", 1);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, request.ClientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddDays(expiryDays);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return Ok(new ApiAuthResponse
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = expires
            });
        }
    }

    public class ApiAuthRequest
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class ApiAuthResponse
    {
        public string AccessToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
