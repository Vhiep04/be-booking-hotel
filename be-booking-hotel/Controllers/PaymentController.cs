using be_booking_hotel.DTOs.Payment;
using be_booking_hotel.Models.Vnpay;
using be_booking_hotel.Repositories.Interfaces;
using be_booking_hotel.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace be_booking_hotel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly IEmailService _emailService;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IVnPayService vnPayService,
            IEmailService emailService,
            IPaymentRepository paymentRepository,
            ILogger<PaymentController> logger)
        {
            _vnPayService = vnPayService;
            _emailService = emailService;
            _paymentRepository = paymentRepository;
            _logger = logger;
        }

        [HttpPost("create-payment")]
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Ok(new { paymentUrl = url });
        }

        [HttpGet]
        public IActionResult PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            return Ok(response);
        }

        [HttpPost("send-receipt")]
        public async Task<IActionResult> SendReceipt([FromBody] SendReceiptRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { message = "Email is required" });

            // 1. Tạo Reservation
            int reservationId;
            try
            {
                reservationId = await _paymentRepository.CreateReservationAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create reservation: {Error}", ex.Message);
                return StatusCode(500, new { message = "Failed to save reservation" });
            }

            // 2. Lưu Payment
            try
            {
                await _paymentRepository.CreatePaymentAsync(reservationId, request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save payment: {Error}", ex.Message);
                return StatusCode(500, new { message = "Failed to save payment" });
            }

            // 3. Gửi email — không throw nếu fail
            var isEmailSent = false;
            try
            {
                await _emailService.SendPaymentReceiptAsync(request.Email, request.Name, request);
                isEmailSent = true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to send receipt email to {Email}: {Error}", request.Email, ex.Message);
            }

            return Ok(new
            {
                success = true,
                reservationId,
                emailSent = isEmailSent,
                message = isEmailSent
                    ? "Đặt phòng thành công, biên lai đã được gửi đến email của bạn"
                    : "Đặt phòng thành công nhưng không thể gửi email biên lai"
            });
        }
    }
}
