using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Cms;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface ICmsService
    {
        Task<BaseResposeDto> DeleteUserAsync(Guid id);
        Task<ResponseGetUsersCmsDto> GetUserAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status);
        Task<ResponseGetUserByIdCmsDto> GetUserByIdAsync(Guid id);
        Task<BaseResposeDto> UpdateUserAsync(RequestUpdateUserCmsDto request, Guid id);
    }
}
