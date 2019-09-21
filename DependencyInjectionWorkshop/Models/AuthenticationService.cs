using System;
using System.Net.Http;
using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Service;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDAO _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OTPService _otpService;
        private readonly SlackAdapter _slackAdapter;

        public AuthenticationService()
        {
            _profileDao = new ProfileDAO();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OTPService();
            _slackAdapter = new SlackAdapter();
        }

        public bool Verifty(string accountId, string password, string otp)
        {
            var isLocked = GetAccountIsLocked(accountId, new HttpClient() { BaseAddress = new Uri("http://joey.com/") });
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

            var passwordFromDB = _profileDao.GetPasswordFromDb(accountId);

            var hashedPassword = _sha256Adapter.GetHashedPassword(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (hashedPassword == passwordFromDB && otp == currentOtp)
            {
                ResetFailedCount(accountId, new HttpClient() { BaseAddress = new Uri("http://joey.com/") });

                return true;
            }
            else
            {
                AddFailedCount(accountId, new HttpClient() { BaseAddress = new Uri("http://joey.com/") });

                LogFailedCount(accountId, new HttpClient() { BaseAddress = new Uri("http://joey.com/") });

                _slackAdapter.Notify(accountId);

                return false;
            }
        }

        private void LogFailedCount(string accountId, HttpClient httpClient)
        {
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            LogMessage($"accountId:{accountId} failed times:{failedCount}");
        }

        private static void LogMessage(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }

        private static void AddFailedCount(string accountId, HttpClient httpClient)
        {
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        private static void ResetFailedCount(string accountId, HttpClient httpClient)
        {
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        private static bool GetAccountIsLocked(string accountId, HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }
    }
}