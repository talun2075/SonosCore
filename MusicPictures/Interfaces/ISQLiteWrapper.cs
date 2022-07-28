using System.Data;

namespace SonosSQLiteWrapper.Interfaces
{
    public interface ISQLiteWrapper
    {
        DataTable GetMusicPictures();
        void Update();
    }
}