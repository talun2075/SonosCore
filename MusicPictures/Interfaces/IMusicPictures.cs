using SonosData;
using System.Data;

namespace SonosSQLiteWrapper.Interfaces
{
    public interface IMusicPictures
    {
        DataTable CurrentMusicPictures { get; }
        bool GenerateDBContent(List<SonosItem> tracks);
        List<SonosItem> UpdateItemListToHashPath(List<SonosItem> items);
        SonosItem UpdateItemToHashPath(SonosItem item);
    }
}