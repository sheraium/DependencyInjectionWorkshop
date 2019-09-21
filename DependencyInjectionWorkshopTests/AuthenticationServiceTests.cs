using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Service;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_valid()
        {
            var failedCounter = Substitute.For<IFailedCounter>();
            var otpService = Substitute.For<IOTPService>();
            var profile = Substitute.For<IProfile>();
            var hash = Substitute.For<IHash>();
            var notification = Substitute.For<INotification>();
            var logger = Substitute.For<ILogger>();

            profile.GetPassword("joey").Returns("my hashed password");
            hash.Compute("abc").Returns("my hashed password");
            otpService.GetCurrentOtp("joey").Returns("123456");

            var authenticationService =
                new AuthenticationService(failedCounter, otpService, profile, hash, notification, logger);

            Assert.IsTrue(authenticationService.Verifty("joey", "abc", "123456"));
        }
    }
}