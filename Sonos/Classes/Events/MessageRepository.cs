using System;
namespace Sonos.Classes.Events
{
    public interface IMessageRepository
    {
        event EventHandler<NotificationArgs> NotificationEvent;
        void Broadcast(Notification notification);
    }

    public class MessageRepository : IMessageRepository
    {
        public MessageRepository()
        {
        }

        public event EventHandler<NotificationArgs> NotificationEvent;

        public void Broadcast(Notification notification)
        {
            NotificationEvent?.Invoke(this, new NotificationArgs(notification));
        }
    }
}
