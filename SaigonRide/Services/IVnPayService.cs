using SaigonRide.Models.Entities;

namespace SaigonRide.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, int rentalId, double amount);
        VnPayResponseModel PaymentExecute(IQueryCollection collections);
    }
}