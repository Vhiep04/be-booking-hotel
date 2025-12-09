using be_booking_hotel.DTOs;
using be_booking_hotel.DTOs.Auth;
using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Interfaces;
using be_booking_hotel.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace be_booking_hotel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthRepository authRepository,
            IConfiguration configuration,
            IEmailService emailService,
            IOtpService otpService,
            ILogger<AuthController> logger)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _emailService = emailService;
            _otpService = otpService;
            _logger = logger;
        }

        
        /// Đăng ký tài khoản mới - Gửi OTP qua email
        
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var existingUser = await _authRepository.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                // Nếu user đã tồn tại nhưng chưa verify email
                if (!existingUser.EmailConfirmed)
                {
                    // Generate OTP mới và gửi lại
                    var newOtp = _otpService.GenerateOtp();
                    _otpService.SaveOtpToSession(existingUser.Email, existingUser.Id, newOtp, "Registration");

                    try
                    {
                        await _emailService.SendOtpEmailAsync(
                            existingUser.Email,
                            $"{existingUser.FirstName} {existingUser.LastName}",
                            newOtp
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to send OTP: {ex.Message}");
                        return StatusCode(500, new
                        {
                            success = false,
                            message = "Failed to send verification email"
                        });
                    }

                    return Ok(new
                    {
                        success = true,
                        message = "Email already registered but not verified. New OTP has been sent.",
                        data = new
                        {
                            email = existingUser.Email,
                            requiresVerification = true
                        }
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "Email already registered and verified"
                });
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                BirthDate = model.BirthDate,
                AvatarUrl = null,
                CreatedAt = DateTime.Now,
                EmailConfirmed = false
            };

            var result = await _authRepository.CreateUserAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Registration failed",
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            await _authRepository.AddToRoleAsync(user, "User");

            // Generate và lưu OTP vào session
            var otpCode = _otpService.GenerateOtp();
            _otpService.SaveOtpToSession(user.Email, user.Id, otpCode, "Registration");

            // Gửi email OTP
            try
            {
                await _emailService.SendOtpEmailAsync(
                    user.Email,
                    $"{user.FirstName} {user.LastName}",
                    otpCode
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send OTP email: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Registration successful but failed to send verification email. Please use resend OTP.",
                    data = new { email = user.Email }
                });
            }

            return Ok(new
            {
                success = true,
                message = "Registration successful. Please check your email for OTP verification code.",
                data = new
                {
                    email = user.Email,
                    expiresIn = "5 minutes"
                }
            });
        }

        
        /// Xác thực OTP sau khi đăng ký
        
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var isValid = _otpService.ValidateOtp(model.Email, model.OtpCode, "Registration");

            if (!isValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid or expired OTP code"
                });
            }

            var user = await _authRepository.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            // Xác thực email
            user.EmailConfirmed = true;
            await _authRepository.UpdateUserAsync(user);

            // Gửi email chào mừng
            try
            {
                await _emailService.SendWelcomeEmailAsync(
                    user.Email!,
                    $"{user.FirstName} {user.LastName}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to send welcome email: {ex.Message}");
            }

            return Ok(new
            {
                success = true,
                message = "Email verified successfully. You can login now.",
                data = new
                {
                    userId = user.Id,
                    email = user.Email,
                    isVerified = user.EmailConfirmed
                }
            });
        }

        /// Gửi lại OTP
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var user = await _authRepository.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Email not found"
                });
            }

            if (user.EmailConfirmed)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Email already verified"
                });
            }

            // Check rate limit
            if (!_otpService.CanResendOtp(model.Email, "Registration"))
            {
                var otpData = _otpService.GetOtpData(model.Email, "Registration");
                var cooldownMinutes = int.Parse(_configuration["OtpSettings:ResendCooldownMinutes"] ?? "1");

                return StatusCode(429, new
                {
                    success = false,
                    message = $"Please wait {cooldownMinutes} minute(s) before requesting a new OTP",
                    remainingAttempts = 3 - (otpData?.ResendAttempts ?? 0)
                });
            }

            // Generate OTP mới
            var otpCode = _otpService.GenerateOtp();
            _otpService.SaveOtpToSession(user.Email, user.Id, otpCode, "Registration");
            _otpService.IncrementResendAttempt(user.Email, "Registration");

            // Gửi email
            try
            {
                await _emailService.SendOtpEmailAsync(
                    user.Email,
                    $"{user.FirstName} {user.LastName}",
                    otpCode
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send OTP: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to send OTP email",
                    error = ex.Message
                });
            }
            return Ok(new
            {
                success = true,
                message = "OTP has been resent to your email",
                data = new
                {
                    email = user.Email,
                    expiresIn = "5 minutes"
                }
            });
        }

        /// Đăng nhập - Chỉ cho phép nếu đã verify email
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var user = await _authRepository.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid email or password"
                });
            }

            // Kiểm tra email đã verify chưa
            if (!user.EmailConfirmed)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Please verify your email before logging in",
                    requiresVerification = true,
                    email = user.Email
                });
            }

            var result = await _authRepository.CheckPasswordSignInAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid email or password"
                });
            }

            var token = await GenerateJwtToken(user);
            var expiration = DateTime.Now.AddMinutes(
                Convert.ToDouble(_configuration["JwtSettings:ExpirationMinutes"]));

            return Ok(new
            {
                success = true,
                message = "Login successful",
                data = new AuthResponseDto
                {
                    Token = token,
                    UserId = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber ?? "",
                    BirthDate = user.BirthDate,
                    AvatarUrl = user.AvatarUrl,
                    Expiration = expiration
                }
            });
        }

        
        /// Lấy thông tin user hiện tại
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "User not authenticated"
                });
            }

            var user = await _authRepository.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    userId = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    fullName = $"{user.FirstName} {user.LastName}".Trim(),
                    phoneNumber = user.PhoneNumber,
                    birthDate = user.BirthDate,
                    avatarUrl = user.AvatarUrl,
                    createdAt = user.CreatedAt
                }
            });
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var roles = await _authRepository.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}