using SonosData.Enums;
using SonosUPnP;
using SonosUPNPCore.Classes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SonosUPNPCore.Interfaces
{
    public interface ISonosDiscovery
    {
        IList<SonosPlayer> Players { get; set; }
        ZoneMethods ZoneMethods { get; set; }
        DiscoveryZoneProperties ZoneProperties { get; set; }

        event EventHandler<SonosDiscovery> GlobalSonosChange;
        event EventHandler<SonosPlayer> PlayerChange;

        SonosPlayer GetPlayerbyName(string playerName);
        SonosPlayer GetPlayerbyUuid(string uuid);
        SonosPlayer GetPlayerbySoftwareGenerationPlaylistentry(string playlist);
        SonosPlayer GetPlayerbySoftWareGeneration(SoftwareGeneration softgen);

        void CheckDevicesToPlayer();
        bool CheckPlaylists();
        Task<bool> GetSonosTimeStuff();
        void Player_ManuallChanged(object sender, SonosPlayer e);
        void RemoveDevice(SonosPlayer playerToRemove);
        Task<bool> SetPlaylists(bool makenew = false);
        Task<bool> SetSettings();
        Task<bool> UpdateMusicIndex();
    }
}