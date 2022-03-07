﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Api.Data;
using Api.Helpers;
using Api.Models;
using Api.Models.Dtos;
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
            bool emailExists = await _db.Users.AnyAsync(u => u.Email == register.Email);

            if (emailExists)
            {
                return Conflict("This email already registered");
            }

            try
            {
                string hashString = register.Password.GenerateVerySecretHash(register.Email);

                await _db.Users.AddAsync(new User()
                {
                    Email = register.Email,
                    Password = hashString,
                    Name = register.Name,
                    Lastname = register.Lastname,
                    TicketRequest = false,
                    RoleId = 2
                });

                await _db.SaveChangesAsync();

                return await Login(new Login()
                {
                    Email = register.Email,
                    Password = register.Password
                });
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
            return await _db.Users.Where(u => u.Email == email && u.Password == password.GenerateVerySecretHash(email))
                                  .FirstOrDefaultAsync();
        }

        private string GenerateJwt(User user)
        {
            SymmetricSecurityKey securityKey = _authOptions.GetSymmetricSecurityKey();
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            
            _db.Entry(user).Reference(u => u.Role).Load();
            
            List<Claim> claims = new List<Claim>()
            {
                new Claim("id", user.Id.ToString()),
                new Claim("role", user.Role.RoleName),
            };

            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: _authOptions.Issuer,
                audience: _authOptions.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_authOptions.TokenLifeTime),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }
    }
}