namespace SaigonRide.Services
{
    public interface IEmailService
    {
        Task SendOtpAsync(string toEmail, string otpCode);
    }
}
