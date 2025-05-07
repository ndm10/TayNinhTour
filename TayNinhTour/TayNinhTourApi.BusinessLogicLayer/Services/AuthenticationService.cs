using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _memoryCache;
        private readonly EmailSender _emailSender;

        private readonly TimeSpan _otpExpiration = TimeSpan.FromMinutes(3);
        private readonly TimeSpan _lockoutDuration = TimeSpan.FromMinutes(5);

        public AuthenticationService(IUserRepository userRepository, IMapper mapper, BcryptUtility bcryptUtility, JwtUtility jwtUtility, IRoleRepository roleRepository, IMemoryCache memoryCache, EmailSender emailSender)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _bcryptUtility = bcryptUtility;
            _jwtUtility = jwtUtility;
            _roleRepository = roleRepository;
            _memoryCache = memoryCache;
            _emailSender = emailSender;
        }

        public async Task<ResponseAuthenticationDto> RegisterAsync(RequestRegisterDto request)
        {
            // Generate OTP and save it to cache
            string otp = new Random().Next(100000, 999999).ToString();
            string otpKey = $"{request.Email}_{request.ClientIp}_OTP";
            _memoryCache.Set(otpKey, otp, _otpExpiration);

            // Send OTP to email
            await _emailSender.SendOtpRegisterAsync(request.Email, otp);

            // Find user by email
            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);

            // Register email if user not exists
            var newUser = _mapper.Map<User>(request);

            // Check if user is already registered
            if (existingUser == null)
            {

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

                await _userRepository.AddAsync(newUser);
            }
            else if (existingUser != null && existingUser.IsVerified)
            {
                existingUser.Email = request.Email;
                existingUser.Name = request.Name;
                existingUser.PasswordHash = _bcryptUtility.HashPassword(request.Password);
                existingUser.PhoneNumber = request.PhoneNumber;
                existingUser.Avatar = request.Avatar ?? existingUser.Avatar;
            }

            // Save user to database
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
            if (user == null || !_bcryptUtility.VerifyPassword(request.Password, user.PasswordHash))
            {
                return new ResponseAuthenticationDto
                {
                    StatusCode = 400,
                    Message = "Invalid email or password!"
                };
            }
            // Check is user verified
            if (!user.IsVerified)
            {
                return new ResponseAuthenticationDto
                {
                    StatusCode = 400,
                    Message = "User is not verified, back to register to verify your email!"
                };
            }

            // Check is user locked
            if (!user.IsActive)
            {
                return new ResponseAuthenticationDto
                {
                    StatusCode = 400,
                    Message = "This account is not available at this time!"
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
                Message = "Register successfully. OTP is sent to your e" +
                "" +
                "" +
                "mail!"
            };
        }

        public async Task<ResponseVerifyOtpDto> VerifyOtpAsync(RegisterVerifyOtpRequestDto request)
        {
            if (string.IsNullOrEmpty(request.ClientIp))
            {
                return new ResponseVerifyOtpDto
                {
                    StatusCode = 400,
                    Message = "Can not verify user, try again!"
                };
            }

            // Find user by email
            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            // If user not exists, return error response
            if (user == null)
            {
                return new ResponseVerifyOtpDto
                {
                    StatusCode = 400,
                    Message = "Email is not existed in the system"
                };
            }

            string failedAttemptsKey = $"FailedAttempts_IP_{request.ClientIp}";

            string lockoutKey = $"Lockout_IP_{request.ClientIp}";
            if (_memoryCache.TryGetValue(lockoutKey, out DateTime lockoutEnd) && lockoutEnd > DateTime.UtcNow)
            {
                var remainingTime = (lockoutEnd - DateTime.UtcNow).TotalMinutes;
                return new ResponseVerifyOtpDto
                {
                    StatusCode = 400,
                    Message = $"OTP is wrong, try again after {_lockoutDuration.TotalMinutes} minutes."
                };
            }

            string otpKey = $"{request.Email}_{request.ClientIp}_OTP";
            if (!_memoryCache.TryGetValue(otpKey, out string? storedOtp) || string.IsNullOrEmpty(storedOtp))
            {
                return new ResponseVerifyOtpDto
                {
                    StatusCode = 400,
                    Message = "OTP is not valid or expired."
                };
            }

            if (request.Otp == storedOtp)
            {
                _memoryCache.Remove(otpKey);
                _memoryCache.Remove(failedAttemptsKey);

                // Update user status to verified
                user.IsVerified = true;
                await _userRepository.SaveChangesAsync();

                return new ResponseVerifyOtpDto
                {
                    StatusCode = 200,
                    Message = "Your account is verified. You can login now!",
                };
            }
            else
            {
                // If OTP is incorrect, increment failed attempts
                int failedAttempts = _memoryCache.TryGetValue(failedAttemptsKey, out int attempts) ? attempts + 1 : 1;
                _memoryCache.Set(failedAttemptsKey, failedAttempts, _otpExpiration);

                if (failedAttempts >= Constants.MaxFailedAttempts)
                {
                    _memoryCache.Set(lockoutKey, DateTime.UtcNow.Add(_lockoutDuration), _lockoutDuration);
                    return new ResponseVerifyOtpDto
                    {
                        StatusCode = 400,
                        Message = $"OTP is wrong, try again after {_lockoutDuration.TotalMinutes} minutes."
                    };
                }
                else
                {
                    return new ResponseVerifyOtpDto
                    {
                        StatusCode = 400,
                        Message = $"OTP is wrong, you have {Constants.MaxFailedAttempts - failedAttempts} attempts left."
                    };
                }
            }
        }
    }
}
