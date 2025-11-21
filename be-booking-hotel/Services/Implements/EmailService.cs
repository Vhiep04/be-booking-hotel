using be_booking_hotel.Services.Interfaces;
using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;


namespace be_booking_hotel.Services.Implements
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendOtpEmailAsync(string toEmail, string recipientName, string otpCode)
        {
            var subject = "Verify Your Email - Hotel Booking";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .otp-box {{ background: white; border: 2px dashed #667eea; padding: 20px; text-align: center; margin: 20px 0; border-radius: 8px; }}
                        .otp-code {{ font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #667eea; }}
                        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🏨 Hotel Booking</h1>
                            <p>Email Verification</p>
                        </div>
                        <div class='content'>
                            <h2>Hello {recipientName}!</h2>
                            <p>Thank you for registering with Hotel Booking. To complete your registration, please verify your email address using the OTP code below:</p>
                            
                            <div class='otp-box'>
                                <p style='margin: 0; color: #666;'>Your OTP Code:</p>
                                <div class='otp-code'>{otpCode}</div>
                                <p style='margin: 10px 0 0 0; color: #999; font-size: 14px;'>This code will expire in 5 minutes</p>
                            </div>
                            
                            <p><strong>⚠️ Security Notice:</strong></p>
                            <ul>
                                <li>Never share this OTP with anyone</li>
                                <li>Our staff will never ask for your OTP</li>
                                <li>This code expires in 5 minutes</li>
                            </ul>
                            
                            <p>If you didn't request this verification, please ignore this email.</p>
                        </div>
                        <div class='footer'>
                            <p>© 2024 Hotel Booking System. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string recipientName)
        {
            var subject = "Welcome to Hotel Booking!";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🎉 Welcome to Hotel Booking!</h1>
                        </div>
                        <div class='content'>
                            <h2>Hello {recipientName}!</h2>
                            <p>Your email has been successfully verified. Welcome to our community!</p>
                            <p>You can now enjoy all features:</p>
                            <ul>
                                <li>🔍 Search hotels worldwide</li>
                                <li>💰 Compare prices and get best deals</li>
                                <li>❤️ Save your favorite hotels</li>
                                <li>📱 Manage bookings easily</li>
                            </ul>
                        </div>
                    </div>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetOtpAsync(string toEmail, string recipientName, string otpCode)
        {
            var subject = "Password Reset OTP - Hotel Booking";
            var body = $@"
                <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>Hello {recipientName},</p>
                    <p>Your OTP code: <strong>{otpCode}</strong></p>
                    <p>This code expires in 5 minutes.</p>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(
                    emailSettings["SenderName"],
                    emailSettings["SenderEmail"]
                ));

                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = body };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                await client.ConnectAsync(
                    emailSettings["SmtpServer"],
                    int.Parse(emailSettings["SmtpPort"]!),
                    SecureSocketOptions.StartTls
                );

                await client.AuthenticateAsync(
                    emailSettings["Username"],
                    emailSettings["Password"]
                );

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email to {toEmail}: {ex.Message}");
                throw;
            }
        }
    }
}
