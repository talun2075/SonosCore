using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using HomeLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OSTL.UPnP;
using SonosData.DataClasses;
using SonosData.Enums;
using SonosUPnP;
using SonosUPNPCore.Classes;
using SonosUPNPCore.Interfaces;

namespace SonosUPNPCore
{
    public class SonosDiscovery : ISonosDiscovery
    {
        #region Klassenvariablen
        private UPnPSmartControlPoint ControlPoint { get; set; }
        private readonly List<TopologyChange> topologyChanges = new() { new TopologyChange() { SoftwareVersion = 1, UseAlarmClock = true, UseMediaServer = true }, new TopologyChange() { SoftwareVersion = 2 } };
        public readonly IDictionary<string, UPnPDevice> playerDevices = new Dictionary<string, UPnPDevice>();
        private Timer stateChangedTimer;
        private readonly Dictionary<SonosEnums.EventingEnums, DateTime> LastChangeDates = new();
        private string OnZoneGroupStateChangedValue = string.Empty;
        public event EventHandler<SonosPlayer> PlayerChange = delegate { };
        public event EventHandler<SonosDiscovery> GlobalSonosChange = delegate { };

        private readonly IConfiguration _config;
        private readonly ILogging Logger;
        private readonly IServiceProvider _provider;
        #endregion Klassenvariablen
        #region Public Methoden
        public SonosDiscovery(IConfiguration config, ILogging log, IServiceProvider provider)
        {
            _config = config;
            Logger = log;
            _provider = provider;
            try
            {
                Zone = new Zone(Logger);
                Zone.GlobalSonosChange += ZoneSwGen_GlobalSonosChange;
                //ZoneProperties = new DiscoveryZoneProperties(ZoneSwGen1);
            }
            catch (Exception ex)
            {
                Logger.ServerErrorsAdd("ctor:ZonePerSoftwareGeneration", ex, "Discovery");
            }
            CtorInit();
        }


        /// <summary>
        /// Gibt den SonosPlayer aufgrund des übergebenen Names zurück oder Null.
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public SonosPlayer GetPlayerbyName(string playerName)
        {
            if (!Players.Any()) return null;

            lock (Players)
            {
                foreach (SonosPlayer sonosPlayer in Players)
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
        public SonosPlayer GetPlayerbyUuid(string uuid)
        {
            if (!Players.Any()) return null;

            lock (Players)
            {
                foreach (SonosPlayer sonosPlayer in Players)
                {
                    if (sonosPlayer.UUID == uuid)
                        return sonosPlayer;
                }
            }
            return null;
        }
        /// <summary>
        /// Gibt den ersten Player einer bestimmten Softwaregen zurück
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public SonosPlayer GetPlayerbySoftWareGeneration(SoftwareGeneration softgen)
        {
            if (!Players.Any()) return null;
            lock (Players)
            {
                return Players.FirstOrDefault(x => x.SoftwareGeneration == softgen);
            }
        }

        /// <summary>
        /// Alle Wiedergabelisten befüllen
        /// </summary>
        public async Task<bool> SetPlaylists(bool makenew = false)
        {
            bool retval = true;
            List<bool> retvallist = new();
            if (Players.Count > 0)
            {
                if (Zone.Properties.ListOfFavorites.Count == 0 || makenew)
                {
                    retvallist.Add(await SetFavoritesPlaylists());
                }
                if (Zone.Properties.ListOfImportedPlaylist.Count == 0 || makenew)
                {
                    retvallist.Add(await SetImportetPlaylist());
                }
                if (Zone.Properties.ListOfSonosPlaylist.Count == 0 || makenew)
                {
                    retvallist.Add(await SetSonosPlaylist());
                }
            }
            foreach (var item in retvallist)
            {
                if (!item)
                {
                    retval = false;
                    break;
                }
            }
            if (makenew)
                ManuellStateChange(SonosEnums.EventingEnums.SavedQueuesUpdateID, DateTime.Now);
            return retval;
        }
        /// <summary>
        /// Remove Device and make a Reset of SonosDiscovery
        /// </summary>
        /// <param name="playerToRemove"></param>
        public void RemoveDevice(SonosPlayer playerToRemove)
        {
            var dev = playerDevices[playerToRemove.UUID];
            ControlPoint.ForceDisposeDevice(dev);

        }
        /// <summary>
        /// Füllt die Alarmliste und die Uhrzeit;
        /// </summary>
        public async Task<bool> GetSonosTimeStuff()
        {
            bool retval = true;
            if (Players.Count > 0)
            {
                try
                {
                    try
                    {
                        //take only non Ikea Player. They have a Sync Bug and different ListAlarms
                        SonosPlayer pl1 = Players.FirstOrDefault(x => (x.Device.FriendlyName.Contains("Play") || x.Device.FriendlyName.Contains("Conn")));
                        if (pl1 == null)
                        {
                            //Fallback
                            pl1 = Players.FirstOrDefault();
                        }
                        if (pl1 != null)
                        {
                            try
                            {
                                Zone.Properties.CurrentSonosTime = await pl1.AlarmClock?.GetTimeNow();
                            }
                            catch (Exception ex)
                            {
                                Logger.ServerErrorsAdd("GetSonosTimeStuff:FillProps:CurrentSonosTime:Player:" + pl1.Name, ex, "Discovery");
                            }
                            try
                            {
                                Zone.Properties.ListOfAlarms = await pl1.AlarmClock?.ListAlarms();
                            }
                            catch (Exception ex)
                            {
                                Logger.ServerErrorsAdd("GetSonosTimeStuff:FillProps:ListOfAlarms:Player:" + pl1.Name, ex, "Discovery");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.ServerErrorsAdd("GetSonosTimeStuff:FillProps", ex, "Discovery");
                    }
                    try
                    {
                        if (Zone.Properties.AlarmListVersion > 0)
                        {
                            foreach (Alarm al in Zone.Properties.ListOfAlarms)
                            {
                                SonosPlayer plfe = Players.FirstOrDefault(y => y.UUID == al.RoomUUID);
                                if (plfe != null)
                                {
                                    al.RoomName = plfe.Name;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.ServerErrorsAdd("GetSonosTimeStuff:FillProps:ListOfAlarms:RefillRoomName", ex, "Discovery");
                    }
                }
                catch (Exception ex)
                {
                    Logger.ServerErrorsAdd("GetSonosTimeStuff", ex, "Discovery");
                    retval = false;
                }
            }
            return retval;
        }
        /// <summary>
        /// Löst das Update des Musikindexes aus.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UpdateMusicIndex()
        {
            if (Players.Any())
            {
                try
                {
                    SonosPlayer pl1 = Players.FirstOrDefault(x => x.SoftwareGeneration == SoftwareGeneration.ZG1);
                    SonosPlayer pl2 = Players.FirstOrDefault(x => x.SoftwareGeneration == SoftwareGeneration.ZG2);
                    if (pl1 != null)
                    {
                        var cd = pl1.ContentDirectory;
                        if (cd != null)
                        {
                            await cd.RefreshShareIndex();
                            while (await cd.GetShareIndexInProgress())
                            {
                                await Task.Delay(300);
                            }
                        }
                    }
                    if (pl2 != null)
                    {
                        var cd = pl2.ContentDirectory;
                        if (cd != null)
                        {
                            await cd.RefreshShareIndex();
                            while (await cd.GetShareIndexInProgress())
                            {
                                await Task.Delay(1000);
                            }
                        }
                    }
                    await SetPlaylists(true);
                    foreach (var playerdic in Zone.Properties.PlayerPlayedPlaylist)
                    {
                        foreach (var item in playerdic.Value)
                        {
                            Zone.Properties.PlayerPlayedPlaylist[playerdic.Key][item.Key] = 0;
                        }

                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
        /// <summary>
        /// Verarbeitet die default Settings (wird über Checktimer verarbeitet)
        /// </summary>
        public async Task<bool> SetSettings()
        {
            //Setting für Timeserver
            bool retval = false;
            if (!Players.Any())
                return retval;
            string timeServer = _config["TimeServer"];
            if (!string.IsNullOrEmpty(timeServer))
            {
                SonosPlayer sp1 = Players.FirstOrDefault(p => p.SoftwareGeneration == SoftwareGeneration.ZG1);
                if (Zone.Properties.TimeServer != timeServer)
                {
                    if (sp1 != null)
                        retval = await sp1.AlarmClock.SetTimeServer(timeServer);
                }
                else
                {
                    retval = true;
                }
            }
            else
            {
                retval = true;//Not set so no need to change;
            }
            var retvalalarm = true;
            //Nach Updates die Wecker wieder richten
            if (Zone.Properties.ListOfAlarms.Count == 0)
                await GetSonosTimeStuff();
            foreach (Alarm alarm in Zone.Properties.ListOfAlarms)
            {
                try
                {
                    if (alarm.PlayMode != "NORMAL")
                    {
                        alarm.PlayMode = "NORMAL";
                        SonosPlayer apl = Players.FirstOrDefault(x => x.UUID == alarm.RoomUUID);
                        if (apl != null)
                            await apl.AlarmClock?.UpdateAlarm(alarm);
                    }
                }
                catch (Exception ex)
                {
                    Logger.ServerErrorsAdd("SetSettings", ex, "Discovery");
                    retvalalarm = false;
                    continue;
                }
            }
            return retval == retvalalarm;
        }
        #endregion Public Methoden
        #region Propertys
        public ZoneMethods ZoneMethods { get; set; } = new ZoneMethods();
        public Zone Zone { get; set; }
        /// <summary>
        /// Alle Geräte als Liste
        /// </summary>
        public IList<SonosPlayer> Players { get; set; } = new List<SonosPlayer>();
        #endregion Propertys
        #region Devices
        /// <summary>
        /// Wenn ein Gerät gefunden wird, wird dieses den 
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="device"></param>
        private void OnDeviceAdded(UPnPSmartControlPoint cp, UPnPDevice device)
        {
            // we need to save these for future reference
            lock (playerDevices)
            {
                playerDevices[device.UniqueDeviceName] = device;
            }
            //define needed vars
            List<KeyValuePair<string, string>> customfields;
            string swgen = string.Empty;
            try
            {
                customfields = device.GetCustomFieldsFromDescription("urn:schemas-upnp-org:device-1-0");
                swgen = customfields.FirstOrDefault(x => x.Key == "swGen").Value;
                // okay, we will try and notify the players that they have been found now.
                var player = Players.FirstOrDefault(p => p.UUID == device.UniqueDeviceName);
                if (player != null)
                {
                    player.SetDevice(device);
                }
            }
            catch (Exception ex)
            {
                Logger.ServerErrorsAdd("OnDeviceAdded:Definevars:" + device.FriendlyName, ex, "Discovery");
            }
            if (string.IsNullOrEmpty(swgen))
            {
                Logger.ServerErrorsAdd("OnDeviceAdded", new Exception("swgen ist leer für device:" + device.FriendlyName), "Discovery");
                return;
            }
            var tp = topologyChanges.FirstOrDefault(x => x.SoftwareVersion.ToString() == swgen);
            if (tp == null)
            {
                Logger.ServerErrorsAdd("OnDeviceAdded", new Exception("tp ist leer für device:" + device.FriendlyName), "Discovery");
                return;
            }
            if (!tp.ActiveSubscription)
            {
                tp.ActiveSubscription = true;
                Debug.WriteLine(device.FriendlyName);
                Debug.WriteLine(tp.ActiveSubscription);
                // Subscribe to events
                var topologyService = device.GetService("urn:upnp-org:serviceId:ZoneGroupTopology");
                topologyService.Subscribe(600, (service, subscribeok) =>
                    {
                        if (!subscribeok) return;
                        var stateVariable = service.GetStateVariableObject("ZoneGroupState");
                        stateVariable.OnModified += OnZoneGroupStateChanged;
                    });
                Zone.StartSubscription(device);
            }
        }
        #endregion Devices       
            #region Eventing

            /// <summary>
            /// Eventing, wird benutzt um Änderungen an der Zone zu ermitteln
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="newvalue"></param>
        private void OnZoneGroupStateChanged(UPnPStateVariable sender, object newvalue)
        {
            // Avoid multiple state changes and consolidate them
            if (stateChangedTimer != null)
                stateChangedTimer.Dispose();
            if (OnZoneGroupStateChangedValue != sender.Value.ToString())
            {
                OnZoneGroupStateChangedValue = sender.Value.ToString();
                stateChangedTimer = new Timer(state => HandleZoneXML(sender.Value.ToString()), null, TimeSpan.FromMilliseconds(100),
                                          TimeSpan.FromMilliseconds(-1));
            }

        }
        /// <summary>
        /// Ermittelt die Zonen und die Player.
        /// </summary>
        /// <param name="xml"></param>
        private void HandleZoneXML(string xml)
        {
            try
            {
                Zone.Properties.ZoneGroupState.UpdateZoneGroups(xml);
            }
            catch (Exception ex)
            {
                Logger.ServerErrorsAdd("HandleZoneXML", ex, "Discovery");
            }
            var doc = XElement.Parse(xml);
            foreach (var zoneXML in doc.Descendants("ZoneGroup"))
            {
                CreateZone(zoneXML);
            }
        }
        /// <summary>
        /// Dient dazu manuelle Änderungen als Event zu feuern und den LastChange entsprechend zu setzen.
        /// </summary>
        /// <param name="_lastchange"></param>
        private void ManuellStateChange(SonosEnums.EventingEnums t, DateTime _lastchange)
        {
            try
            {
                if (GlobalSonosChange == null) return;
                LastChangeDates[t] = _lastchange;
                GlobalSonosChange(t, this);
            }
            catch (Exception ex)
            {
                Logger.ServerErrorsAdd("ManuellStateChange", ex, "Discovery");
            }
        }
        /// <summary>
        /// Wird bei jeder Änderung eines Players ausgelöst. 
        /// Die Änderungsart kann man im Sender sehen
        /// </summary>
        /// <param name="sender">EventingEnum</param>
        /// <param name="e">SonosPlayer</param>
        private async void Player_Changed(object sender, SonosPlayer e)
        {
            try
            {
                //hier nun bestimmte Dinge abgreifen um darauf reagieren zu können
                SonosEnums.EventingEnums t = (SonosEnums.EventingEnums)sender;
                if (t == SonosEnums.EventingEnums.QueueChangedSaved)
                {
                    Zone.Properties.ListOfSonosPlaylist.Clear();
                    await SetPlaylists();
                }
                //Hier auf Änderungen in der Playlist reagieren.
                if (t == SonosEnums.EventingEnums.EnqueuedTransportURI && !string.IsNullOrEmpty(e.PlayerProperties.EnqueuedTransportURI))
                {
                    //Logger.TraceLog("playlistToCurrentTrack", "EnqueuedTransportURI:" + e.PlayerProperties.EnqueuedTransportURI);
                    var playlistToCurrentTrack = Zone.Properties.PlayerPlayedPlaylist[e.UUID];
                    if (playlistToCurrentTrack.ContainsKey(e.PlayerProperties.EnqueuedTransportURI) && playlistToCurrentTrack[e.PlayerProperties.EnqueuedTransportURI] != e.PlayerProperties.CurrentTrackNumber && e.PlayerProperties.CurrentTrackNumber < 2 && playlistToCurrentTrack[e.PlayerProperties.EnqueuedTransportURI] > 1)
                    {
                        //Logger.TraceLog("playlistToCurrentTrack", "EnqueuedTransportURI Seek. Currentracknumber: " + e.PlayerProperties.CurrentTrackNumber + " Dictionary Number: " + playlistToCurrentTrack[e.PlayerProperties.EnqueuedTransportURI]);
                        await e.AVTransport.Seek(playlistToCurrentTrack[e.PlayerProperties.EnqueuedTransportURI].ToString(), SonosEnums.SeekUnit.TRACK_NR);
                        //PlayerChange(SonosEnums.EventingEnums.CurrentTrackNumber, e);
                    }
                    else
                    {
                        if (!playlistToCurrentTrack.ContainsKey(e.PlayerProperties.EnqueuedTransportURI) && (e.PlayerProperties.EnqueuedTransportURI.StartsWith("file") || e.PlayerProperties.EnqueuedTransportURI.EndsWith(".m3u") || e.PlayerProperties.EnqueuedTransportURI.StartsWith("x-rincon-playlist")))
                        {
                            var sonosplaylist = Zone.Properties.ListOfSonosPlaylist.FirstOrDefault(x => x.Uri == e.PlayerProperties.EnqueuedTransportURI);
                            if (sonosplaylist == null || !sonosplaylist.Title.Contains("zzz"))
                                playlistToCurrentTrack.Add(e.PlayerProperties.EnqueuedTransportURI, 1);
                            //Logger.TraceLog("playlistToCurrentTrack", "EnqueuedTransportURI zufügen: "+ e.PlayerProperties.EnqueuedTransportURI);
                        }
                    }
                }
                if (t == SonosEnums.EventingEnums.CurrentTrackNumber && !string.IsNullOrEmpty(e.PlayerProperties.EnqueuedTransportURI) && e.PlayerProperties.CurrentTrackNumber > 1)
                {
                    //Logger.TraceLog("playlistToCurrentTrack", "CurrentTrackNumber: " + e.PlayerProperties.CurrentTrackNumber);
                    //wenn wert vorhanden die Currentrack nummer rein setzen.
                    var ppp = Zone.Properties.PlayerPlayedPlaylist[e.UUID];
                    if (ppp.ContainsKey(e.PlayerProperties.EnqueuedTransportURI))
                    {
                        ppp[e.PlayerProperties.EnqueuedTransportURI] = e.PlayerProperties.CurrentTrackNumber;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ServerErrorsAdd("SonosDiscovery:Player_Changed", ex);
            }
            PlayerChange(sender, e);
        }
        public void Player_ManuallChanged(object sender, SonosPlayer e)
        {
            PlayerChange(sender, e);
        }
        /// <summary>
        /// Generiert alle benötigten Player falls nicht vorhanden
        /// </summary>
        /// <param name="zoneXml"></param>
        private void CreateZone(XElement zoneXml)
        {
            var list = zoneXml.Descendants("ZoneGroupMember").Where(x => x.Attribute("Invisible") == null).ToList();
            if (list.Count > 0)
            {
                foreach (var playerXml in list)
                {
                    SonosPlayer player = ActivatorUtilities.CreateInstance(_provider, typeof(SonosPlayer)) as SonosPlayer;
                    player.Name = (string)playerXml.Attribute("ZoneName");
                    player.UUID = (string)playerXml.Attribute("UUID");
                    player.DeviceLocation = new Uri((string)playerXml.Attribute("Location"));
                    player.ControlPoint = ControlPoint;

                    var swgentemp = (string)playerXml.Attribute("SWGen");
                    if (swgentemp == "2")
                    {
                        player.SoftwareGeneration = SoftwareGeneration.ZG2;
                    }

                    SonosPlayer sp = Players.FirstOrDefault(x => x.UUID == player.UUID);
                    if (sp == null)
                    {
                        Players.Add(player);
                        if (!Zone.Properties.PlayerPlayedPlaylist.ContainsKey(player.UUID))
                            Zone.Properties.PlayerPlayedPlaylist.Add(player.UUID, new Dictionary<string, int>());
                        player.Player_Changed += Player_Changed;
                    }
                    SonosPlayer pl = Players.First(x => x.UUID == player.UUID);
                    //ZonenInfos laden:
                    PrepareZonesPerPlayer(pl);
                    // This can happen before or after the topology event...
                    if (playerDevices.ContainsKey(player.UUID) && player.Device == null)
                    {
                        player.SetDevice(playerDevices[player.UUID]);
                    }
                }
            }
        }

        private void PrepareZonesPerPlayer(SonosPlayer pl)
        {
            var zone = Zone.Properties.ZoneGroupState.ZoneGroupStates.FirstOrDefault(x => x.CoordinatorUUID == pl.UUID);
            if (zone == null)
            {
                //Player ist nicht cooridnator
                if (pl.PlayerProperties.GroupCoordinatorIsLocal != false)
                {
                    pl.PlayerProperties.GroupCoordinatorIsLocal = false;
                    pl.LastChange = DateTime.Now;
                    PlayerChange(SonosEnums.EventingEnums.GroupCoordinatorIsLocal, pl);
                }
                foreach (var zoneGroup in Zone.Properties.ZoneGroupState.ZoneGroupStates)
                {
                    var found = zoneGroup.ZoneGroupMember.FirstOrDefault(x => x.UUID == pl.UUID);
                    if (found == null) continue;
                    if (pl.PlayerProperties.LocalGroupUUID != zoneGroup.CoordinatorUUID)
                    {
                        pl.PlayerProperties.LocalGroupUUID = zoneGroup.CoordinatorUUID;
                        pl.LastChange = DateTime.Now;
                        PlayerChange(SonosEnums.EventingEnums.LocalGroupUUID, pl);
                    }
                    //hier nun die zonenliste vergleichen
                    HandleZoneUUIDinGroup(pl, zoneGroup.ZoneGroupMember);
                    break;
                }
            }
            else
            {
                //player ist coordinator
                if (pl.PlayerProperties.GroupCoordinatorIsLocal != true)
                {
                    pl.PlayerProperties.GroupCoordinatorIsLocal = true;
                    pl.LastChange = DateTime.Now;
                    PlayerChange(SonosEnums.EventingEnums.GroupCoordinatorIsLocal, pl);
                    if (pl.PlayerProperties.TransportState == SonosEnums.TransportState.PLAYING)
                    {
                        pl.PlayerProperties.TransportState = SonosEnums.TransportState.PAUSED_PLAYBACK;
                        PlayerChange(SonosEnums.EventingEnums.TransportState, pl);
                    }
                }
                if (pl.PlayerProperties.LocalGroupUUID != zone.CoordinatorUUID)
                {
                    pl.PlayerProperties.LocalGroupUUID = zone.CoordinatorUUID;
                    pl.LastChange = DateTime.Now;
                    PlayerChange(SonosEnums.EventingEnums.LocalGroupUUID, pl);
                }
                HandleZoneUUIDinGroup(pl, zone.ZoneGroupMember);
            }
        }
        /// <summary>
        /// Prüft ob alle Playlisten geladen wurden. ACHTUNG: Software 1 wird bevorzugt.
        /// </summary>
        /// <returns>true, wenn neu geladen werden muss</returns>
        public bool CheckPlaylists()
        {
            if (Zone.Properties.ListOfSonosPlaylist.Count == 0 || Zone.Properties.ListOfImportedPlaylist.Count == 0 || Zone.Properties.ListOfFavorites.Count == 0)
                return true;

            return false;
        }
        public void CheckDevicesToPlayer()
        {
            Debug.WriteLine("CheckDevicesToPlayer wurde aufgerufen.");
            if (Players.Count != playerDevices.Count)
            {
                //Ein Player wurde nicht initialisiert. 
                foreach (UPnPDevice device in playerDevices.Values)
                {
                    SonosPlayer pl = Players.FirstOrDefault(x => x.UUID == device.UniqueDeviceName);
                    if (pl == null)
                    {
                        //define needed vars
                        List<KeyValuePair<string, string>> customfields;
                        string name;
                        string swgen = string.Empty;
                        string uuid;
                        string locat;
                        customfields = device.GetCustomFieldsFromDescription("urn:schemas-upnp-org:device-1-0");
                        name = customfields.FirstOrDefault(x => x.Key == "roomName").Value;
                        swgen = customfields.FirstOrDefault(x => x.Key == "swGen").Value;
                        uuid = device.UniqueDeviceName;
                        locat = device.LocationURL;

                        pl = ActivatorUtilities.CreateInstance(_provider, typeof(SonosPlayer)) as SonosPlayer;
                        pl.Name = name;
                        pl.UUID = uuid;
                        pl.DeviceLocation = new Uri(locat);
                        pl.ControlPoint = ControlPoint;

                        //pl = new SonosPlayer(serviceEnums, useSubscriptions, _icons, Logger)
                        //{
                        //    Name = name,
                        //    UUID = uuid,
                        //    DeviceLocation = new Uri(locat),
                        //    ControlPoint = ControlPoint
                        //};
                        if (swgen == "2")
                        {
                            pl.SoftwareGeneration = SoftwareGeneration.ZG2;
                        }
                        pl.SetDevice(device);
                        pl.Player_Changed += Player_Changed;
                        Players.Add(pl);
                        if (!Zone.Properties.PlayerPlayedPlaylist.ContainsKey(pl.UUID))
                            Zone.Properties.PlayerPlayedPlaylist.Add(pl.UUID, new Dictionary<string, int>());
                    }
                }
            }
        }
        #endregion Eventing
        #region private Methoden

        /// <summary>
        /// Initialisierung und Suchen der Sonosgeräte
        /// </summary>
        private void StartScan()
        {
            ControlPoint = new UPnPSmartControlPoint();
            ControlPoint.OnAddedDevice += OnDeviceAdded;
            ControlPoint.OnRemovedDevice += OnRemovedDevice;
        }
        private void OnRemovedDevice(UPnPSmartControlPoint sender, UPnPDevice device)
        {
            var dev = device as UPnPDevice;
            var pl = GetPlayerbyUuid(dev.UniqueDeviceName);
            pl.Player_Changed -= Player_Changed;
            Players.Remove(pl);
            playerDevices.Remove(dev.UniqueDeviceName);
        }

        /// <summary>
        /// Start Scan. Use Config for minimum foundet Player with Timeout.
        /// </summary>
        private void CtorInit()
        {
            StartScan();
            bool ok = false;
            DateTime startnow = DateTime.Now;
            int minFoundedPlayers = int.Parse(_config["MinFoundedPlayers"]);
            while (!ok)
            {
                //Timer, falls das suchen länger als 360 Sekunden dauert abbrechen
                int tdelta = (DateTime.Now - startnow).Seconds;
                if (Players.Count > minFoundedPlayers || tdelta > 360)
                {
                    ok = true;
                }
            }
        }
        private async void ZoneSwGen_GlobalSonosChange(object sender, Zone e)
        {
            //implementierung
            SonosEnums.EventingEnums t = (SonosEnums.EventingEnums)sender;
            switch (t)
            {
                case SonosEnums.EventingEnums.AlarmListVersion:
                    await GetSonosTimeStuff();
                    break;
                case SonosEnums.EventingEnums.FavoritesUpdateID:
                    await SetFavoritesPlaylists();
                    break;
                case SonosEnums.EventingEnums.SavedQueuesUpdateID:
                    await SetSonosPlaylist();
                    break;
                case SonosEnums.EventingEnums.ShareListUpdateID:
                    await SetImportetPlaylist();
                    break;

            }
            ManuellStateChange(t, DateTime.Now);
        }

        private void HandleZoneUUIDinGroup(SonosPlayer sp, List<ZoneGroupMember> zgm)
        {
            bool makenew = false;
            if (sp.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup.Count == 0 || sp.PlayerProperties.GroupCoordinatorIsLocal == false && sp.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup.Count == 1)
            {
                makenew = true;
            }
            else
            {
                foreach (var item in sp.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup)
                {
                    var z = zgm.FirstOrDefault(x => x.UUID == item);
                    if (z != null) continue;
                    makenew = true;
                    break;
                }
                foreach (var item in zgm)
                {
                    if (sp.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup.Contains(item.UUID)) continue;
                    makenew = true;
                    break;
                }
            }
            if (makenew)
            {
                sp.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup = zgm.Select(x => x.UUID).ToList();
                sp.LastChange = DateTime.Now;
                PlayerChange(SonosEnums.EventingEnums.ZonePlayerUUIDsInGroup, sp);
            }
        }
        /// <summary>
        /// Läd die Favoriten
        /// </summary>
        /// <param name="sleep"></param>
        private async Task<bool> SetFavoritesPlaylists()
        {
            bool retval = false;
            if (Players?.Count > 0)
            {
                SonosPlayer sp = Players.FirstOrDefault(x => x.SoftwareGeneration == SoftwareGeneration.ZG1);
                if (sp != null && sp.ContentDirectory != null)
                {
                    Zone.Properties.ListOfFavorites.Clear();
                    var br = await sp.ContentDirectory.Browse(BrowseObjects.Favorites);
                    Zone.Properties.ListOfFavorites = br.Result;
                    retval = true;
                }
            }
            return retval;
        }
        /// <summary>
        /// Läd die SonosPlaylists
        /// </summary>
        /// <param name="sleep"></param>
        private async Task<bool> SetSonosPlaylist()
        {
            bool retval = false;
            if (Players.Count > 0)
            {
                SonosPlayer sp = Players.FirstOrDefault(x => x.SoftwareGeneration == SoftwareGeneration.ZG1);
                if (sp != null && sp.ContentDirectory != null)
                {
                    var br = await sp.ContentDirectory.Browse(BrowseObjects.SonosPlaylist);
                    Zone.Properties.ListOfSonosPlaylist = br.Result;
                    retval = true;
                }
            }
            return retval;
        }
        /// <summary>
        /// Läd die M3U Playlist
        /// </summary>
        /// <param name="sleep"></param>
        private async Task<bool> SetImportetPlaylist()
        {
            bool retval = false;
            if (Players?.Count > 0)
            {
                SonosPlayer sp = Players.FirstOrDefault(x => x.SoftwareGeneration == SoftwareGeneration.ZG1);
                if (sp != null && sp.ContentDirectory != null)
                {
                    Zone.Properties.ListOfImportedPlaylist.Clear();
                    var br = await sp.ContentDirectory.Browse(BrowseObjects.ImportetPlaylist);
                    Zone.Properties.ListOfImportedPlaylist = br.Result;
                    retval = true;
                }
            }
            return retval;
        }
        #endregion private Methoden
    }
}