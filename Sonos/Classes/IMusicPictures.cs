using SonosUPnP;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sonos.Classes
{
    public interface IMusicPictures
    {
        Task<bool> GenerateDBContent();
        Task<List<SonosItem>> UpdateItemListToHashPath(List<SonosItem> items);
    }
}