namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authentication, IFailedCounter failedCounter) : base(authentication)
        {
            _failedCounter = failedCounter;
        }

        private void Reset(string accountId)
        {
            _failedCounter.ResetFailedCount(accountId);
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            CheckAccountIdIsLocked(accountId);
            var isVerify = base.Verify(accountId, password, otp);
            if (isVerify)
            {
                Reset(accountId);
            }
            else
            {
                AddFailedCount(accountId);
            }
            return isVerify;
        }

        private void AddFailedCount(string accountId)
        {
            _failedCounter.AddFailedCount(accountId);
        }

        private void CheckAccountIdIsLocked(string accountId)
        {
            var isLocked = _failedCounter.GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }
        }
    }
}