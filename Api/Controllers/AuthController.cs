using Api.Models;
using Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IOptions<AuthOptions> _authOptions;

        public AuthController(IOptions<AuthOptions> authOptions)
        {
            _authOptions = authOptions;
        }
        
        [Route("[action]")]
        [HttpPost]
        public IActionResult Login([FromBody] LoginModel loginModel)
        {
            UserModel user = AuthenticateUser(loginModel.Email, loginModel.Password);

            if (user is not null)
            {
                //todo generate JWT
            }

            return Unauthorized();
        }

        private UserModel AuthenticateUser(string email, string password)
        {
            return new UserModel(); //todo ретурн из базы
        }
    }
}