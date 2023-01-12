using System;
using System.Collections.Generic;
using Sonos.Controllers;
using SonosUPnP;
using HomeLogging;
using System.Threading.Tasks;
using Sonos.Classes.Events;
using System.Threading;
using System.Net;
using SonosConst;
using Sonos.Classes.Interfaces;
using SonosData.DataClasses;
using SonosData;
using SonosSQLiteWrapper.Interfaces;
using SonosUPNPCore.Interfaces;

namespace Sonos.Classes
{
    /// <summary>
    /// Zwischenschicht zu Sonos Player und Controller
    /// </summary>
    public class SonosHelper : ISonosHelper
    {
        #region Klassenvariablen
        private readonly IMusicPictures _musicpictures;
        #endregion Klassenvariablen
        #region public Methoden
        public SonosHelper(ILogging logger, ISonosDiscovery sonosDiscovery, IMusicPictures imu)
        {
            Logger = logger;
            Sonos = sonosDiscovery;
            Sonos.PlayerChange += Sonos_Player_Changed;
            Sonos.GlobalSonosChange += Sonos_TopologyChanged;
            _musicpictures = imu;
        }

        public Boolean CheckPlayerForHashImages(IList<SonosPlayer> sp)
        {
            foreach (var item in sp)
            {
                CheckPlayerForHashImages(item);
            }
            return true;
        }

        public bool CheckAllPlayerReachable(Boolean usetimer = false)
        {
            Boolean retval = false;
            foreach (SonosPlayer sp in Sonos.Players)
            {
                var ip = sp.PlayerProperties.BaseUrl;
                retval = ScanPort(ip);

                if (!retval)
                {
                    RemoveDevice(sp);
                    break;
                }
            }
            if (usetimer) //Timer um sich selber aufzurfuen alle 30 Minuten, wenn das einmal passiert ist.
                _ = new Timer(state => CheckAllPlayerReachable(true), null, TimeSpan.FromMinutes(30), TimeSpan.FromMilliseconds(-1));
            return retval;
        }

        public void RemoveDevice(SonosPlayer playerToRemove)
        {
            Sonos.RemoveDevice(playerToRemove);
        }

        /// <summary>
        /// Wait until Player is not more in Transition Playstate
        /// </summary>
        /// <param name="sp"></param>
        public async Task<Boolean> WaitForTransitioning(SonosPlayer sp)
        {
            Boolean trans = false;
            try
            {
                if (sp.PlayerProperties.TransportState == SonosEnums.TransportState.TRANSITIONING)
                {

                    int counter = 0;
                    while (!trans)
                    {
                        if (sp.PlayerProperties.TransportState != SonosEnums.TransportState.TRANSITIONING || counter > 5)
                        {
                            trans = true;
                        }
                        await Task.Delay(200);
                        counter++;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ServerErrorsAdd("WaitForTransitioning bei Player: " + sp.Name, ex);
            }
            return trans;
        }
        /// <summary>
        /// Generiert die Übergebene Zone
        /// </summary>
        /// <param name="masterUUID">UUID des ZoneMasters</param>
        /// <param name="toCoordinatedPlayer">Liste der zu coordinierenden Player</param>
        /// <returns></returns>
        public async Task<Boolean> GenerateZoneConstruct(string masterUUID, List<String> toCoordinatedPlayer)
        {
            return await GenerateZoneConstruct(Sonos.GetPlayerbyUuid(masterUUID), toCoordinatedPlayer);
        }
        /// <summary>
        /// Generiert die Übergebene Zone
        /// </summary>
        /// <param name="master">Player der ZoneMasters werden soll</param>
        /// <param name="toCoordinatedPlayer">Liste der zu coordinierenden Player</param>
        /// <returns></returns>
        public async Task<Boolean> GenerateZoneConstruct(SonosPlayer master, List<String> toCoordinatedPlayer)
        {
            Boolean retval = false;
            try
            {
                if (toCoordinatedPlayer.Count == 1 && toCoordinatedPlayer[0] == master.UUID)
                {
                    //Player nun als alleingien machen und alle anderen rauswerfen. 
                    foreach (var item in master.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup)
                    {
                        if (item == master.UUID) continue;
                        var gpluu = Sonos.GetPlayerbyUuid(item);
                        await gpluu.AVTransport.BecomeCoordinatorOfStandaloneGroup();
                    }
                    master.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup.Clear();
                    master.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup.Add(master.UUID);
                    master.LastChange = DateTime.Now;
                    Sonos.Player_ManuallChanged(SonosEnums.EventingEnums.ZonePlayerUUIDsInGroup, master);
                    return true;
                }
                if (master.PlayerProperties.GroupCoordinatorIsLocal == false)
                {
                    await master.AVTransport.BecomeCoordinatorOfStandaloneGroup();
                }
                //Player löschen
                foreach (var item in master.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup)
                {
                    if (toCoordinatedPlayer.Contains(item)) continue;
                    var gpluu = Sonos.GetPlayerbyUuid(item);
                    await gpluu.AVTransport.BecomeCoordinatorOfStandaloneGroup();
                }
                //Player zufügen 
                foreach (var item in toCoordinatedPlayer)
                {
                    if (master.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup.Contains(item)) continue;
                    var newplayer = Sonos.GetPlayerbyUuid(item);
                    if (newplayer.PlayerProperties.GroupCoordinatorIsLocal && newplayer.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup.Count > 1)
                    {
                        //war coordinator und soll nun alleine sein. 
                        await newplayer.AVTransport.BecomeCoordinatorOfStandaloneGroup();
                    }
                    await newplayer.AVTransport.SetAVTransportURI(SonosConstants.xrincon + master.UUID);
                    await Task.Delay(200);
                }

                return retval;
            }
            catch
            {
                return retval;
            }
        }

        /// <summary>
        /// Prüft ob das Ziel Konstrukt schon vorhanden ist.
        /// </summary>
        /// <param name="primaray"></param>
        /// <param name="listOfPlayers"></param>
        /// <returns></returns>
        public Boolean IsSonosTargetGroupExist(SonosPlayer primaray, List<string> listOfPlayers)
        {
            if (primaray == null) return false;
            if (!primaray.PlayerProperties.GroupCoordinatorIsLocal) return false;
            //Liste der vorhandenen Player durchlaufen
            if ((primaray.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup.Count - 1) != listOfPlayers.Count) return false;

            foreach (var item in listOfPlayers)
            {
                if (!primaray.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup.Contains(item)) return false;
            }
            foreach (var item in primaray.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup)
            {
                if (!listOfPlayers.Contains(item) && item != primaray.UUID) return false;
            }
            return true;
        }
        /// <summary>
        /// Prüft die übergebene Playlist mit dem Übergeben Player ob neu geladen werden muss.
        /// </summary>
        /// <param name="pl">Playliste, die geladen werden soll.</param>
        /// <param name="sp">Coordinator aus der Führenden Zone</param>
        /// <returns>True muss neu geladen werden</returns>
        public Boolean CheckPlaylist(SonosItem playlist, SonosPlayer sp, Boolean trace = false)
        {
            try
            {
                if (trace)
                    Logger.TraceLog("CheckPlaylist", "Start");
                if (sp.AVTransport == null) return false;
                return sp.PlayerProperties.EnqueuedTransportURI != playlist.Uri;
            }
            catch (Exception ex)
            {
                Logger.ServerErrorsAdd("CheckPlaylist", ex);
                return true;
            }
        }

        /// <summary>
        /// Läd die übergebene Playlist
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="sp"></param>
        /// <returns></returns>
        public async Task<Boolean> LoadPlaylist(SonosItem pl, SonosPlayer sp)
        {
            //laden der übergebenen Playlist
            try
            {
                if (sp.AVTransport == null) return false;
                await sp.AVTransport.RemoveAllTracksFromQueue();
                await Task.Delay(300);
                var qd = sp.AVTransport.AddURIToQueue(pl, true);
                if (sp.PlayerProperties.AVTransportURI != SonosConstants.xrinconqueue + sp.UUID + "#0")
                {
                    await sp.AVTransport.SetAVTransportURI(SonosConstants.xrinconqueue + sp.UUID + "#0");
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.ServerErrorsAdd("Loadplaylist.log", ex, "SonosHelper");
                return false;
            }
        }
        /// <summary>
        /// Liefert aus der ZoneProperties die Wiedergabelisten bzw. füllt diese auch bei Bedarf
        /// </summary>
        /// <returns></returns>
        public async Task<List<SonosItem>> GetAllPlaylist()
        {
            if (Sonos != null && Sonos.Players != null && Sonos.Players.Count != 0)
            {
                if (Sonos.ZoneProperties.ListOfAllPlaylist == null || Sonos.ZoneProperties.ListOfAllPlaylist.Count == 0)
                {
                    await Sonos.SetPlaylists();
                }

            }
            return Sonos.ZoneProperties.ListOfAllPlaylist;
        }
        /// <summary>
        /// Füllt alle Player einmal mit aktuellen Daten inkl. Playlist
        /// </summary>
        /// <returns></returns>
        public async Task<Boolean> FillAllPlayerProperties()
        {
            try
            {
                foreach (SonosPlayer player in Sonos.Players)
                {
                    if (await player.FillPlayerPropertiesDefaultsAsync(false, true))
                    {
                        CheckPlayerForHashImages(player);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.ServerErrorsAdd("FillAllPlayerProperties", ex);
                return false;
            }
        }
        /// <summary>
        /// Füllt alle Zeit/Settings Werte aus dem SonosSystem. Für den Timer
        /// </summary>
        public async Task<Boolean> FillSonosTimeSettingStuff()
        {
            return await Sonos.GetSonosTimeStuff() && await Sonos.SetSettings();
        }
        #endregion public Methoden
        #region Props
        /// <summary>
        /// Das primäre Sonos Objekt
        /// </summary>
        public ISonosDiscovery Sonos { get; private set; }

        internal Boolean WasInitialed { get; private set; }

        /// <summary>
        /// Erlaubte Services aus der Webconfig. 
        /// </summary>
        public List<SonosEnums.Services> ServiceEnums { get; set; }
        public List<ISonosBrowseList> ChildGenrelist { get; set; } = new();
        public ILogging Logger { get; set; }

        #endregion Props
        #region private Methoden
        private Boolean ScanPort(string ipwithport)
        {
            if (string.IsNullOrEmpty(ipwithport) || !ipwithport.Contains(':')) return true;
            var splitedip = ipwithport.Split(':');
            Boolean retval = false;
            string ip = splitedip[0];
            int portno = int.Parse(splitedip[1]);
            IPAddress ipa = IPAddress.Parse(ip);
            try
            {
                System.Net.Sockets.Socket sock = new(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                sock.SendTimeout = 6000;
                sock.Connect(ipa, portno);

                if (sock.Connected == true) // Port is in use and connection is successful
                    retval = true;
                sock.Close();
                return retval;
            }
            catch (Exception ex)
            {
                Logger.ServerErrorsAdd("ScanPort", ex, "SonosHelper");
                return false;
            }
        }

        public Boolean CheckPlayerForHashImages(SonosPlayer sp)
        {
            try
            {
                var props = sp.PlayerProperties;
                try
                {
                    if (!string.IsNullOrEmpty(props.NextTrack.AlbumArtURI))
                        props.NextTrack = _musicpictures.UpdateItemToHashPath(props.NextTrack);
                    if (!string.IsNullOrEmpty(props.CurrentTrack.AlbumArtURI))
                        props.CurrentTrack = _musicpictures.UpdateItemToHashPath(props.CurrentTrack);
                }
                catch (Exception ex)
                {
                    Logger.ServerErrorsAdd("CheckPlayerForHashImages:NC", ex, "SonosHelper");
                    return false;
                }
                if (!props.Playlist.IsEmpty && !props.Playlist.PlayListItemsHashChecked)
                {
                    lock (props.Playlist.PlayListItems)
                    {
                        foreach (SonosItem item in props.Playlist.PlayListItems)
                        {
                            try
                            {
                                if (item != null)
                                    _musicpictures.UpdateItemToHashPath(item);
                                else
                                    break;
                            }
                            catch (Exception ex)
                            {
                                Logger.ServerErrorsAdd("CheckPlayerForHashImages item:" + item.Uri, ex, "SonosHelper");
                                continue;
                            }
                        }
                        props.Playlist.PlayListItemsHashChecked = true;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.ServerErrorsAdd("CheckPlayerForHashImages", ex, "SonosHelper");
                return false;
            }
        }
        #region Eventing
        /// <summary>
        /// Wenn sich an den Zonen/Anzahl Playern etwas ändert, wird dies entsprechend mit Datetime.Now befüllt und kann abgefragt werden. 
        /// Wird einmalig bei der Initialisierung aufgerufen.
        /// </summary>
        private void Sonos_TopologyChanged(object sender, SonosDiscovery sd)
        {
            EventController.EventBroadCast(new Notification((SonosEnums.EventingEnums)sender, sd));
        }
        /// <summary>
        /// Wird aufgerufen, wenn sich was beim einem Player geändert hat.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sonos_Player_Changed(object sender, SonosPlayer e)
        {
            EventController.EventBroadCast(new Notification((SonosEnums.EventingEnums)sender, e));
        }
        #endregion Eventing
        #endregion private Methoden
    }
}