using AutoMapper;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Authentication;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Authentication;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private readonly BcryptUtility _bcryptUtility;
        private readonly JwtUtility _jwtUtility;

        public AuthenticationService(IUserRepository userRepository, IMapper mapper, BcryptUtility bcryptUtility, JwtUtility jwtUtility, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _bcryptUtility = bcryptUtility;
            _jwtUtility = jwtUtility;
            _roleRepository = roleRepository;
        }

        public async Task<ResponseAuthenticationDto> RegisterAsync(RequestRegisterDto request)
        {
            // Find user by email
            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            // If user exists, return error response
            if (user != null)
            {
                return new ResponseAuthenticationDto
                {
                    StatusCode = 400,
                    Message = "User already exists"
                };
            }

            // Register email if user not exists
            var newUser = _mapper.Map<User>(request);

            // Hash password
            newUser.PasswordHash = _bcryptUtility.HashPassword(request.Password);

            // Find role by name
            var role = await _roleRepository.GetRoleByNameAsync(Constants.RoleUserName);
            if (role == null)
            {
                return new ResponseAuthenticationDto
                {
                    StatusCode = 500,
                    Message = "Role not found"
                };
            }

            // Set role id
            newUser.RoleId = role.Id;
            newUser.Role = role;

            // Save user to database
            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveChangesAsync();

            // Return success response
            return GenerateTokenAsync(newUser);
        }

        public async Task<ResponseAuthenticationDto> LoginAsync(RequestLoginDto request)
        {
            var includes = new string[]
            {
                nameof(User.Role)
            };

            // Find user by email
            var user = await _userRepository.GetUserByEmailAsync(request.Email, includes);
            // If user not exists, return error response
            if (user == null)
            {
                return new ResponseAuthenticationDto
                {
                    StatusCode = 400,
                    Message = "User not found"
                };
            }
            // Check password
            if (!_bcryptUtility.VerifyPassword(request.Password, user.PasswordHash))
            {
                return new ResponseAuthenticationDto
                {
                    StatusCode = 400,
                    Message = "Invalid password"
                };
            }
            // Return success response
            return GenerateTokenAsync(user);
        }

        private ResponseAuthenticationDto GenerateTokenAsync(User newUser)
        {
            var token = _jwtUtility.GenerateToken(newUser);

            return new ResponseAuthenticationDto
            {
                Token = token,
                RefreshToken = _jwtUtility.GenerateRefreshToken(),
                TokenExpirationTime = DateTime.UtcNow.AddDays(Constants.TokenExpiredTime),
                UserId = newUser.Id,
                Email = newUser.Email,
                Name = newUser.Name,
                PhoneNumber = newUser.PhoneNumber,
                StatusCode = 200,
                Message = "Register successfully"
            };
        }
    }
}
