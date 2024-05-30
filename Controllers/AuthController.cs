using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OAuthLoginAPI.Modles;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OAuthLoginAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginModel login)
        {
            // Validate user credentials (static data for demonstration)
            if (IsValidUser(login.Username, login.Password, out var user))
            {
                // Create claims for the user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("scope", user.Scope),
                };

                // Create JWT token
                var token = GenerateJWTToken(claims);

                return Ok(new { token });
            }

            return Unauthorized();
        }

        private bool IsValidUser(string username, string password, out User user)
        {
            // Static user data for demonstration
            if (username == "user" && password == "password")
            {
                user = new User { Id = "1", Username = "user", Role = "player", Scope = "b_game" };
                return true;
            }
            else if (username == "vipuser" && password == "password")
            {
                user = new User { Id = "2", Username = "vipuser", Role = "admin", Scope = "b_game vip_character_personalize" };
                return true;
            }

            user = null;
            return false;
        }

        private string GenerateJWTToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:ExpirationMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

   
}
