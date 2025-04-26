using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Authentication;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Authentication;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface IAuthenticationService
    {
        Task<ResponseAuthenticationDto> RegisterAsync(RequestRegisterDto request);
        Task<ResponseAuthenticationDto> LoginAsync(RequestLoginDto request);
    }
}
