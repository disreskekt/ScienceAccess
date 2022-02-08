using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Api.Models;
using Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthOptions _authOptions;

        public AuthController(IOptions<AuthOptions> authOptions)
        {
            _authOptions = authOptions.Value;
        }
        
        [Route("[action]")]
        [HttpPost]
        public IActionResult Login([FromBody] LoginModel loginModel)
        {
            try
            {
                UserModel user = AuthenticateUser(loginModel.Email, loginModel.Password);

                if (user is null)
                {
                    return Unauthorized();
                }

                string token = GenerateJwt(user);

                return Ok(token);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(e.Message);
            }
        }

        private UserModel AuthenticateUser(string email, string password)
        {
            return new UserModel(); //todo ретурн из базы
        }

        private string GenerateJwt(UserModel userModel)
        {
            SymmetricSecurityKey securityKey = _authOptions.GetSymmetricSecurityKey();
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.Sha256);

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Role, userModel.Role.RoleName),
            };

            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: _authOptions.Issuer,
                audience: _authOptions.Audience,
                claims: claims,
                expires: DateTime.Now.AddSeconds(_authOptions.TokenLifeTime),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }
    }
}