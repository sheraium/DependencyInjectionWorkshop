using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Service;
using NSubstitute;
using NUnit.Framework;
using System;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string DefaultAccountId = "joey";
        private const int DefaultFailedCount = 88;
        private const string DefaultHashedPassword = "my hashed password";
        private const string DefaultInputPassword = "abc";
        private const string DefaultOtp = "123456";
        private IAuthentication _authentication;
        private IFailedCounter _failedCounter;
        private IHash _hash;
        private ILogger _logger;
        private INotification _notification;
        private IOTPService _otpService;
        private IProfile _profile;

        [Test]
        public void account_is_locked()
        {
            _failedCounter.GetAccountIsLocked(DefaultAccountId).Returns(true);
            ShouldThrow<FailedTooManyTimesException>();
        }

        [Test]
        public void add_failed_count_when_invalid()
        {
            WhenInvalid();
            ShouldAddFailedCount();
        }

        [Test]
        public void is_invalid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultInputPassword, "111111");
            ShouldBeInvalid(isValid);
        }

        [Test]
        public void is_valid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultInputPassword, DefaultOtp);
            ShouldBeValid(isValid);
        }

        [Test]
        public void log_failed_count_when_invalid()
        {
            GivenFailedCount(DefaultAccountId, DefaultFailedCount);
            WhenInvalid();
            LogShouldContains(DefaultAccountId, DefaultFailedCount.ToString());
        }

        [Test]
        public void notify_user_when_invalid()
        {
            WhenInvalid();
            ShouldNotify();
        }

        [Test]
        public void reset_failed_count_when_valid()
        {
            WhenValid();
            ShouldResetFailedCount(DefaultAccountId);
        }

        [SetUp]
        public void SetUp()
        {
            _failedCounter = Substitute.For<IFailedCounter>();
            _otpService = Substitute.For<IOTPService>();
            _profile = Substitute.For<IProfile>();
            _hash = Substitute.For<IHash>();
            _notification = Substitute.For<INotification>();
            _logger = Substitute.For<ILogger>();
            _authentication =
                new AuthenticationService(_otpService, _profile, _hash);
            _authentication = new FailedCounterDecorator(_authentication, _failedCounter);
            _authentication = new NotificationDecorator(_authentication, _notification);
            _authentication = new LogFailedCountDecorator(_authentication, _failedCounter, _logger);
        }

        private static void ShouldBeInvalid(bool isValid)
        {
            Assert.IsFalse(isValid);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private void GivenFailedCount(string defaultAccountId, int failedCount)
        {
            _failedCounter.GetFailedCount(defaultAccountId).Returns(failedCount);
        }

        private void GivenHash(string defaultInputPassword, string defaultHashedPassword)
        {
            _hash.Compute(defaultInputPassword).Returns(defaultHashedPassword);
        }

        private void GivenOtp(string defaultAccountId, string defaultOtp)
        {
            _otpService.GetCurrentOtp(defaultAccountId).Returns(defaultOtp);
        }

        private void GivenPassword(string defaultAccountId, string defaultHashedPassword)
        {
            _profile.GetPassword(defaultAccountId).Returns(defaultHashedPassword);
        }

        private void LogShouldContains(string accountId, string failedCount)
        {
            _logger.Received(1).Info(Arg.Is<string>(m => m.Contains(accountId) && m.Contains(failedCount)));
        }

        private void ShouldAddFailedCount()
        {
            _failedCounter.Received(1).AddFailedCount(DefaultAccountId);
        }

        private void ShouldNotify()
        {
            _notification.Received(1).Send(DefaultAccountId);
        }

        private void ShouldResetFailedCount(string defaultAccountId)
        {
            _failedCounter.Received(1).ResetFailedCount(defaultAccountId);
        }

        private void ShouldThrow<TException>() where TException : Exception
        {
            TestDelegate action = () => WhenVerify(DefaultAccountId, DefaultInputPassword, DefaultOtp);
            Assert.Throws<TException>(action);
        }

        private bool WhenInvalid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultInputPassword, "111111");
            return isValid;
        }

        private bool WhenValid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultInputPassword, DefaultOtp);
            return isValid;
        }

        private bool WhenVerify(string defaultAccountId, string defaultInputPassword, string defaultOtp)
        {
            return _authentication.Verify(defaultAccountId, defaultInputPassword, defaultOtp);
        }
    }
}