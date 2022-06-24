using System;
using System.Runtime.Serialization;

namespace SonosUPnP
{
    /// <summary>
    /// Dient für die Liste der Audio Eingänge
    /// </summary>
    [Serializable]
    [DataContract]
    public class AudioComponentMemer
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string UUID { get; set; }

    }
}
