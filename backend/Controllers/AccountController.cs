using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using backend.Entities;
using backend.Entities.Models;
using backend.Payloads;
using BC = BCrypt.Net.BCrypt;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController
    {
        private IConfiguration _config { get; }
        private readonly backendContext _db;
        public AccountController(backendContext db, IConfiguration configuration)
        {
            _config = configuration;
            _db = db;
        }
        [AllowAnonymous]
        [HttpPost("register")]
        public ActionResult<User> Register([FromBody] RegisterPayload registerPayload)
        {
            try
            {
                var existingUser = _db.Users.Any(u => u.Email == registerPayload.Email);
                if (existingUser)
                {
                    return new JsonResult(new { status = "false", message = "An account with this email already exists" });
                }
                var userToCreate = new User
                {
                    Email = registerPayload.Email,
                    FirstName = registerPayload.FirstName,
                    LastName = registerPayload.LastName,
                    PasswordHash = BC.HashPassword(registerPayload.Password),
                    Gender = registerPayload.Gender,

                };
                _db.Users.Add(userToCreate);
                _db.SaveChanges();

                return new JsonResult(new { status = true, user = userToCreate });
            }
            catch (Exception)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public ActionResult<User> Login([FromBody] LoginPayload loginPayload)
        {
            var foundUser = _db.Users.SingleOrDefault(u => u.Email == loginPayload.Email);

            if (foundUser != null)
            {
                if (BC.Verify(loginPayload.Password, foundUser.PasswordHash))
                {
                    var tokenString = GenerateJSONWebToken(foundUser); 
                    return new JsonResult(new { status = true, foundUser.FirstName, foundUser.Id });
                }
                return new JsonResult(new { status = false, message = "Wrong password or email" });
            }
            else
            {
                return new JsonResult(new { status = false, message = "User does not exist" });
            }
        }

        private string GenerateJSONWebToken(User User)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.NameId, User.Id.ToString()),
            };


            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddDays(30),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
