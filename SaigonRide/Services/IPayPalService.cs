namespace SaigonRide.Services
{
    public interface IPayPalService
    {
        Task<string> CreatePaymentUrl(int rentalId, double usdAmount);
        Task<bool> ExecutePayment(string paymentId, string payerId);
    }
}
