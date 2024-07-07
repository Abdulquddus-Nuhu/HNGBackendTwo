using FluentAssertions;
using HNGBackendTwo.Controllers;
using HNGBackendTwo.Data;
using HNGBackendTwo.Dtos;
using HNGBackendTwo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Xunit;

namespace tests
{
    public class UserControllerTests
    {
        private readonly AppDbContext _context;
        private readonly UserController _controller;
        private readonly IConfiguration _configuration;

        public UserControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new AppDbContext(options);

            SeedDatabase();

            _configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            _configuration["Jwt:Key"] = "supersecretkey_supersecretkey_supersecretkey_supersecretkey";
            _configuration["Jwt:Issuer"] = "TestIssuer";
            _controller = new UserController(_context, _configuration);
        }

  
        [Fact]
        public async Task Register_Should_Return_Created_On_Success()
        {
            var userDto = new UserRegistrationDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "Password123",
                Phone = "1234567890"
            };

            var result = await _controller.Register(userDto);
            var createdResult = result as CreatedResult;

            Assert.NotNull(createdResult);
            Assert.Equal(201, createdResult.StatusCode);

            var user = _context.Users.FirstOrDefault(u => u.Email == userDto.Email);
            Assert.NotNull(user);
            Assert.Equal("John's Organisation", _context.Organisations.FirstOrDefault(o => o.Name == "John's Organisation")?.Name);
        }

        [Fact]
        public async Task Register_Should_Return_UnprocessableEntity_On_Duplicate_Email()
        {
            var userDto = new UserRegistrationDto
            {
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice.smith@example.com",
                Password = "Password123",
                Phone = "1234567890"
            };

            var result = await _controller.Register(userDto);
            var unprocessableEntityResult = result as UnprocessableEntityObjectResult;

            Assert.NotNull(unprocessableEntityResult);
            Assert.Equal(422, unprocessableEntityResult.StatusCode);
        }


        [Fact]
        public async Task Login_Should_Return_Ok_On_Success()
        {
            var loginDto = new UserLoginDto
            {
                Email = "alice.smith@example.com",
                Password = "Password123"
            };

            var result = await _controller.Login(loginDto);
            var okResult = result as OkObjectResult;

            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);

            var responseObject = JsonSerializer.Serialize(okResult.Value);
            using var document = JsonDocument.Parse(responseObject);
            var root = document.RootElement;

            Assert.True(root.TryGetProperty("data", out var data));
            Assert.True(data.TryGetProperty("accessToken", out var accessTokenElement));

            var token = accessTokenElement.GetString();
            Assert.NotNull(token);

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            var emailClaim = jsonToken.Claims.First(claim => claim.Type == "unique_name").Value;

            Assert.Equal(loginDto.Email, emailClaim);
        }

        [Fact]
        public async Task Login_Should_Return_Unauthorized_On_Failure()
        {
            var loginDto = new UserLoginDto
            {
                Email = "alice.smith@example.com",
                Password = "WrongPassword"
            };

            var result = await _controller.Login(loginDto);
            var unauthorizedResult = result as UnauthorizedObjectResult;

            Assert.NotNull(unauthorizedResult);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task GetUser_Should_Return_Ok_On_Success()
        {
            var user = await _context.Users.FirstAsync();
            var token = GenerateJwtToken(user);

            _controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.UserId)
            }))
            };

            var result = await _controller.GetUser(user.UserId);
            var okResult = result as OkObjectResult;

            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
        }


        [Fact]
        public async Task GetOrganisations_Should_Return_Ok_On_Success()
        {
            var user = await _context.Users.FirstAsync();
            var token = GenerateJwtToken(user);

            _controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId)
                }))
            };

            var result = await _controller.GetOrganisations();
            var okResult = result as OkObjectResult;

            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetOrganisation_Should_Return_Ok_On_Existing_Organisation()
        {
            var organisation = await _context.Organisations.FirstAsync();
            var result = await _controller.GetOrganisation(organisation.OrgId);
            var okResult = result as OkObjectResult;

            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);

            var responseObject = JsonSerializer.Serialize(okResult.Value);
            using var document = JsonDocument.Parse(responseObject);
            var root = document.RootElement;

            Assert.True(root.TryGetProperty("data", out var data));
            Assert.Equal(organisation.OrgId, data.GetProperty("OrgId").GetString());
            Assert.Equal(organisation.Name, data.GetProperty("Name").GetString());
            Assert.Equal(organisation.Description, data.GetProperty("Description").GetString());
        }

        [Fact]
        public async Task GetOrganisation_Should_Return_NotFound_On_Nonexistent_Organisation()
        {
            var nonExistingOrgId = "825fb3ea-5cc9-4b48-93ff-d2e5c5a6f624";

            var result = await _controller.GetOrganisation(nonExistingOrgId);
            var notFoundResult = result as NotFoundObjectResult;

            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task CreateOrganisation_Should_Return_CreatedAtAction_On_Success()
        {
            var dto = new OrganisationDto
            {
                Name = "NewOrg",
                Description = "New Organisation"
            };

            var user = await _context.Users.FirstAsync();
            var token = GenerateJwtToken(user);

            _controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId)
                }))
            };

            var result = await _controller.CreateOrganisation(dto);
            var createdAtActionResult = result as CreatedAtActionResult;

            Assert.NotNull(createdAtActionResult);
            Assert.Equal(201, createdAtActionResult.StatusCode);

            var responseObject = JsonSerializer.Serialize(createdAtActionResult.Value);
            using var document = JsonDocument.Parse(responseObject);
            var root = document.RootElement;

            Assert.True(root.TryGetProperty("data", out var data));
            Assert.Equal(dto.Name, data.GetProperty("Name").GetString());
            Assert.Equal(dto.Description, data.GetProperty("Description").GetString());
        }

        [Fact]
        public async Task CreateOrganisation_Should_Return_BadRequest_On_Invalid_Model()
        {
            var invalidDto = new OrganisationDto(); 

            _controller.ModelState.AddModelError("Name", "The Name field is required.");

            var result = await _controller.CreateOrganisation(invalidDto);
            var badRequestResult = result as BadRequestObjectResult;

            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
        }


        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId),
                    new Claim(ClaimTypes.Email, user.Email),
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private void SeedDatabase()
        {
            var users = new[]
            {
                new User
                {
                    UserId = Guid.NewGuid().ToString(),
                    FirstName = "Alice",
                    LastName = "Smith",
                    Email = "alice.smith@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("Password123"),
                    Phone = "1111111111"
                },
                new User
                {
                    UserId = Guid.NewGuid().ToString(),
                    FirstName = "Bob",
                    LastName = "Johnson",
                    Email = "bob.johnson@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("Password123"),
                    Phone = "2222222222"
                },
                new User
                {
                    UserId = Guid.NewGuid().ToString(),
                    FirstName = "Charlie",
                    LastName = "Brown",
                    Email = "charlie.brown@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("Password123"),
                    Phone = "3333333333"
                }
            };

            _context.Users.AddRange(users);
            _context.SaveChanges();
        }

    }
}