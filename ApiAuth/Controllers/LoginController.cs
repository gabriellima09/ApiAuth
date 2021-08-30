using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApiAuth.Security;
using System.Threading.Tasks;

namespace ApiAuth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        [Authorize]
        [HttpGet("Ping")]
        public async Task<IActionResult> Get()
        {
            await Task.CompletedTask;
            return Ok();
        }

        [Authorize(Roles = "Test")]
        [HttpGet("TestRole")]
        public async Task<IActionResult> TestRole()
        {
            await Task.CompletedTask;
            return Ok();
        }

        [Authorize(Roles = Roles.ROLE_API_AUTH)]
        [HttpGet("ApiAdminRole")]
        public async Task<IActionResult> ApiAdminRole()
        {
            await Task.CompletedTask;
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Post(
            [FromBody]AccessCredentials credencials,
            [FromServices]AccessManager accessManager)
        {
            if (await accessManager.ValidateCredentials(credencials))
            {
                return Ok(accessManager.GenerateToken(credencials));
            }
            else
            {
                return Unauthorized(new
                {
                    Authenticated = false,
                    Message = "Falha ao autenticar"
                });
            }
        }
    }
}