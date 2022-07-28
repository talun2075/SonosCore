using SonosUPnP;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sonos.Classes.Interfaces
{
    public interface IMusicPictures
    {
        Task<bool> GenerateDBContent();
        List<SonosItem> UpdateItemListToHashPath(List<SonosItem> items);
    }
}