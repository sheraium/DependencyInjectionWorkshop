using DependencyInjectionWorkshop.Adapter;

namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : IAuthentication
    {
        private IAuthentication _authentication;
        private INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification)
        {
            _authentication = authentication;
            _notification = notification;
        }

        private void Send(string accountId)
        {
            _notification.Send(accountId);
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isVerify = _authentication.Verify(accountId, password, otp);
            if (!isVerify)
            {
                Send(accountId);
            }
            return isVerify;
        }
    }
}