using be_booking_hotel.DTOs;
using be_booking_hotel.DTOs.Auth;
using be_booking_hotel.Helpers;
using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Interfaces;
using be_booking_hotel.Services.Implements;
using be_booking_hotel.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using be_booking_hotel.Repositories.Implementations;
using Microsoft.SqlServer.Server;

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
        private readonly INotificationService _notificationService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthRepository authRepository,
            IConfiguration configuration,
            IEmailService emailService,
            IOtpService otpService,
            INotificationService notificationService,
            ILogger<AuthController> logger)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _emailService = emailService;
            _otpService = otpService;
            _notificationService = notificationService;
            _logger = logger;
        }

        
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
                        message = "OTP sent to email.",
                        data = new
                        {
                            email = existingUser.Email,
                            requiresVerification = true
                        }
                    });
                }

                return Conflict(new
                {
                    success = false,
                    message = "Địa chỉ Email đã được sử dụng"
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
                await _notificationService.NotifyNewUserRegistered(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send notification: {ex.Message}");
                // Không return, vẫn tiếp tục
            }

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
                    message = "Registration successful but failed to send verification email.",
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

        /// Bước 1: Quên mật khẩu - Gửi OTP qua email
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var user = await _authRepository.FindByEmailAsync(model.Email);

            // Luồng 2.1: Email không tồn tại → vẫn trả 200 OK để tránh lộ thông tin
            if (user == null || !user.EmailConfirmed)
            {
                return Ok(new
                {
                    success = true,
                    message = "If this email is registered, an OTP has been sent."
                });
            }

            // Tái sử dụng OtpService với purpose = "ForgotPassword"
            var otpCode = _otpService.GenerateOtp();
            _otpService.SaveOtpToSession(user.Email!, user.Id, otpCode, "ForgotPassword");

            // Luồng 2.2: Gửi email thất bại
            try
            {
                await _emailService.SendPasswordResetOtpAsync(
                    user.Email!,
                    $"{user.FirstName} {user.LastName}",
                    otpCode
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send password reset OTP: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to send OTP email. Please try again."
                });
            }

            return Ok(new
            {
                success = true,
                message = "If this email is registered, an OTP has been sent.",
                data = new { expiresIn = "5 minutes" }
            });
        }

        /// Bước 2: Xác thực OTP - Trả về resetToken tạm thời
        [HttpPost("verify-reset-otp")]
        public async Task<IActionResult> VerifyResetOtp([FromBody] VerifyResetOtpDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            // Luồng 3.1 / 3.2: OTP sai hoặc hết hạn
            var isValid = _otpService.ValidateOtp(model.Email, model.OtpCode, "ForgotPassword");
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
                return NotFound(new { success = false, message = "User not found" });
            }

            // Tạo resetToken (GUID) và lưu vào session, có hiệu lực 10 phút
            var resetToken = Guid.NewGuid().ToString();
            var resetData = new OtpData
            {
                OtpCode = resetToken,       // Tái dụng OtpData để lưu token
                Email = user.Email!,
                UserId = user.Id,
                Purpose = "PasswordReset",
                ExpiresAt = DateTime.Now.AddMinutes(10),
                ResendAttempts = 0,
                LastResendAt = DateTime.Now
            };

            var session = HttpContext.Session;
            var sessionKey = $"reset_token_{model.Email}";
            session.SetString(sessionKey, System.Text.Json.JsonSerializer.Serialize(resetData));

            return Ok(new
            {
                success = true,
                message = "OTP verified. Use the reset token to set a new password.",
                data = new
                {
                    email = user.Email,
                    resetToken,
                    expiresIn = "10 minutes"
                }
            });
        }

        /// Bước 3: Đặt lại mật khẩu
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            // Xác thực resetToken từ session
            var sessionKey = $"reset_token_{model.Email}";
            var tokenJson = HttpContext.Session.GetString(sessionKey);

            if (string.IsNullOrEmpty(tokenJson))
            {
                return BadRequest(new { success = false, message = "Reset token not found or expired" });
            }

            OtpData? resetData;
            try
            {
                resetData = System.Text.Json.JsonSerializer.Deserialize<OtpData>(tokenJson);
            }
            catch
            {
                return BadRequest(new { success = false, message = "Invalid reset token" });
            }

            if (resetData == null || resetData.OtpCode != model.ResetToken)
            {
                return BadRequest(new { success = false, message = "Invalid reset token" });
            }

            if (resetData.ExpiresAt < DateTime.Now)
            {
                HttpContext.Session.Remove(sessionKey);
                return BadRequest(new { success = false, message = "Reset token has expired. Please start again." });
            }

            var user = await _authRepository.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            // Đặt lại mật khẩu qua Identity
            var token = await _authRepository.GeneratePasswordResetTokenAsync(user);
            var result = await _authRepository.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
            {
                // Luồng 4.1: Mật khẩu yếu (Identity validation)
                return BadRequest(new
                {
                    success = false,
                    message = "Password reset failed",
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            // Xóa token khỏi session sau khi dùng
            HttpContext.Session.Remove(sessionKey);

            _logger.LogInformation($"Password reset successful for {model.Email}");

            return Ok(new
            {
                success = true,
                message = "Password has been reset successfully. You can now login with your new password."
            });
        }

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

        [HttpPost("login-google-code")]
        public async Task<IActionResult> LoginWithGoogleCode([FromBody] GoogleCodeDto model)
        {
            // Đổi authorization_code lấy token từ Google
            using var httpClient = new HttpClient();

            var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = model.Code,
                ["client_id"] = _configuration["Authentication:Google:ClientId"]!,
                ["client_secret"] = _configuration["Authentication:Google:ClientSecret"]!,
                ["redirect_uri"] = "postmessage",   // bắt buộc khi dùng popup
                ["grant_type"] = "authorization_code"
            });

            var tokenResponse = await httpClient.PostAsync(
                "https://oauth2.googleapis.com/token", tokenRequest);

            if (!tokenResponse.IsSuccessStatusCode)
                return Unauthorized(new { success = false, message = "Failed to exchange Google code" });

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = System.Text.Json.JsonDocument.Parse(tokenJson).RootElement;
            var idToken = tokenData.GetProperty("id_token").GetString()!;

            // Verify idToken (tái dụng logic cũ)
            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Authentication:Google:ClientId"] }
                });
            }
            catch
            {
                return Unauthorized(new { success = false, message = "Invalid Google token" });
            }

            // ... phần tìm/tạo user giữ nguyên như endpoint login-google cũ
            var user = await _authRepository.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                // 3a. Chưa có account → tạo mới (không cần OTP vì Google đã verify email)
                user = new ApplicationUser
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    FirstName = payload.GivenName ?? "",
                    LastName = payload.FamilyName ?? "",
                    AvatarUrl = payload.Picture,
                    EmailConfirmed = true, // ✅ Google đã verify rồi
                    CreatedAt = DateTime.Now
                };

                var result = await _authRepository.CreateUserAsync(user, Guid.NewGuid().ToString() + "!Aa1");
                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to create account",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                await _authRepository.AddToRoleAsync(user, "User");

                try { await _notificationService.NotifyNewUserRegistered(user); }
                catch { /* không block luồng */ }
            }
            else
            {
                // 3b. Đã có account nhưng chưa verify → tự động verify luôn
                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    await _authRepository.UpdateUserAsync(user);
                }

                // Cập nhật avatar nếu chưa có
                if (string.IsNullOrEmpty(user.AvatarUrl) && !string.IsNullOrEmpty(payload.Picture))
                {
                    user.AvatarUrl = payload.Picture;
                    await _authRepository.UpdateUserAsync(user);
                }
            }

            // 4. Tạo JWT token của hệ thống (giống login thường)
            var token = await GenerateJwtToken(user);
            var expiration = DateTime.Now.AddMinutes(
                Convert.ToDouble(_configuration["JwtSettings:ExpirationMinutes"]));

            return Ok(new
            {
                success = true,
                message = "Google login successful",
                data = new AuthResponseDto
                {
                    Token = token,
                    UserId = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber ?? "",
                    BirthDate = user.BirthDate,
                    AvatarUrl = user.AvatarUrl ?? "",
                    Expiration = expiration
                }
            });
        }

        [HttpPost("login-google")]
            public async Task<IActionResult> LoginWithGoogle([FromBody] GoogleLoginDto model)
            {
                // 1. Verify id_token với Google
                GoogleJsonWebSignature.Payload payload;
                try
                {
                    var settings = new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _configuration["Authentication:Google:ClientId"] }
                    };
                    payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken, settings);
                }
                catch (InvalidJwtException)
                {
                    return Unauthorized(new { success = false, message = "Invalid Google token" });
                }

                // 2. Tìm user theo email
                var user = await _authRepository.FindByEmailAsync(payload.Email);

                if (user == null)
                {
                    // 3a. Chưa có account → tạo mới (không cần OTP vì Google đã verify email)
                    user = new ApplicationUser
                    {
                        UserName = payload.Email,
                        Email = payload.Email,
                        FirstName = payload.GivenName ?? "",
                        LastName = payload.FamilyName ?? "",
                        AvatarUrl = payload.Picture,
                        EmailConfirmed = true, // ✅ Google đã verify rồi
                        CreatedAt = DateTime.Now
                    };

                    var result = await _authRepository.CreateUserAsync(user, Guid.NewGuid().ToString() + "!Aa1");
                    if (!result.Succeeded)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to create account",
                            errors = result.Errors.Select(e => e.Description)
                        });
                    }

                    await _authRepository.AddToRoleAsync(user, "User");

                    try { await _notificationService.NotifyNewUserRegistered(user); }
                    catch { /* không block luồng */ }
                }
                else
                {
                    // 3b. Đã có account nhưng chưa verify → tự động verify luôn
                    if (!user.EmailConfirmed)
                    {
                        user.EmailConfirmed = true;
                        await _authRepository.UpdateUserAsync(user);
                    }

                    // Cập nhật avatar nếu chưa có
                    if (string.IsNullOrEmpty(user.AvatarUrl) && !string.IsNullOrEmpty(payload.Picture))
                    {
                        user.AvatarUrl = payload.Picture;
                        await _authRepository.UpdateUserAsync(user);
                    }
                }

                // 4. Tạo JWT token của hệ thống (giống login thường)
                var token = await GenerateJwtToken(user);
                var expiration = DateTime.Now.AddMinutes(
                    Convert.ToDouble(_configuration["JwtSettings:ExpirationMinutes"]));

                return Ok(new
                {
                    success = true,
                    message = "Google login successful",
                    data = new AuthResponseDto
                    {
                        Token = token,
                        UserId = user.Id,
                        Email = user.Email!,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        PhoneNumber = user.PhoneNumber ?? "",
                        BirthDate = user.BirthDate,
                        AvatarUrl = user.AvatarUrl ?? "",
                        Expiration = expiration
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