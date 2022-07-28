﻿using SonosUPnP;
using System.Collections.Generic;

namespace Sonos.Classes.Interfaces
{
    public interface ISonosBrowseList
    {
        string Artist { get; set; }
        List<SonosItem> Childs { get; set; }
    }
}