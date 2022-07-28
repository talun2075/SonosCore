using Sonos.Classes.Interfaces;
using System;
namespace Sonos.Classes.Events
{
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
