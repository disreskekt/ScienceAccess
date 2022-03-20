using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models.Dtos;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        
        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword changePasswordModel)
        {
            try
            {
                int userId = int.Parse(this.User.Claims.First(i => i.Type == "id").Value); //getting from token

                await _userService.ChangePassword(changePasswordModel, userId);

                return Ok("Password changed");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<AllUsersDto> userDtosList = await _userService.GetAll();

                return Ok(userDtosList);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUser([FromQuery] int id)
        {
            try
            {
                UserDto userDto = await _userService.GetUser(id);

                return Ok(userDto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> GetMyself()
        {
            try
            {
                int userId = int.Parse(this.User.Claims.First(i => i.Type == "id").Value); //getting from token

                GetMyselfDto myselfDto = await _userService.GetMyself(userId);

                return Ok(myselfDto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}