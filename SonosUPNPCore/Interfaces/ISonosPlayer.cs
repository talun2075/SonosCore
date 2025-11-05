using OSTL.UPnP;
using SonosData.DataClasses;
using SonosData.Enums;
using SonosUPnP;
using SonosUPnP.Services;
using SonosUPnP.Services.MediaRendererService;
using SonosUPnP.Services.MediaServerServices;
using SonosUPNPCore.Classes;
using SonosUPNPCore.Services;
using System;
using System.Threading.Tasks;

namespace SonosUPNPCore.Interfaces
{
    public interface ISonosPlayer
    {
        AlarmClock AlarmClock { get; }
        AudioIn AudioIn { get; }
        AVTransport AVTransport { get; }
        SonosUPnP.Services.MediaRendererService.ConnectionManager ConnectionManagerMR { get; }
        SonosUPnP.Services.MediaServerServices.ConnectionManager ConnectionManagerMS { get; }
        ContentDirectory ContentDirectory { get; }
        UPnPSmartControlPoint ControlPoint { get; set; }
        UPnPDevice Device { get; set; }
        Uri DeviceLocation { get; set; }
        DeviceProperties DeviceProperties { get; }
        GroupManagement GroupManagement { get; }
        GroupRenderingControl GroupRenderingControl { get; }
        DateTime LastChange { get; set; }
        MusicServices MusicServices { get; }
        string Name { get; set; }
        PlayerProperties PlayerProperties { get; set; }
        QPlay QPlay { get; }
        Queue Queue { get; }
        SonosRatingFilter RatingFilter { get; set; }
        RenderingControl RenderingControl { get; }
        SoftwareGeneration SoftwareGeneration { get; set; }
        SystemProperties SystemProperties { get; }
        string UUID { get; set; }
        ZoneGroupTopology ZoneGroupTopology { get; }

        event EventHandler<SonosPlayer> Player_Changed;
        public void ServiceCheck();
        void CheckPlayerPropertiesWithClient(PlayerProperties pp);
        Task<bool> FillPlayerPropertiesDefaultsAsync(bool overrule = false, bool CheckValues = false);
        Task<Playlist> GetPlayerPlaylist(bool fillnew = false, bool loadcurrent = false);
        void ServerErrorsAdd(string Method, string Source, Exception ex);
        void SetDevice(UPnPDevice playerDevice);
    }
}