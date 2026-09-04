using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using TicketBackend.Data;
using TicketBackend.DTOs;
using TicketBackend.Models;

namespace TicketBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest(new { message = "Gmail ထည့်ပေးပါ။" });
            }

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Password ထည့်ပေးပါ။" });
            }

            var email = dto.Email.Trim().ToLower();

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (existingUser != null)
            {
                return BadRequest(new { message = "Account already exists!" });
            }

            var user = new User
            {
                FullName = dto.FullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "အကောင့်ဖွင့်ခြင်း အောင်မြင်ပါသည်။" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest(new { message = "Gmail ထည့်ပေးပါ။" });
            }

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Password ထည့်ပေးပါ။" });
            }

            var email = dto.Email.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return BadRequest(new { message = "Gmail သို့မဟုတ် စကားဝှက် မှားယွင်းနေပါသည်။" });
            }

            var response = new AuthResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };

            return Ok(new
            {
                message = "Login ဝင်ရောက်ခြင်း အောင်မြင်ပါသည်",
                user = response
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Email))
                {
                    return BadRequest(new { message = "Gmail ထည့်ပေးပါ။" });
                }

                var email = dto.Email.Trim().ToLower();

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

                if (user == null)
                {
                    return BadRequest(new { message = "ဒီ Gmail အကောင့် ရှာမတွေ့ပါ။" });
                }
                var otp = Random.Shared.Next(100000, 1000000).ToString();
                user.OtpCode = otp;
                user.OtpExpiry = DateTime.UtcNow.AddMinutes(5);

                await _context.SaveChangesAsync();
                await SendEmailOtpAsync(user.Email, otp);

                return Ok(new { message = "OTP ကုဒ်ကို သင့် Gmail ထဲသို့ ပို့ပေးလိုက်ပါပြီ!" });
            }
            catch (SmtpException ex)
            {
                Console.WriteLine("SMTP ERROR:");
                Console.WriteLine(ex.ToString());

                return StatusCode(500, new
                {
                    message = "Connection Lost!Check Your Internet Connection! ",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("FORGOT PASSWORD ERROR:");
                Console.WriteLine(ex.ToString());

                return StatusCode(500, new
                {
                    message = "Forgot Password လုပ်ရာတွင် Error ဖြစ်နေပါသည်။",
                    error = ex.Message
                });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(VerifyOtpDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.OtpCode) || string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest(new { message = "အချက်အလက်များ အားလုံး ဖြည့်သွင်းပေးပါ။" });
            }

            var email = dto.Email.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (user == null)
            {
                return BadRequest(new { message = "ဒီ Gmail အကောင့် ရှာမတွေ့ပါ။" });
            }

            if (user.OtpCode != dto.OtpCode)
            {
                return BadRequest(new { message = "OTP ကုဒ် မှားယွင်းနေပါသည်။" });
            }

            if (user.OtpExpiry == null || user.OtpExpiry < DateTime.UtcNow)
            {
                return BadRequest(new { message = "OTP ကုဒ် သက်တမ်းကုန်သွားပါပြီ။ OTP အသစ်တောင်းပါ။" });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.OtpCode = null;
            user.OtpExpiry = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "စကားဝှက်အသစ် အောင်မြင်စွာ ပြောင်းလဲပြီးပါပြီ!" });
        }

        private async Task SendEmailOtpAsync(string toEmail, string otpCode)
        {
            var senderEmail = "thetpaingoo2002920@gmail.com";
            var appPassword = "lyhfgnlhtjihwwzg";

            using var smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(senderEmail, appPassword),
                EnableSsl = true,
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, "Ticket System App"),
                Subject = "Ticket System - Password Reset OTP",
                Body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <div style='max-width: 600px; margin: auto; padding: 30px; background-color: #f4f4f5; border-radius: 12px;'>
        <h2 style='color: #2563eb;'>Ticket Booking System</h2>
        <p>သင့် Password ကို ပြန်လည်ပြောင်းလဲရန် OTP Code ဖြစ်ပါတယ်။</p>
        <div style='text-align: center; margin: 30px 0;'>
            <h1 style='font-size: 40px; letter-spacing: 8px; color: #16a34a;'>{otpCode}</h1>
        </div>
        <p>ဒီ OTP ကုဒ်သည် <strong>၅ မိနစ်</strong> အတွင်းသာ သက်တမ်းရှိပါတယ်။</p>
        <p style='color: #6b7280; font-size: 13px;'>မိမိမှ Password Reset တောင်းဆိုခြင်း မဟုတ်ပါက ဤ Email ကို လျစ်လျူရှုနိုင်ပါတယ်။</p>
    </div>
</body>
</html>
",
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}