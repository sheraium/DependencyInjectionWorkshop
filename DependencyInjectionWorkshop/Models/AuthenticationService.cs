using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Service;

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool Verify(string accountId, string password, string otp);
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IOTPService _otpService;
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly ILogger _logger;

        public AuthenticationService()
        {
            _profile = new ProfileDAO();
            _hash = new Sha256Adapter();
            _otpService = new OTPService();
            _failedCounter = new FailedCounter();
            _logger = new NLogAdapter();
        }

        public AuthenticationService(IFailedCounter failedCounter, IOTPService otpService, IProfile profile, IHash hash, INotification notification, ILogger logger)
        {
            _failedCounter = failedCounter;
            _otpService = otpService;
            _profile = profile;
            _hash = hash;
            _logger = logger;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isLocked = _failedCounter.GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

            var passwordFromDB = _profile.GetPassword(accountId);

            var hashedPassword = _hash.Compute(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (hashedPassword == passwordFromDB && otp == currentOtp)
            {
                _failedCounter.ResetFailedCount(accountId);

                return true;
            }
            else
            {
                _failedCounter.AddFailedCount(accountId);

                var failedCount = _failedCounter.GetFailedCount(accountId);
                _logger.Info($"accountId:{accountId} failed times:{failedCount}");

                return false;
            }
        }
    }
}