using DependencyInjectionWorkshop.Adapter;

namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : AuthenticationDecoratorBase
    {
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification) : base(authentication)
        {
            _notification = notification;
        }

        private void Send(string accountId)
        {
            _notification.Send(accountId);
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isVerify = base.Verify(accountId, password, otp);
            if (!isVerify)
            {
                Send(accountId);
            }
            return isVerify;
        }
    }
}