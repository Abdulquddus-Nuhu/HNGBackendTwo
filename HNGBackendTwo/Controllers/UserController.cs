using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using HNGBackendTwo.Data;
using HNGBackendTwo.Dtos;
using HNGBackendTwo.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace HNGBackendTwo.Controllers
{
    //[Route("api/[controller]")]
    [Route("api/")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(dto);
                //return UnprocessableEntity(new { errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })) });
            }

            if (_context.Users.Any(u => u.Email == dto.Email))
            {
                return UnprocessableEntity(new { errors = new[] { new { field = "email", message = "Email is already taken" } } });
            }

            var user = new UserModel
            {
                UserId = Guid.NewGuid().ToString(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone = dto.Phone
            };

            var organisation = new OrganisationModel
            {
                OrgId = Guid.NewGuid().ToString(),
                Name = $"{dto.FirstName}'s Organisation"
            };

            user.Organisations = new List<OrganisationModel> { organisation };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            return Created("", new { status = "success", message = "Registration successful", data = new { accessToken = token, user = new { user.UserId, user.FirstName, user.LastName, user.Email, user.Phone } } });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                return Unauthorized(new { status = "Bad request", message = "Authentication failed", statusCode = 401 });
            }

            var token = GenerateJwtToken(user);
            return Ok(new { status = "success", message = "Login successful", data = new { accessToken = token, user = new { user.UserId, user.FirstName, user.LastName, user.Email, user.Phone } } });
        }

        [Authorize]
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _context.Users.Include(u => u.Organisations).FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null || !user.Organisations.Any(o => o.Users.Contains(user)))
            {
                return NotFound();
            }

            return Ok(new { status = "success", message = "User retrieved", data = new { user.UserId, user.FirstName, user.LastName, user.Email, user.Phone } });
        }

        [Authorize]
        [HttpGet("organisations")]
        public async Task<IActionResult> GetOrganisations()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var organisations = await _context.Organisations.Include(o => o.Users).Where(o => o.Users.Any(u => u.UserId == userId)).ToListAsync();
            return Ok(new { status = "success", message = "Organisations retrieved", data = new { organisations = organisations.Select(o => new { o.OrgId, o.Name, o.Description }) } });
        }

        private string GenerateJwtToken(UserModel user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("SecretKeySecretKeySecretKeySecretKeySecretKeySecretKeySecretKey");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId),
                    new Claim(ClaimTypes.Name, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
