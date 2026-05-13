using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace SaigonRide.Services
{
    public class PayPalService : IPayPalService
    {
        private readonly IConfiguration _configuration;

        public PayPalService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private async Task<string> GetAccessToken()
        {
            var clientId = _configuration["PayPal:ClientId"];
            var secret = _configuration["PayPal:Secret"];
            var mode = _configuration["PayPal:Mode"]; 
            var baseUrl = mode == "sandbox" ? "https://api-m.sandbox.paypal.com" : "https://api-m.paypal.com";

            using var client = new HttpClient();
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{secret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

            var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") });
            var response = await client.PostAsync($"{baseUrl}/v1/oauth2/token", content);
            var json = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(json);
            return result.access_token;
        }

        public async Task<string> CreatePaymentUrl(int rentalId, double usdAmount)
        {
            var accessToken = await GetAccessToken();
            var mode = _configuration["PayPal:Mode"];
            var baseUrl = mode == "sandbox" ? "https://api-m.sandbox.paypal.com" : "https://api-m.paypal.com";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var payment = new
            {
                intent = "sale",
                payer = new { payment_method = "paypal" },
                transactions = new[]
                {
                    new {
                        amount = new { total = usdAmount.ToString("F2"), currency = "USD" },
                        description = $"Rental Payment for ID: {rentalId}"
                    }
                },
                redirect_urls = new
                {
                    return_url = _configuration["PayPal:ReturnUrl"],
                    cancel_url = _configuration["PayPal:CancelUrl"]
                }
            };

            var response = await client.PostAsync($"{baseUrl}/v1/payments/payment",
                new StringContent(JsonConvert.SerializeObject(payment), Encoding.UTF8, "application/json"));

            var json = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(json);

            foreach (var link in result.links)
            {
                if (link.rel == "approval_url") return link.href;
            }
            return null;
        }

        public async Task<bool> ExecutePayment(string paymentId, string payerId)
        {
            var accessToken = await GetAccessToken();
            var mode = _configuration["PayPal:Mode"];
            var baseUrl = mode == "sandbox" ? "https://api-m.sandbox.paypal.com" : "https://api-m.paypal.com";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var execution = new { payer_id = payerId };
            var response = await client.PostAsync($"{baseUrl}/v1/payments/payment/{paymentId}/execute",
                new StringContent(JsonConvert.SerializeObject(execution), Encoding.UTF8, "application/json"));

            return response.IsSuccessStatusCode;
        }
    }
}