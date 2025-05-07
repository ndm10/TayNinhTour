using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Cms;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Constants.RoleAdminName)]
    public class CmsController : ControllerBase
    {
        private readonly ICmsService _cmsService;

        public CmsController(ICmsService cmsService)
        {
            _cmsService = cmsService;
        }

        [HttpGet("user")]
        public async Task<ActionResult<ResponseGetUsersCmsDto>> GetUser(int? pageIndex, int? pageSize, string? textSearch, bool? status)
        {
            var response = await _cmsService.GetUserAsync(pageIndex, pageSize, textSearch, status);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("user/{id}")]
        public async Task<ActionResult<ResponseGetUserByIdCmsDto>> GetUserById(Guid id)
        {
            var response = await _cmsService.GetUserByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPatch("user/{id}")]
        public async Task<ActionResult<BaseResposeDto>> UpdateUser(RequestUpdateUserCmsDto request, Guid id)
        {
            var response = await _cmsService.UpdateUserAsync(request, id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("user/{id}")]
        public async Task<ActionResult<BaseResposeDto>> DeleteUser(Guid id)
        {
            var response = await _cmsService.DeleteUserAsync(id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
