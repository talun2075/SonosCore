using SonosData;
using SonosUPnP;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Sonos.Classes.Interfaces
{
    public interface IMusicPictures
    {
        DataTable CurrentMusicPictures { get; }
        bool GenerateDBContent(List<SonosItem> tracks);
        List<SonosItem> UpdateItemListToHashPath(List<SonosItem> items);
        SonosItem UpdateItemToHashPath(SonosItem item);
    }
}