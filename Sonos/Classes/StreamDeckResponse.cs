using Sonos.Classes.Interfaces;
using System;

namespace Sonos.Classes
{
    public class StreamDeckResponse : IStreamDeckResponse
    {
        public Boolean Playing { get; set; }
        public String CoverString { get; set; }
        public String RandomCover { get; set; }
    }
}