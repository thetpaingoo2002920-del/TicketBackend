using System.ComponentModel.DataAnnotations;

namespace TicketBackend.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "အမည်ထည့်ရန် လိုအပ်ပါသည်။")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email ထည့်ရန် လိုအပ်ပါသည်။")]
        [EmailAddress(ErrorMessage = "Email ပုံစံ မှန်ကန်မှု မရှိပါ။")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password ထည့်ရန် လိုအပ်ပါသည်။")]
        [MinLength(6, ErrorMessage = "Password သည် အနည်းဆုံး 6 လုံး ရှိရပါမည်။")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Email ထည့်ရန် လိုအပ်ပါသည်။")]
        [EmailAddress(ErrorMessage = "Email ပုံစံ မှန်ကန်မှု မရှိပါ။")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password ထည့်ရန် လိုအပ်ပါသည်။")]
        public string Password { get; set; } = string.Empty;
    }

    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email ထည့်ရန် လိုအပ်ပါသည်။")]
        [EmailAddress(ErrorMessage = "Email ပုံစံ မှန်ကန်မှု မရှိပါ။")]
        public string Email { get; set; } = string.Empty;
    }

    public class VerifyOtpDto
    {
        [Required(ErrorMessage = "Email ထည့်ရန် လိုအပ်ပါသည်။")]
        [EmailAddress(ErrorMessage = "Email ပုံစံ မှန်ကန်မှု မရှိပါ။")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP ကုဒ် ထည့်ရန် လိုအပ်ပါသည်။")]
        public string OtpCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password အသစ် ထည့်ရန် လိုအပ်ပါသည်။")]
        [MinLength(6, ErrorMessage = "Password သည် အနည်းဆုံး 6 လုံး ရှိရပါမည်။")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class AuthUserResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}