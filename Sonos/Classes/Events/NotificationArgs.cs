using Sonos.Classes.Interfaces;
using System;

namespace Sonos.Classes.Events
{
    public class NotificationArgs : EventArgs, INotificationArgs
    {
        public Notification Notification { get; }

        public NotificationArgs(Notification notification)
        {
            Notification = notification;
        }
    }
}