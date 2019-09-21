using SlackAPI;

namespace DependencyInjectionWorkshop.Adapter
{
    public interface INotification
    {
        void Send(string accountId);
    }

    public class SlackAdapter : INotification
    {
        public SlackAdapter()
        {
        }

        public void Send(string accountId)
        {
            string message = $"{accountId}Try to login failed";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
        }
    }
}