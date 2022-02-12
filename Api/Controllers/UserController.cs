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
        public async Task<List<User>> GetAll()
        {
            return await _db.Users.ToListAsync();
        }
    }
}