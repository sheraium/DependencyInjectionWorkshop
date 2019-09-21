using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Service;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly FailedCounter _failedCounter;
        private readonly OTPService _otpService;
        private readonly IProfile _profile;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly SlackAdapter _slackAdapter;
        private readonly NLogAdapter _nLogAdapter;

        public AuthenticationService()
        {
            _profile = new ProfileDAO();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OTPService();
            _slackAdapter = new SlackAdapter();
            _failedCounter = new FailedCounter();
            _nLogAdapter = new NLogAdapter();
        }

        public AuthenticationService(FailedCounter failedCounter, OTPService otpService, IProfile profile, Sha256Adapter sha256Adapter, SlackAdapter slackAdapter, NLogAdapter nLogAdapter)
        {
            _failedCounter = failedCounter;
            _otpService = otpService;
            _profile = profile;
            _sha256Adapter = sha256Adapter;
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

                var failedCount = _failedCounter.GetFailedCount(accountId);
                _nLogAdapter.LogMessage($"accountId:{accountId} failed times:{failedCount}");

                _slackAdapter.Notify(accountId);

                return false;
            }
        }
    }
}