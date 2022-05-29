using System;
using System.Threading.Tasks;
using Api.Options;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class GlobalParametersController : ControllerBase
    {
        private readonly GlobalParametersService _globalParametersService;

        public GlobalParametersController(GlobalParametersService globalParametersService)
        {
            _globalParametersService = globalParametersService;
        }
        
        [HttpGet]
        public IActionResult GetGlobalParameters()
        {
            try
            {
                GlobalParameters globalParameters = _globalParametersService.GetGlobalParameters();

                return Ok(globalParameters);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> SetGlobalParameters([FromBody] GlobalParameters newGlobalParameters)
        {
            try
            {
                GlobalParameters globalParameters =
                    await _globalParametersService.SetGlobalParameters(newGlobalParameters);

                return Ok(globalParameters);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAvailableProgramVersions()
        {
            try
            {
                string[] availableProgramVersions = _globalParametersService.GetAvailableProgramVersions();
                
                return Ok(availableProgramVersions);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}