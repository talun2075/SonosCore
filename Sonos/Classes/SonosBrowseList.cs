using Sonos.Classes.Interfaces;
using SonosData;
using System;
using System.Collections.Generic;

namespace Sonos.Classes
{
    public class SonosBrowseList : ISonosBrowseList
    {
        public String Artist { get; set; }
        public String Source { get; set; }
        public List<SonosItem> Childs { get; set; }
    }
}
