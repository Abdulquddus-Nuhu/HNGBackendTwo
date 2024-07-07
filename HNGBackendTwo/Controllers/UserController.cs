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
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace HNGBackendTwo.Controllers
{
    //[Route("api/[controller]")]
    [Route("api/")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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

        [AllowAnonymous]
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

            var user = new User
            {
                UserId = Guid.NewGuid().ToString(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone = dto.Phone
            };

            var organisation = new Organisation
            {
                OrgId = Guid.NewGuid().ToString(),
                Name = $"{dto.FirstName}'s Organisation"
            };

            _context.Add(organisation);
            _context.Add(user);

            var organizationUser = new OrganisationUser()
            {
                OrganisationId = organisation.OrgId,
                UserId = user.UserId,
            };
            _context.Add(organizationUser);

            var status = await _context.SaveChangesAsync();
            //Successfully saved data in database
            if (status > 0)
            {
                var token = GenerateJwtToken(user);
                return Created("", new { status = "success", message = "Registration successful", data = new { accessToken = token, user = new { user.UserId, user.FirstName, user.LastName, user.Email, user.Phone } } });

            }
            else
            {
                return BadRequest(new
                {
                    status = "Bad request",
                    message = "Registration unsuccessful",
                    statusCode = 400
                });
            }
        }

        [AllowAnonymous]
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

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
            {
                return NotFound(new
                {
                    status = "success",
                    message = "User not found",
                    statusCode = 404
                });
            }

            return Ok(new { status = "success", message = "User retrieved", data = new { user.UserId, user.FirstName, user.LastName, user.Email, user.Phone } });
        }

        [HttpGet("organisations")]
        public async Task<IActionResult> GetOrganisations()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var organisations = await _context.Organisations
                .Include(o => o.OrganisationUsers)
                .Where(o => o.OrganisationUsers.Any(u => u.UserId == userId)).ToListAsync();

            return Ok(new { status = "success", message = "Organisations retrieved", data = new { organisations = organisations.Select(o => new { o.OrgId, o.Name, o.Description }) } });
        }

        [HttpGet("{orgId}")]
        public async Task<IActionResult> GetOrganisation(string orgId)
        {
            var organisation = await _context.Organisations.FindAsync(orgId);
            if (organisation == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "Organisation not found"
                });
            }

            return Ok(new
            {
                status = "success",
                message = "Organisation retrieved successfully",
                data = new
                {
                    organisation.OrgId,
                    organisation.Name,
                    organisation.Description
                }
            });
        }

        [HttpPost("organisation")]
        public async Task<IActionResult> CreateOrganisation([FromBody] OrganisationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    status = "Bad Request",
                    message = "Client error",
                    statusCode = 400
                });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var newOrganization = new Organisation()
            {
                Name = dto.Name,
                Description = dto.Description,
            };
            _context.Organisations.Add(newOrganization);

            var organizationUser = new OrganisationUser()
            {
                OrganisationId = newOrganization.OrgId,
                UserId = userId,
            };
            _context.Add(organizationUser);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrganisation), new { orgId = newOrganization.OrgId }, new
            {
                status = "success",
                message = "Organisation created successfully",
                data = new
                {
                    newOrganization.OrgId,
                    dto.Name,
                    dto.Description
                }
            });
        }

        [HttpPost("{orgId}/users")]
        public async Task<IActionResult> AddUserToOrganisation(string orgId, [FromBody] string userId)
        {
            var organisation = await _context.Organisations.FindAsync(orgId);
            if (organisation == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "Organisation not found"
                });
            }

            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "User not found"
                });
            }

            var orgUser = new OrganisationUser
            {
                OrganisationId = orgId,
                UserId = userId
            };

            _context.OrganisationUsers.Add(orgUser);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = "success",
                message = "User added to organisation successfully"
            });
        }

        [AllowAnonymous]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.Select(x => new 
            {
                UserId = x.UserId,
                firstname = x.FirstName,
                lastname = x.LastName,
                phonenumber = x.Phone,
                email = x.Email,
            }).ToListAsync();

            return Ok(users);
        }
        private string GenerateJwtToken(User user)
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
