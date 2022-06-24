using SonosUPNPCore.Props;
using System;

namespace SonosUPnP.DataClasses
{
    /// <summary>
    /// Hält die Informationen eines Songs
    /// </summary>
    public class PlayerInfo
    {
        public string TrackURI { get; set; }
        /// <summary>
        /// Numme rin der aktuellen Playlist
        /// </summary>
        public int TrackIndex { get; set; } = 0;
        public string TrackMetaData { get; set; }
        /// <summary>
        /// Zeit Position beim Abspielen 
        /// </summary>
        public SonosTimeSpan RelTime { get; set; }
        /// <summary>
        /// Dauer des Songs
        /// </summary>
        public SonosTimeSpan TrackDuration { get; set; }
        /// <summary>
        /// Prüft ob die PlayerInfo verändert wurde
        /// oder noch eine leere Instanz
        /// </summary>
        /// <returns></returns>
	    public Boolean IsEmpty

        {
            get
            {
                return (string.IsNullOrEmpty(TrackURI) && TrackIndex == 0 && string.IsNullOrEmpty(TrackMetaData));
            }
        }
    }
}

    