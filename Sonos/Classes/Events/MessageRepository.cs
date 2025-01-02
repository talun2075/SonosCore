using Sonos.Classes.Interfaces;
using System;
namespace Sonos.Classes.Events
{
    public class MessageRepository : IMessageRepository
    {
        public MessageRepository()
        {
        }

        public event EventHandler<Notification> NotificationEvent;

        public void Broadcast(Notification notification)
        {
            NotificationEvent?.Invoke(this, notification);
        }
    }
}
