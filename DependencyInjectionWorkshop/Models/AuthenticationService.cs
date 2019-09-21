using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Service;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly OTPService _otpService;
        private readonly ProfileDAO _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly SlackAdapter _slackAdapter;
        private readonly FailedCounter _failedCounter;

        public AuthenticationService()
        {
            _profileDao = new ProfileDAO();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OTPService();
            _slackAdapter = new SlackAdapter();
            _failedCounter = new FailedCounter();
        }

        public bool Verifty(string accountId, string password, string otp)
        {
            var isLocked = _failedCounter.GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

            var passwordFromDB = _profileDao.GetPasswordFromDb(accountId);

            var hashedPassword = _sha256Adapter.GetHashedPassword(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (hashedPassword == passwordFromDB && otp == currentOtp)
            {
                _failedCounter.ResetFailedCount(accountId);

                return true;
            }
            else
            {
                _failedCounter.AddFailedCount(accountId);

                LogFailedCount(accountId);

                _slackAdapter.Notify(accountId);

                return false;
            }
        }

        private static void LogMessage(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }

        private void LogFailedCount(string accountId)
        {
            var failedCount = _failedCounter.GetFailedCount(accountId);
            LogMessage($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}