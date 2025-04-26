using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Authentication;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Authentication;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService) {
            _authenticationService = authenticationService;
        }
        
        [HttpPost("register")]
        public async Task<ActionResult<ResponseAuthenticationDto>> Register(RequestRegisterDto request)
        {
            var response = await _authenticationService.RegisterAsync(request);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<ResponseAuthenticationDto>> Login(RequestLoginDto request)
        {
            var response = await _authenticationService.LoginAsync(request);
            return StatusCode(response.StatusCode, response);
        }
    }
}
