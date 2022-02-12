﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.Data;
using Api.Models;
using Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AuthController : ControllerBase
    {
        private readonly Context _db;
        private readonly AuthOptions _authOptions;

        public AuthController(Context context, IOptions<AuthOptions> authOptions)
        {
            _db = context;
            _authOptions = authOptions.Value;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] Register register)
        {
            try
            {
                await _db.Users.AddAsync(new User()
                {
                    Email = register.Email,
                    Password = register.Password,
                    Name = register.Name,
                    SurName = register.SurName,
                    RoleId = 1
                });

                await _db.SaveChangesAsync();

                return await Login(new Login() {Email = register.Email, Password = register.Password});
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            try
            {
                User user = await AuthenticateUser(login.Email, login.Password);

                if (user is null)
                {
                    return Unauthorized();
                }

                string token = GenerateJwt(user);

                return Ok(token);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private async Task<User> AuthenticateUser(string email, string password)
        {
            return await _db.Users.Where(u => u.Email == email && u.Password == password).Include(u => u.Role).FirstOrDefaultAsync();
        }

        private string GenerateJwt(User user)
        {
            SymmetricSecurityKey securityKey = _authOptions.GetSymmetricSecurityKey();
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.RoleName),
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