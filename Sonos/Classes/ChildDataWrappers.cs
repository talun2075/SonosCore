using SonosUPnP;
using System;
using System.Collections.Generic;

namespace Sonos.Classes
{
    public class SonosBrowseList
    {
        public String Artist { get; set; }
        public List<SonosItem> Childs { get; set; }
    }
}
