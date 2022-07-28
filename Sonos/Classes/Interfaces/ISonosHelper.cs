using HomeLogging;
using SonosUPnP;
using SonosUPnP.DataClasses;
using SonosUPNPCore.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sonos.Classes.Interfaces
{
    public interface ISonosHelper
    {
        public ILogging Logger { get; set; }
        public List<ISonosBrowseList> ChildGenrelist { get; set; }
        public List<SonosEnums.Services> ServiceEnums { get; set; }

        bool CheckAllPlayerReachable(bool usetimer = false);
        bool CheckPlayerForHashImages(IList<SonosPlayer> sp);
        bool CheckPlaylist(SonosItem playlist, SonosPlayer sp, bool trace = false);
        Task<bool> FillAllPlayerProperties();
        Task<bool> FillSonosTimeSettingStuff();
        Task<bool> GenerateZoneConstruct(SonosPlayer master, List<string> toCoordinatedPlayer);
        Task<bool> GenerateZoneConstruct(string masterUUID, List<string> toCoordinatedPlayer);
        Task<List<SonosItem>> GetAllPlaylist();
        bool IsSonosTargetGroupExist(SonosPlayer primaray, List<string> listOfPlayers);
        Task<bool> LoadPlaylist(SonosItem pl, SonosPlayer sp);
        void RemoveDevice(SonosPlayer playerToRemove);
        Task<bool> WaitForTransitioning(SonosPlayer sp);
    }
}