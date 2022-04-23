using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Models.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly TaskService _taskService;

        public TaskController(TaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpPost]
        public async Task<IActionResult> StartTask([FromBody]StartTask startTaskModel)
        {
            try
            {
                await _taskService.StartTask(startTaskModel);

                return Ok("Task started");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> StopTask([FromQuery] Guid taskId)
        {
            try
            {
                int userId = int.Parse(this.User.Claims.First(i => i.Type == "id").Value); //getting from token

                await _taskService.StopTask(taskId, userId);

                return Ok("Task stopped");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> StopTaskByAdmin([FromQuery] Guid taskId)
        {
            try
            {
                await _taskService.StopTaskByAdmin(taskId);

                return Ok("Task stopped");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}