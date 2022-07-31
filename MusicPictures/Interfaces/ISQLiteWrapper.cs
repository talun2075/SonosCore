
using System.Data;

namespace SonosSQLiteWrapper.Interfaces
{
    public interface ISQLiteWrapper
    {
        public DataTable MusicPictures { get; set; }
        void Update();
    }
}