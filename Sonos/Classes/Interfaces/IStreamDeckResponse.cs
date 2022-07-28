namespace Sonos.Classes.Interfaces
{
    public interface IStreamDeckResponse
    {
        string CoverString { get; set; }
        bool Playing { get; set; }
        string RandomCover { get; set; }
    }
}