using System;
using System.Threading.Tasks;
using Api.Models.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] Register register)
        {
            try
            {
                await _authService.Register(register);

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
                string token = await _authService.Login(login);

                return Ok(token);
            }
            catch (Exception e)
            {
                return Unauthorized(e.Message);
            }
        }
    }
}