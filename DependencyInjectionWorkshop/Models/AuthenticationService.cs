using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Service;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IOTPService _otpService;
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly INotification _slackAdapter;
        private readonly ILogger _nLogAdapter;

        public AuthenticationService()
        {
            _profile = new ProfileDAO();
            _hash = new Sha256Adapter();
            _otpService = new OTPService();
            _slackAdapter = new SlackAdapter();
            _failedCounter = new FailedCounter();
            _nLogAdapter = new NLogAdapter();
        }

        public AuthenticationService(IFailedCounter failedCounter, IOTPService otpService, IProfile profile, IHash hash, INotification slackAdapter, ILogger nLogAdapter)
        {
            _failedCounter = failedCounter;
            _otpService = otpService;
            _profile = profile;
            _hash = hash;
            _slackAdapter = slackAdapter;
            _nLogAdapter = nLogAdapter;
        }

        public bool Verifty(string accountId, string password, string otp)
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
                _nLogAdapter.Info($"accountId:{accountId} failed times:{failedCount}");

                _slackAdapter.Send(accountId);

                return false;
            }
        }
    }
}