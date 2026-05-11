using SaigonRide.Models.Entities;

namespace SaigonRide.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _config;

        public VnPayService(IConfiguration config)
        {
            _config = config;
        }

        public string CreatePaymentUrl(HttpContext context, int rentalId, double amount)
        {
            var vnpay = new VnPayLibrary();
            var vnp_TmnCode = _config["VnPay:TmnCode"];
            var vnp_HashSecret = _config["VnPay:HashSecret"];
            var vnp_Url = _config["VnPay:BaseUrl"];
            var vnp_ReturnUrl = _config["VnPay:ReturnUrl"];

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (amount * 100).ToString()); // VNPay nhân 100 số tiền
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang SaigonRide #" + rentalId);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", rentalId.ToString());

            return vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
        }

        public VnPayResponseModel PaymentExecute(IQueryCollection collections)
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value);
                }
            }

            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
            var vnp_HashSecret = _config["VnPay:HashSecret"];

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);

            return new VnPayResponseModel
            {
                Success = checkSignature && vnp_ResponseCode == "00",
                OrderId = vnpay.GetResponseData("vnp_TxnRef"),
                VnPayResponseCode = vnp_ResponseCode,
                TransactionId = vnpay.GetResponseData("vnp_TransactionNo")
            };
        }
    }
}