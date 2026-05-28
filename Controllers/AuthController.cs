using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExpensesSystem.Data;
using ExpensesSystem.Models;

namespace ExpensesSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public IActionResult Register(User user)
        {
            var exists = _context.Users.FirstOrDefault(u => u.Name == user.Name);
            if (exists != null) return BadRequest("User already exists");

            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok("Registered successfully");
        }

        [HttpPost("login")]
        public IActionResult Login(User user)
        {
            var exists = _context.Users.FirstOrDefault(u => u.Name == user.Name && u.Password == user.Password);
            if (exists == null) return Unauthorized("Invalid credentials");

            var token = GenerateToken(exists);
            return Ok(new { token, role = exists.Role });
        }

        private string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}