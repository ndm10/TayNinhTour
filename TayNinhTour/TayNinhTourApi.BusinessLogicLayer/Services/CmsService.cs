using AutoMapper;
using LinqKit;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Cms;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    public class CmsService : ICmsService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public CmsService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<BaseResposeDto> DeleteUserAsync(Guid id)
        {
            // Find user by id
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null || user.IsDeleted)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }

            // Delete user
            user.IsDeleted = true;

            // Save changes to database
            await _userRepository.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "User deleted successfully"
            };
        }

        public async Task<ResponseGetUsersCmsDto> GetUserAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status)
        {
            // Set page index and page size
            var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
            var pageSizeValue = pageSize ?? Constants.PageSizeDefault;

            // Create predicate for filtering
            var predicate = PredicateBuilder.New<User>(x => !x.IsDeleted);

            if (!string.IsNullOrEmpty(textSearch))
            {
                predicate = predicate.And(x => x.Name.Contains(textSearch, StringComparison.OrdinalIgnoreCase) || x.Email.Contains(textSearch, StringComparison.OrdinalIgnoreCase));
            }

            if (status.HasValue)
            {
                predicate = predicate.And(x => x.IsActive == status.Value);
            }

            // Get users from repository
            var users = await _userRepository.GenericGetPaginationAsync(pageIndexValue, pageSizeValue, predicate);

            var totalUsers = users.Count();
            var totalPages = (int)Math.Ceiling((double)totalUsers / pageSizeValue);

            return new ResponseGetUsersCmsDto
            {
                StatusCode = 200,
                Data = _mapper.Map<List<UserCmsDto>>(users),
                TotalRecord = totalUsers,
                TotalPages = totalPages,
            };
        }

        public async Task<ResponseGetUserByIdCmsDto> GetUserByIdAsync(Guid id)
        {
            // Find user by id
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null || user.IsDeleted)
            {
                return new ResponseGetUserByIdCmsDto
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }

            return new ResponseGetUserByIdCmsDto
            {
                StatusCode = 200,
                Data = _mapper.Map<UserCmsDto>(user)
            };
        }

        public async Task<BaseResposeDto> UpdateUserAsync(RequestUpdateUserCmsDto request, Guid id)
        {
            // Find user by email
            var existingUser = await _userRepository.GetByIdAsync(id);

            if (existingUser == null)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }

            // Update user
            existingUser.Name = request.Name ?? existingUser.Name;
            existingUser.PhoneNumber = request.PhoneNumber ?? existingUser.PhoneNumber;
            existingUser.Avatar = request.Avatar ?? existingUser.Avatar;
            existingUser.IsActive = request.IsActive ?? existingUser.IsActive;

            // Save changes to database
            await _userRepository.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "User updated successfully"
            };
        }
    }
}
