using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.Common;

namespace TayNinhTourApi.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CMSController : ControllerBase
    {
        [Authorize(Roles = Constants.RoleAdminName)]
        [HttpGet("admin")]
        public IActionResult GetAdmin()
        {
            return Ok("This is the admin endpoint.");
        }
    }
}
