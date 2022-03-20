using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;
using Api.Helpers;
using Api.Models;
using Api.Models.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly Context _db;
        private readonly IMapper _mapper;

        public UserController(Context context, IMapper mapper)
        {
            _db = context;
            _mapper = mapper;
        }
        
        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword changePasswordModel)
        {
            try
            {
                int userId = int.Parse(this.User.Claims.First(i => i.Type == "id").Value); //getting from token

                User user = await _db.Users.FindAsync(userId);

                if (user is null)
                {
                    return BadRequest("User doesn't exist");
                }

                if (user.Password != changePasswordModel.OldPassword.GenerateVerySecretHash(user.Email))
                {
                    return BadRequest("Invalid password");
                }

                user.Password = changePasswordModel.NewPassword.GenerateVerySecretHash(user.Email);

                await _db.SaveChangesAsync();

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
                List<User> usersList = await _db.Users.ToListAsync();

                List<AllUsersDto> userDtosList = _mapper.Map<List<AllUsersDto>>(usersList);
                
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
                User user = await _db.Users
                    .Include(user => user.Role)
                    .Include(user => user.Tickets)
                    .ThenInclude(ticket => ticket.Task)
                    .FirstOrDefaultAsync(user => user.Id == id);

                if (user is null)
                {
                    return BadRequest("User doesn't exist");
                }

                UserDto userDto = _mapper.Map<UserDto>(user);

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

                User user = await _db.Users
                    .Include(user => user.Role)
                    .Include(user => user.Tickets)
                    .ThenInclude(ticket => ticket.Task)
                    .FirstOrDefaultAsync(user => user.Id == userId);

                if (user is null)
                {
                    return BadRequest("User doesn't exist");
                }
                
                GetMyselfDto getMyselfDto = _mapper.Map<GetMyselfDto>(user);
                
                return Ok(getMyselfDto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser([FromQuery] int id)
        {
            try
            {
                User user = await _db.Users.FindAsync(id);

                if (user is null)
                {
                    return BadRequest("User doesn't exist");
                }

                _db.Users.Remove(user);
                await _db.SaveChangesAsync();

                return Ok("User deleted");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}