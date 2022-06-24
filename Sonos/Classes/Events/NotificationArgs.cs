using System;

namespace Sonos.Classes.Events
{
    public class NotificationArgs : EventArgs
    {
        public Notification Notification { get; }

        public NotificationArgs(Notification notification)
        {
            Notification = notification;
        }
    }
}