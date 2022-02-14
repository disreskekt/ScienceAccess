using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly Context _db; 
        
        public UserController(Context context)
        {
            _db = context;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<User> usersList = await _db.Users.ToListAsync();

                return Ok(usersList);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}