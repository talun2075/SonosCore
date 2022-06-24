using SonosUPnP.DataClasses;
using System;
using System.Collections.Generic;

namespace Sonos.Classes.Events
{
    /// <summary>
    /// Element welches die letzten Änderungen bereit hält.
    /// </summary>
    public class RinconLastChangeItem
    {
        /// <summary>
        /// UUID des Players oder "Discovery" für globale Änderungen
        /// </summary>
        public String UUID { get; set; }
        /// <summary>
        /// Wann ist die Änderung passiert.
        /// </summary>
        public DateTime LastChange { get; set; }
        /// <summary>
        /// TypeEnum als String
        /// </summary>
        public String ChangeType => TypeEnum.ToString();
        /// <summary>
        /// Welches Event wurde ausgelöst
        /// </summary>
        internal SonosEnums.EventingEnums TypeEnum { get; set; }
        /// <summary>
        /// Welche Daten sollen mit gegeben werden. 
        /// Event ID und der geänderte Wert ist normal.
        /// </summary>
        public Dictionary<String, String> ChangedValues { get; set; } = new Dictionary<string, string>();
    }
}
