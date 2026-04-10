using be_booking_hotel.Services.Interfaces;
using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;
using be_booking_hotel.DTOs.Payment;


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

        public async Task SendPaymentReceiptAsync(string toEmail, string recipientName, SendReceiptRequest receipt)
        {
            var subject = $"Biên lai thanh toán #{receipt.TransactionId} - Hotel Booking";
            var formattedAmount = string.Format("{0:N0}", receipt.Amount);
            var checkIn = receipt.CheckInDate.ToString("dd/MM/yyyy");
            var checkOut = receipt.CheckOutDate.ToString("dd/MM/yyyy");
            var nights = receipt.CheckOutDate.DayNumber - receipt.CheckInDate.DayNumber;

            var body = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; background: #f5f5f5; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #10b981 0%, #059669 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                    .content {{ background: #ffffff; padding: 30px; border-radius: 0 0 10px 10px; }}
                    .amount-box {{ background: #f0fdf4; border: 2px solid #10b981; padding: 20px; text-align: center; margin: 20px 0; border-radius: 8px; }}
                    .amount {{ font-size: 32px; font-weight: bold; color: #059669; }}
                    .section {{ margin: 20px 0; }}
                    .section-title {{ font-size: 14px; font-weight: 700; color: #059669; text-transform: uppercase; letter-spacing: 1px; border-bottom: 2px solid #d1fae5; padding-bottom: 6px; margin-bottom: 12px; }}
                    .info-table {{ width: 100%; border-collapse: collapse; }}
                    .info-table tr {{ border-bottom: 1px solid #e5e7eb; }}
                    .info-table tr:last-child {{ border-bottom: none; }}
                    .info-table td {{ padding: 10px 8px; font-size: 14px; }}
                    .info-table td:first-child {{ color: #6b7280; width: 45%; }}
                    .info-table td:last-child {{ font-weight: 600; color: #111827; text-align: right; }}
                    .badge {{ background: #d1fae5; color: #065f46; padding: 4px 12px; border-radius: 20px; font-size: 13px; font-weight: 600; }}
                    .footer {{ text-align: center; margin-top: 20px; color: #9ca3af; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>🏨 Hotel Booking</h1>
                        <p style='margin:0; opacity:0.9;'>Biên lai thanh toán</p>
                    </div>
                    <div class='content'>
                        <h2>Xin chào {recipientName}!</h2>
                        <p>Thanh toán của bạn đã được xử lý thành công. Dưới đây là chi tiết đặt phòng:</p>

                        <div class='amount-box'>
                            <p style='margin:0; color:#6b7280; font-size:13px;'>SỐ TIỀN ĐÃ THANH TOÁN</p>
                            <div class='amount'>{formattedAmount} VND</div>
                        </div>

                        <div class='section'>
                            <div class='section-title'>👤 Thông tin người đặt</div>
                            <table class='info-table'>
                                <tr>
                                    <td>Họ và tên</td>
                                    <td>{recipientName}</td>
                                </tr>
                                <tr>
                                    <td>Email</td>
                                    <td>{toEmail}</td>
                                </tr>
                                <tr>
                                    <td>Số điện thoại</td>
                                    <td>{receipt.PhoneNumber}</td>
                                </tr>
                            </table>
                        </div>

                        <div class='section'>
                            <div class='section-title'>🏨 Thông tin khách sạn</div>
                            <table class='info-table'>
                                <tr>
                                    <td>Tên khách sạn</td>
                                    <td>{receipt.HotelName}</td>
                                </tr>
                                <tr>
                                    <td>Địa chỉ</td>
                                    <td>{receipt.HotelAddress}</td>
                                </tr>
                                <tr>
                                    <td>Ngày check-in</td>
                                    <td>{checkIn}</td>
                                </tr>
                                <tr>
                                    <td>Ngày check-out</td>
                                    <td>{checkOut}</td>
                                </tr>
                                <tr>
                                    <td>Số đêm</td>
                                    <td>{nights} đêm</td>
                                </tr>
                            </table>
                        </div>

                        <div class='section'>
                            <div class='section-title'>💳 Thông tin giao dịch</div>
                            <table class='info-table'>
                                <tr>
                                    <td>Mô tả đơn hàng</td>
                                    <td>{receipt.OrderDescription}</td>
                                </tr>
                                <tr>
                                    <td>Mã giao dịch</td>
                                    <td style='color:#059669;'>{receipt.TransactionId}</td>
                                </tr>
                                <tr>
                                    <td>Mã đơn hàng</td>
                                    <td>{receipt.OrderId}</td>
                                </tr>
                                <tr>
                                    <td>Phương thức thanh toán</td>
                                    <td>{receipt.PaymentMethod}</td>
                                </tr>
                                <tr>
                                    <td>Trạng thái</td>
                                    <td><span class='badge'>✓ Thành công</span></td>
                                </tr>
                                <tr>
                                    <td>Thời gian</td>
                                    <td>{DateTime.Now:dd/MM/yyyy HH:mm:ss}</td>
                                </tr>
                            </table>
                        </div>

                        <p style='color:#6b7280; font-size:13px;'>
                            Nếu bạn có bất kỳ thắc mắc nào về giao dịch này, vui lòng liên hệ với chúng tôi kèm mã giao dịch 
                            <strong>{receipt.TransactionId}</strong>.
                        </p>
                    </div>
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} Hotel Booking System. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendCashBookingConfirmationAsync(string toEmail, string recipientName, CashReservationRequest request)
        {
            var subject = $"Xác nhận đặt phòng - Hotel Booking";
            var checkIn = request.CheckInDate.ToString("dd/MM/yyyy");
            var checkOut = request.CheckOutDate.ToString("dd/MM/yyyy");
            var nights = request.CheckOutDate.DayNumber - request.CheckInDate.DayNumber;
            var formattedAmount = string.Format("{0:N0}", request.Amount);

            var body = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; background: #f5f5f5; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                    .content {{ background: #ffffff; padding: 30px; border-radius: 0 0 10px 10px; }}
                    .section {{ margin: 20px 0; }}
                    .section-title {{ font-size: 14px; font-weight: 700; color: #1d4ed8; text-transform: uppercase; letter-spacing: 1px; border-bottom: 2px solid #dbeafe; padding-bottom: 6px; margin-bottom: 12px; }}
                    .info-table {{ width: 100%; border-collapse: collapse; }}
                    .info-table tr {{ border-bottom: 1px solid #e5e7eb; }}
                    .info-table tr:last-child {{ border-bottom: none; }}
                    .info-table td {{ padding: 10px 8px; font-size: 14px; }}
                    .info-table td:first-child {{ color: #6b7280; width: 45%; }}
                    .info-table td:last-child {{ font-weight: 600; color: #111827; text-align: right; }}
                    .notice-box {{ background: #fef9c3; border: 1px solid #fbbf24; padding: 16px; border-radius: 8px; margin: 20px 0; }}
                    .badge-pending {{ background: #fef3c7; color: #92400e; padding: 4px 12px; border-radius: 20px; font-size: 13px; font-weight: 600; }}
                    .amount-row td {{ font-size: 16px; color: #1d4ed8 !important; }}
                    .footer {{ text-align: center; margin-top: 20px; color: #9ca3af; font-size: 12px; }}
                </style>
            </head>
            <body>
            <div class='container'>
                <div class='header'>
                    <h1>🏨 Hotel Booking</h1>
                    <p style='margin:0; opacity:0.9;'>Xác nhận đặt phòng</p>
                </div>
                <div class='content'>
                    <h2>Xin chào {recipientName}!</h2>
                    <p>Đặt phòng của bạn đã được ghi nhận. Vui lòng thanh toán tại quầy lễ tân khi nhận phòng.</p>

                    <div class='notice-box'>
                        ⚠️ <strong>Lưu ý:</strong> Đây là xác nhận đặt phòng, <u>chưa phải biên lai thanh toán</u>. 
                        Vui lòng thanh toán bằng tiền mặt tại khách sạn khi check-in.
                    </div>

                    <div class='section'>
                        <div class='section-title'>👤 Thông tin người đặt</div>
                        <table class='info-table'>
                            <tr>
                                <td>Họ và tên</td>
                                <td>{recipientName}</td>
                            </tr>
                            <tr>
                                <td>Email</td>
                                <td>{toEmail}</td>
                            </tr>
                            <tr>
                                <td>Số điện thoại</td>
                                <td>{request.PhoneNumber}</td>
                            </tr>
                        </table>
                    </div>

                    <div class='section'>
                        <div class='section-title'>🏨 Thông tin khách sạn</div>
                        <table class='info-table'>
                            <tr>
                                <td>Tên khách sạn</td>
                                <td>{request.HotelName}</td>
                            </tr>
                            <tr>
                                <td>Địa chỉ</td>
                                <td>{request.HotelAddress}</td>
                            </tr>
                            <tr>
                                <td>Ngày check-in</td>
                                <td>{checkIn}</td>
                            </tr>
                            <tr>
                                <td>Ngày check-out</td>
                                <td>{checkOut}</td>
                            </tr>
                            <tr>
                                <td>Số đêm</td>
                                <td>{nights} đêm</td>
                            </tr>
                        </table>
                    </div>

                <div class='section'>
                    <div class='section-title'>💰 Thông tin thanh toán</div>
                    <table class='info-table'>
                        <tr>
                            <td>Phương thức</td>
                            <td>Tiền mặt tại quầy</td>
                        </tr>
                        <tr>
                            <td>Trạng thái</td>
                            <td><span class='badge-pending'>⏳ Chờ thanh toán</span></td>
                        </tr>
                        <tr class='amount-row'>
                            <td>Tổng tiền cần thanh toán</td>
                            <td style='color:#1d4ed8; font-size:16px;'>{formattedAmount} VND</td>
                        </tr>
                    </table>
                </div>

                <p style='color:#6b7280; font-size:13px;'>
                    Nếu bạn cần hỗ trợ, vui lòng liên hệ với chúng tôi qua email hoặc hotline.
                </p>
            </div>
                <div class='footer'>
                    <p>© {DateTime.Now.Year} Hotel Booking System. All rights reserved.</p>
                </div>
            </div>
        </body>
        </html>";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
