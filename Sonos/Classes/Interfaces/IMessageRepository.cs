using Sonos.Classes.Events;
using System;

namespace Sonos.Classes.Interfaces
{
    public interface IMessageRepository
    {
        event EventHandler<NotificationArgs> NotificationEvent;
        void Broadcast(Notification notification);
    }
}
