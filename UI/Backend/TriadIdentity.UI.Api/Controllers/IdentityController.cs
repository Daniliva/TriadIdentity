using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TriadIdentity.BLL.Interfaces.Common;
using TriadIdentity.BLL.Models.DTOs.Identity;
using TriadIdentity.UI.Api.Models;
using TriadIdentity.UI.Api.Options;

namespace TriadIdentity.UI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly JwtOptions _jwt;

        public IdentityController(IIdentityService identityService, IOptions<JwtOptions> jwt)
        {
            _identityService = identityService;
            _jwt = jwt.Value;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            await _identityService.RegisterUserAsync(dto);
            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            var userId = await _identityService.LoginUserAsync(dto);
            var user = await _identityService.GetUserByIdAsync(userId);
           
            var token = GenerateJwtToken(user);
            return Ok(new TokenResponse
            {
                Token = token,
                Expires = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes)
            });
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateUserDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (dto.Id != userId)
            {
                return Forbid();

            }

            await _identityService.UpdateUserAsync(dto);
            return Ok("User updated successfully.");
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _identityService.GetUserByIdAsync(userId);
            
            return Ok(user);
        }

        [Authorize]
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var roles = await _identityService.GetUserRolesAsync(userId);
            return Ok(roles);
        }

        [Authorize]
        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var logs = await _identityService.GetUserLogsAsync(userId);
            return Ok(logs);
            
        }

        private string GenerateJwtToken(UserDto user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}