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
            var clientId = _config["Api:ClientId"] ?? "travelguide-app";
            var clientSecret = _config["Api:ClientSecret"] ?? "change-me";

            if (request == null || request.ClientId != clientId || request.ClientSecret != clientSecret)
                return Unauthorized();

            var key = _config["Jwt:Key"] ?? "travelguide-secret-key-change";
            var issuer = _config["Jwt:Issuer"] ?? "TravelGuideWebAdmin";
            var audience = _config["Jwt:Audience"] ?? "TravelGuideMobileApp";

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, request.ClientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(7);

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
