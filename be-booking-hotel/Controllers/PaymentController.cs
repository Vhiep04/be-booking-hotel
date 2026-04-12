using be_booking_hotel.DTOs.Payment;
using be_booking_hotel.Models;
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
        private readonly INotificationService _notificationService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IVnPayService vnPayService,
            IEmailService emailService,
            IPaymentRepository paymentRepository,
            INotificationService notificationService,
            ILogger<PaymentController> logger)
        {
            _vnPayService = vnPayService;
            _emailService = emailService;
            _paymentRepository = paymentRepository;
            _notificationService = notificationService;
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

            Payment? payment = null;
            try
            {
                payment = await _paymentRepository.CreatePaymentAsync(reservationId, request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save payment: {Error}", ex.Message);
                return StatusCode(500, new { message = "Failed to save payment" });
            }

            // ✅ Notify payment success (VNPay)
            if (payment != null)
            {
                try
                {
                    await _notificationService.NotifyPaymentSuccess(payment);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to send payment notification: {Error}", ex.Message);
                }
            }

            var isEmailSent = false;
            try
            {
                await _emailService.SendPaymentReceiptAsync(request.Email, request.Name, request);
                isEmailSent = true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to send receipt email: {Error}", ex.Message);
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

        [HttpPost("cash-booking")]
        public async Task<IActionResult> CashBooking([FromBody] CashReservationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId) || request.RoomId == 0)
                return BadRequest(new { message = "UserId and RoomId are required" });

            int reservationId;
            try
            {
                reservationId = await _paymentRepository.CreateCashReservationAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create cash reservation: {Error}", ex.Message);
                return StatusCode(500, new { message = "Failed to save reservation" });
            }

            try
            {
                var reservation = await _paymentRepository.GetReservationByIdAsync(reservationId);
                if (reservation != null)
                    await _notificationService.NotifyNewBooking(reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to send booking notification: {Error}", ex.Message);
            }

            var isEmailSent = false;
            try
            {
                await _emailService.SendCashBookingConfirmationAsync(request.Email, request.Name, request);
                isEmailSent = true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to send confirmation email: {Error}", ex.Message);
            }

            return Ok(new
            {
                success = true,
                reservationId,
                emailSent = isEmailSent,
                message = isEmailSent
                    ? "Đặt phòng thành công, vui lòng thanh toán tại quầy khi nhận phòng"
                    : "Đặt phòng thành công nhưng không thể gửi email xác nhận"
            });
        }
    }
}
