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
        private readonly IHash _hash;
        private readonly IOTPService _otpService;
        private readonly IProfile _profile;

        public AuthenticationService()
        {
            _profile = new ProfileDAO();
            _hash = new Sha256Adapter();
            _otpService = new OTPService();
        }

        public AuthenticationService(IOTPService otpService, IProfile profile, IHash hash)
        {
            _otpService = otpService;
            _profile = profile;
            _hash = hash;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var passwordFromDB = _profile.GetPassword(accountId);

            var hashedPassword = _hash.Compute(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            return hashedPassword == passwordFromDB && otp == currentOtp;
        }
    }
}