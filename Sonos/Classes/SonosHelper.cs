using System;
using System.Collections.Generic;
using Sonos.Controllers;
using SonosUPnP;
using SonosUPnP.DataClasses;
using HomeLogging;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Sonos.Classes.Events;
using System.Threading;
using System.Linq;
using SonosUPNPCore.Enums;
using System.Net;

namespace Sonos.Classes
{
    /// <summary>
    /// Zwischenschicht zu Sonos Player und Controller
    /// </summary>
    public static class SonosHelper
    {
        #region Klassenvariablen
        /// <summary>
        /// Das primäre Sonos Objekt
        /// </summary>
        public static SonosDiscovery Sonos;
        /// <summary>
        /// Liste mir allen Server Errors.
        /// </summary>
        internal static Dictionary<String, String> serverErrors = new ();
        /// <summary>
        /// Zeitpunkt, wann sich das letzt mal etwas an Zonen oder Anzahl Player geändert hat.
        /// </summary>
        internal static DateTime Topologiechanged { get; set; }
        internal static Boolean WasInitialed { get; private set; }

        internal static Dictionary<string, string> MusicPictureHashes = new();

        /// <summary>
        /// Erlaubte Services aus der Webconfig. 
        /// </summary>
        internal static List<SonosEnums.Services> ServiceEnums { get; private set; }

        internal static List<SonosCheckChangesObject> sccoList = new ();
        internal static readonly List<SonosBrowseList> ChildGenrelist = new();
        public static Logging Logger { get; set; } = new(new LoggerWrapperConfig() { ConfigName = "Sonos", TraceFileName ="trace.txt", ErrorFileName = "Error.txt" });
        public static IConfiguration Configuration { get; set; }
        #endregion Klassenvariablen
        #region private Methoden
        /// <summary>
        /// Sonos Suchen (Start Scan)
        /// </summary>
        private async static Task<Boolean> InitialSonos()
        {
            try
            {
                Logger.InfoLog("Sonos", "InitStart");
                Boolean.TryParse(Configuration["UseSubscription"], out Boolean usesubscriptions);
                ServiceEnums = new List<SonosEnums.Services>();
                var allowedservices = Configuration["UseOnlyThisSubscriptions"];
                if (allowedservices.Contains(","))
                {
                    var x = allowedservices.Split(',');
                    foreach (var item in x)
                    {
                        if (Enum.TryParse(item.Trim(), out SonosEnums.Services se))
                            ServiceEnums.Add(se);
                    }
                }
                else
                {
                    if (Enum.TryParse(allowedservices.Trim(), out SonosEnums.Services se))
                        ServiceEnums.Add(se);
                }
                //Logger = new Logging();
                Sonos = new SonosDiscovery(usesubscriptions, ServiceEnums,Logger);
                await Task.Run(() =>
                {
                    lock (Sonos)
                    {

                        Sonos.StartScan();
                        Boolean ok = false;
                        DateTime startnow = DateTime.Now;
                        int minFoundedPlayers = 3;
                        while (!ok)
                        {
                            //Timer, falls das suchen länger als 360 Sekunden dauert abbrechen
                            int tdelta = (DateTime.Now - startnow).Seconds;
                            if (Sonos.Players.Count > minFoundedPlayers || tdelta > 360)
                            {
                                ok = true;
                            }
                        }
                    }
                });
                Logger.InfoLog("Sonos", "InitEnd");
                return true;
            }
            catch (Exception x)
            {
                Logger.ServerErrorsAdd("SonosHelper:InitialSonos", x);
                return false;
            }
        }
        private static Boolean ScanPort(string ipwithport)
        {
            if (string.IsNullOrEmpty(ipwithport) || !ipwithport.Contains(":")) return true;
            var splitedip = ipwithport.Split(':');
            Boolean retval = false;
            string ip = splitedip[0];
            int portno = int.Parse(splitedip[1]);
            IPAddress ipa = IPAddress.Parse(ip);
            try
            {
                System.Net.Sockets.Socket sock = new (System.Net.Sockets.AddressFamily.InterNetwork,System.Net.Sockets.SocketType.Stream,System.Net.Sockets.ProtocolType.Tcp);
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
        private static async Task<Boolean> CheckPlayerForHashImages(IList<SonosPlayer> sp)
        {
            foreach (var item in sp)
            {
                 await CheckPlayerForHashImages(item);
            }
            return true;
        }
        private static async Task<Boolean> CheckPlayerForHashImages(SonosPlayer sp)
        {
            try
            {
                var props = sp.PlayerProperties;
                if (!string.IsNullOrEmpty(props.NextTrack.AlbumArtURI))
                    props.NextTrack = await MusicPictures.UpdateItemToHashPath(props.NextTrack);
                if (!string.IsNullOrEmpty(props.CurrentTrack.AlbumArtURI))
                    props.CurrentTrack = await MusicPictures.UpdateItemToHashPath(props.CurrentTrack);
                if (props.Playlist.PlayListItems.Any())
                {
                    foreach (SonosItem item in props.Playlist.PlayListItems)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(item.AlbumArtURI))
                            {
                                var titem = await MusicPictures.UpdateItemToHashPath(item);
                                item.AlbumArtURI = titem.AlbumArtURI;
                            }
                        }
                        catch (Exception ex)
                        {
                            SonosHelper.Logger.ServerErrorsAdd("CheckPlayerForHashImages item:" + item.Uri, ex, "DeviceController");
                            continue;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                SonosHelper.Logger.ServerErrorsAdd("CheckPlayerForHashImages", ex, "DeviceController");
                return false;
            }
        }
        #endregion private Methoden
        #region Eventing
        /// <summary>
        /// Wenn sich an den Zonen/Anzahl Playern etwas ändert, wird dies entsprechend mit Datetime.Now befüllt und kann abgefragt werden. 
        /// Wird einmalig bei der Initialisierung aufgerufen.
        /// </summary>
        private static void Sonos_TopologyChanged(object sender, SonosDiscovery sd)
        {
            EventController.EventBroadCast(new Notification((SonosEnums.EventingEnums)sender, sd));
        }
        /// <summary>
        /// Wird aufgerufen, wenn sich was beim einem Player geändert hat.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Sonos_Player_Changed(object sender, SonosPlayer e)
        {
            EventController.EventBroadCast(new Notification((SonosEnums.EventingEnums)sender, e));
        }
        #endregion Eventing
        public static bool CheckAllPlayerReachable(Boolean usetimer = false)
        {
            Boolean retval = false;
            foreach (SonosPlayer sp in Sonos.Players)
            {
                var ip = sp.PlayerProperties.BaseUrl;
                retval = ScanPort(ip);

                if (!retval)
                {
                    SonosHelper.RemoveDevice(sp);
                    break;
                }
            }
            if(usetimer) //Timer um sich selber aufzurfuen alle 30 Minuten, wenn das einmal passiert ist.
#pragma warning disable CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
                _ = new Timer(state => CheckAllPlayerReachable(true), null, TimeSpan.FromMinutes(30), TimeSpan.FromMilliseconds(-1));
#pragma warning restore CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
            return retval;
        }

        public static void RemoveDevice(SonosPlayer playerToRemove)
        {
            Sonos.RemoveDevice(playerToRemove);
        }

        #region public Methoden
        /// <summary>
        /// Initialisierung des Sonos Systems
        /// </summary>
        /// <returns></returns>
        public async static Task<Boolean> Initialisierung()
        {
            try
            {
                Boolean retval = false;
                if (Sonos == null || !WasInitialed)
                {
                    serverErrors.Clear();
                    sccoList.Clear();
                    retval = await InitialSonos();
                    WasInitialed = retval;
                    Sonos.PlayerChange += Sonos_Player_Changed;
                    Sonos.GlobalSonosChange += Sonos_TopologyChanged;
                    _ = new Timer(state => Sonos.CheckDevicesToPlayer(), null, TimeSpan.FromSeconds(15), TimeSpan.FromMilliseconds(-1));
#pragma warning disable CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
                    _ = new Timer(state => CheckPlayerForHashImages(Sonos.Players), null, TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(-1));
                    _ = new Timer(state => CheckAllPlayerReachable(true), null, TimeSpan.FromMinutes(15), TimeSpan.FromMilliseconds(-1));

                }
                Logger.InfoLog("Init", "Init Ohne Fehler");
                return retval;
            }
            catch (Exception ex)
            {
                Logger.ServerErrorsAdd("SonosHelper:Initialisierung", ex);
                return false;
            }
        }
        /// <summary>
        /// Gibt den SonosPlayer aufgrund des übergebenen Names zurück oder Null.
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public async static Task<SonosPlayer> GetPlayerbyName(string playerName)
        {
            if (String.IsNullOrEmpty(playerName)) return null;
            if (Sonos == null || Sonos.Players.Count == 0)
            {
                if(Logger != null)
                Logger.ServerErrorsAdd("GetPlayer Sonos ist Null Player:" + playerName, new Exception("Sonos ist null und wird initialisiert"));
                await InitialSonos();
                return null;
            }
            lock (Sonos.Players)
            {
                foreach (SonosPlayer sonosPlayer in Sonos.Players)
                {
                    if (sonosPlayer.Name.ToLower() == playerName.ToLower())
                        return sonosPlayer;
                }
            }
            return null;
        }
        /// <summary>
        /// Ermittelt einen Player aufgrund der UUID.
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public async static Task<SonosPlayer> GetPlayerbyUuid(string uuid)
        {
            if (String.IsNullOrEmpty(uuid)) return null;
            if (Sonos == null || Sonos.Players.Count == 0)
            {
                if(Logger != null)
                Logger.ServerErrorsAdd("GetPlayer Sonos ist Null Player:" + uuid, new Exception("Sonos ist null und wird initialisiert"));
                await InitialSonos();
                return null;
            }
            lock (Sonos.Players)
            {
                foreach (SonosPlayer sonosPlayer in Sonos.Players)
                {
                    if (sonosPlayer.UUID == uuid)
                        return sonosPlayer;
                }
            }
            return null;
        }

        public async static Task<SonosPlayer> GetPlayerbySoftwareGenerationPlaylistentry(string playlist)
        {
            if (String.IsNullOrEmpty(playlist)) return null;
            if (Sonos == null || Sonos.Players.Count == 0)
            {
                if (Logger != null)
                    Logger.ServerErrorsAdd("GetPlayerbySoftwareGenerationPlaylistentry Sonos ist Null Player", new Exception("Sonos ist null und wird initialisiert"));
                await InitialSonos();
                return null;
            }
            return Sonos.Players.FirstOrDefault(x=>x.SoftwareGeneration == Sonos.ZoneProperties.GetSoftwareVersionForPlaylistEntry(playlist));
        }
        /// <summary>
        /// Gibt den ersten Player einer bestimmten Softwaregen zurück
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public async static Task<SonosPlayer> GetPlayerbySoftWareGeneration(SoftwareGeneration softgen)
        {
            if (Sonos == null || Sonos.Players.Count == 0)
            {
                if (Logger != null)
                    Logger.ServerErrorsAdd("GetPlayer Sonos ist Null Player", new Exception("Sonos ist null und wird initialisiert"));
                await InitialSonos();
                return null;
            }
            lock (Sonos.Players)
            {
                return Sonos.Players.FirstOrDefault(x=>x.SoftwareGeneration == softgen);
            }
        }
        /// <summary>
        /// Wait until Player is not more in Transition Playstate
        /// </summary>
        /// <param name="sp"></param>
        public static async Task<Boolean> WaitForTransitioning(SonosPlayer sp)
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
        public static async Task<Boolean> GenerateZoneConstruct(string masterUUID, List<String> toCoordinatedPlayer)
        {
            return await GenerateZoneConstruct(await GetPlayerbyUuid(masterUUID), toCoordinatedPlayer);
        }
        /// <summary>
        /// Generiert die Übergebene Zone
        /// </summary>
        /// <param name="master">Player der ZoneMasters werden soll</param>
        /// <param name="toCoordinatedPlayer">Liste der zu coordinierenden Player</param>
        /// <returns></returns>
        public static async Task<Boolean> GenerateZoneConstruct(SonosPlayer master, List<String> toCoordinatedPlayer)
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
                        var gpluu = await GetPlayerbyUuid(item);
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
                    var gpluu = await GetPlayerbyUuid(item);
                        await gpluu.AVTransport.BecomeCoordinatorOfStandaloneGroup();
                }
                //Player zufügen 
                foreach (var item in toCoordinatedPlayer)
                {
                    if (master.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup.Contains(item)) continue;
                    var newplayer = await GetPlayerbyUuid(item);
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
        public static Boolean IsSonosTargetGroupExist(SonosPlayer primaray, List<string> listOfPlayers)
        {
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
        /// Innitialisiert das SonosSystem falls nicht vorhanden.
        /// </summary>
        /// <returns></returns>
        public async static Task<Boolean> CheckSonosLiving()
        {
            Boolean retval = false;
            if (Sonos == null)
            {
                retval = await Initialisierung();
            }
            if (Sonos == null)
            {
                if (retval)
                    Logger.ServerErrorsAdd("CheckSonosLiving", new Exception("Sonos Null obwohl initialisierung true zurück gegeben hat"));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Prüft die übergebene Playlist mit dem Übergeben Player ob neu geladen werden muss.
        /// </summary>
        /// <param name="pl">Playliste, die geladen werden soll.</param>
        /// <param name="sp">Coordinator aus der Führenden Zone</param>
        /// <returns>True muss neu geladen werden</returns>
        public static Boolean CheckPlaylist(SonosItem playlist, SonosPlayer sp, Boolean trace = false)
        {
            try
            {
                if(trace)
                SonosHelper.Logger.TraceLog("CheckPlaylist", "Start");
                if (sp.AVTransport == null) return false;
                return sp.PlayerProperties.EnqueuedTransportURI != playlist.Uri;


                //string pl = playlist.ContainerID;
                //Boolean retval = false;
                ////Wenn der Service AVTransport erlaubt ist, darf hier nicht EnqueuedTransportURI geprüft werden, da diese sich nicht ändert aber die Playliste evtl abends schon.
                //if (trace)
                //    SonosHelper.Logger.TraceLog("CheckPlaylist", "Prüfung AVTransport und gleiche URI");
                ////if (sp.PlayerProperties.EnqueuedTransportURI == playlist.Uri && !ServiceEnums.Contains(SonosEnums.Services.AVTransport)) return retval;
                //if (sp.PlayerProperties.EnqueuedTransportURI == playlist.Uri) return retval;

                //if (trace)
                //    SonosHelper.Logger.TraceLog("CheckPlaylist", "Check Stream");
                ////var evtlStream = await sp.AVTransport.GetPositionInfo();
                ////if (SonosItemHelper.CheckItemForStreamingUriCheck(evtlStream.TrackURI))
                ////    return true;
                //if (sp.PlayerProperties.CurrentTrack.Stream) return true;
                //if (trace)
                //    SonosHelper.Logger.TraceLog("CheckPlaylist", "Kein Stream");
                //BrowseResults currbr = new BrowseResults();
                //BrowseResults toLoadBr = new BrowseResults();
                ////Bei großen listen dauert das bis 4 Sekunden. 
                //if (trace)
                //    SonosHelper.Logger.TraceLog("CheckPlaylist", "Listen Laden");
                //currbr = await sp.ContentDirectory.Browse(BrowseObjects.CurrentPlaylist, 0, 10);
                //if (trace)
                //    SonosHelper.Logger.TraceLog("CheckPlaylist", "currentNumberREturned:"+currbr.NumberReturned+" Anzahl:"+currbr.Result.Count);
                //toLoadBr = await sp.ContentDirectory.Browse(pl, 0, 10);
                //SonosHelper.Logger.TraceLog("CheckPlaylist", "TargedNumberREturned:" + toLoadBr.NumberReturned + " Anzahl:" + toLoadBr.Result.Count);
                //if (currbr.NumberReturned == 0) return true;
                //if (toLoadBr.NumberReturned == 0) return true;//eigentlich ein Fehler
                //for (int i = 0; i < currbr.Result.Count; i++)
                //{
                //    if (currbr.Result[i].Title == toLoadBr.Result[i].Title) continue;
                //    if (trace)
                //        SonosHelper.Logger.TraceLog("CheckPlaylist", "Listen nicht gleich");
                //    retval = true;
                //    break;
                //}
                //if (!retval)
                //{
                //    //hier nun die letzten prüfen. 
                //    if (currbr.TotalMatches > 19 && toLoadBr.TotalMatches > 19)
                //    {
                //        int startindex = currbr.TotalMatches - 10;
                //        currbr = new BrowseResults();
                //        toLoadBr = new BrowseResults();
                //        currbr = await sp.ContentDirectory.Browse(BrowseObjects.CurrentPlaylist, startindex, 10);
                //        toLoadBr = await sp.ContentDirectory.Browse(pl, startindex, 10);
                //        if (currbr.NumberReturned == 0) return true;
                //        if (toLoadBr.NumberReturned == 0) return true;//eigentlich ein Fehler
                //        for (int i = 0; i < currbr.Result.Count; i++)
                //        {
                //            if (currbr.Result[i].Title == toLoadBr.Result[i].Title) continue;
                //            retval = true;
                //            break;
                //        }
                //    }
                //}
                //if (trace)
                //    SonosHelper.Logger.TraceLog("CheckPlaylist", "Return:"+retval);
                //return retval;
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
        public static async Task<Boolean> LoadPlaylist(SonosItem pl, SonosPlayer sp)
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
        public static async  Task<List<SonosItem>> GetAllPlaylist()
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
        public static async Task<Boolean> FillAllPlayerProperties()
        {
            try
            {
                foreach (SonosPlayer player in Sonos.Players)
                {
                    await player.FillPlayerPropertiesDefaultsAsync(false, true);
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
        public static async Task<Boolean> FillSonosTimeSettingStuff()
        {
            return await Sonos.GetSonosTimeStuff() && await Sonos.SetSettings();
        }
        /// <summary>
        /// Füllt alle Playlisten mit Inhalt. Für den Timer. 
        /// </summary>
        public async static void FillAllPlaylist()
        {
            await Sonos.FillAllFilledPlaylists();
        }
        #endregion public Methoden
    }
}