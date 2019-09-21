using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Service
{
    public interface IOTPService
    {
        string GetCurrentOtp(string accountId);
    }

    public class OTPService : IOTPService
    {
        public OTPService()
        {
        }

        public string GetCurrentOtp(string accountId)
        {
            var currentOtp = string.Empty;
            var response = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/otps", accountId).Result;
            if (response.IsSuccessStatusCode)
            {
                currentOtp = response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            return currentOtp;
        }
    }
}