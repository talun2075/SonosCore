using OSTL.UPnP;
using SonosUPnP.DataClasses;
using SonosUPnP.Props;
using SonosUPnP.Services;
using SonosUPnP.Services.MediaRendererService;
using SonosUPnP.Services.MediaServerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using HomeLogging;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json.Serialization;
using SonosUPNPCore.Enums;
using SonosConst;

namespace SonosUPnP
{
    [Serializable]
    [DataContract]
    public class SonosPlayer
    {
        #region Klassenvariable und ctor
        public event EventHandler<SonosPlayer> Player_Changed = delegate { };
        [NonSerialized]
        private readonly Timer RelTimer;
        [NonSerialized]
        private readonly Timer RemSleeTimer;
        //[NonSerialized]
        //private Timer FillTimer;
        [NonSerialized]
        private Timer ServiceCheckTimer;
        private Boolean ServiceInit = false;
        private readonly Dictionary<string, string> _icons = new();
        /// <summary>
        /// nutze ich die subscriptions oder mache ich alles selber.
        /// </summary>
        private readonly Boolean useSubscription = false;
        /// <summary>
        /// Die erlaubten services aus der Web.config
        /// </summary>
        private readonly List<SonosEnums.Services> serviceEnums = new();
        private readonly List<SonosEnums.EventingEnums> IgnoreEvent = new() { SonosEnums.EventingEnums.LastChangedPlayState, SonosEnums.EventingEnums.ThirdPartyMediaServersX, SonosEnums.EventingEnums.SettingsReplicationState };
        [NonSerialized]
        private readonly ILogging Logger;
        public SonosPlayer(List<SonosEnums.Services> se, Boolean uSubscriptions = true, Dictionary<string, string> icons = null, ILogging log = null)
        {
            //todo: useSubscription kann gelöscht werden oder auch wieder über DI gemacht werden.
            if (icons != null)
                _icons = icons;
            useSubscription = uSubscriptions;
            serviceEnums = se;
            RelTimer = new Timer(state => RelTimeTimer(), null, 60000, Timeout.Infinite);
            RemSleeTimer = new Timer(state => RemainingSleepTimer(), null, 10000, Timeout.Infinite);
            if (log == null)
            {
                Logger = new Logging();
            }
            else
            {
                Logger = log;
            }
            RatingFilter.RatingFilter_Changed += RatingFilterChangedEvent;
        }
        #endregion Klassenvariable
        #region Public Methoden
        /// <summary>
        /// Aktiviert das Gerät
        /// </summary>
        /// <param name="playerDevice"></param>
        public void SetDevice(UPnPDevice playerDevice)
        {
            Device = playerDevice;
            PlayerProperties.BaseUrl = Device.RemoteEndPoint.ToString();
            try
            {
                lock (Device)
                {
                    if (_icons.ContainsKey(Device.IconName))
                    {
                        PlayerProperties.Icon = _icons[Device.IconName];
                    }
                    else
                    {
                        PlayerProperties.Icon = Device.IconURI;
                    }
                }
            }
            catch
            {
                //continue
            }
            ConnectionManagerMR = new Services.MediaRendererService.ConnectionManager(this);
            ConnectionManagerMS = new Services.MediaServerServices.ConnectionManager(this);
            AlarmClock = new AlarmClock(this);//Eventing in Discovery
            ContentDirectory = new ContentDirectory(this);//Eventing in Discovery
            QPlay = new QPlay(this); //Kein Event vorhanden
            ServiceCheckTimer = new Timer(state => ServiceCheck(), null, 9000, Timeout.Infinite);

        }
        /// <summary>
        /// Erweitert das logging um die Source.
        /// </summary>
        /// <param name="Method"></param>
        /// <param name="Source"></param>
        /// <param name="ex"></param>
        public void ServerErrorsAdd(String Method, String Source, Exception ex)
        {
            Logger.ServerErrorsAdd(Method, ex, Name + ":" + Source);
        }

        /// <summary>
        /// Liefert die Playlist eines Players bzw. füllt diese wenn leer.
        /// </summary>
        /// <param name="fillnew"></param>
        /// <param name="loadcurrent">läd Currentplaylist, da diese nicht mehr dem original entspricht. </param>
        /// <returns></returns>
        public async Task<Playlist> GetPlayerPlaylist(bool fillnew = false, bool loadcurrent = false)
        {
            try
            {
                if (fillnew || loadcurrent)
                    PlayerProperties.Playlist.ResetPlaylist();

                //Es gab fälle wo der Index beim initialisieren durcheinander kommt, daher eine Prüfung und löschen.
                if (PlayerProperties.Playlist.NumberReturned == 0 && PlayerProperties.Playlist.TotalMatches == 0 && !PlayerProperties.Playlist.IsEmpty)
                {
                    try
                    {
                        PlayerProperties.Playlist.PlayListItems.Clear();
                    }
                    catch
                    {
                        PlayerProperties.Playlist = new Playlist();
                    }
                }

                if (PlayerProperties.Playlist.IsEmpty)
                {
                        await PlayerProperties.Playlist.FillPlaylist(this);
                 }
                    if (!useSubscription || useSubscription && !serviceEnums.Contains(SonosEnums.Services.Queue))
                        GetPlaylistFireEvent();
            }
            catch (Exception ex)
            {
                ServerErrorsAdd("GetPlayerPlaylist", Name, ex);
            }
            return PlayerProperties.Playlist;
        }
        /// <summary>
        /// Prüft die Playerangaben und sendet bei unstimmigkeiten entsprechende Events
        /// </summary>
        /// <param name="overrule">Überschreibt die Werte in jedem Fall und sendet Werte (Reparatur Client)</param>
        /// <param name="CheckValues">Läd Daten vom Player und prüft. Sendet nur echte Änderungen (Reparatur Server)</param>
        public async Task<Boolean> FillPlayerPropertiesDefaultsAsync(Boolean overrule = false, Boolean CheckValues = false)
        {
            Boolean retval = true;
            try
            {
                if (AVTransport != null)
                {

                    try
                    {
                        if (PlayerProperties.TransportState == SonosEnums.TransportState.TRANSITIONING || overrule || CheckValues)
                        {

                            var ts = await AVTransport.GetTransportInfo();
                            if (PlayerProperties.TransportState != ts && ts != SonosEnums.TransportState.UNKNOWING || overrule)
                            {
                                PlayerProperties.TransportState = ts;
                                LastChange = DateTime.Now;
                                Player_Changed(SonosEnums.EventingEnums.TransportState, this);
                            }
                        }
                        if (PlayerProperties.CurrentPlayMode == SonosEnums.PlayModes.UNKNOWING || overrule || CheckValues)
                        {
                            var cp = await AVTransport.GetTransportSettings();
                            if (PlayerProperties.CurrentPlayMode != cp && cp != SonosEnums.PlayModes.UNKNOWING || overrule)
                            {
                                PlayerProperties.CurrentPlayMode = cp;
                                LastChange = DateTime.Now;
                                Player_Changed(SonosEnums.EventingEnums.CurrentPlayMode, this);
                            }
                        }
                        if (PlayerProperties.NumberOfTracks == 0 && !string.IsNullOrEmpty(PlayerProperties.AVTransportURI) || overrule || CheckValues)
                        {
                            var me = await AVTransport.GetMediaInfo();
                            if (!me.IsEmpty)
                            {
                                if (PlayerProperties.NumberOfTracks != me.NumberOfTracks || overrule)
                                {
                                    PlayerProperties.NumberOfTracks = me.NumberOfTracks;
                                    LastChange = DateTime.Now;
                                    Player_Changed(SonosEnums.EventingEnums.NumberOfTracks, this);
                                }
                                if (PlayerProperties.AVTransportURI != me.URI || overrule)
                                {
                                    PlayerProperties.AVTransportURI = me.URI;
                                    LastChange = DateTime.Now;
                                    Player_Changed(SonosEnums.EventingEnums.AVTransportURI, this);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.ServerErrorsAdd("FillPlayerPropertiesDefaults:AVTransport", ex, Name);
                        retval = false;
                    }
                }
                try
                {
                    if (PlayerProperties.GroupCoordinatorIsLocal == true && PlayerProperties.GroupRenderingControl_GroupVolume == 0 || overrule || CheckValues)
                    {
                        if (GroupRenderingControl != null)
                        {
                            var gpv = 0;
                            if (PlayerProperties.GroupCoordinatorIsLocal == true)
                            {
                                gpv = await GroupRenderingControl.GetGroupVolume();
                            }
                            if (PlayerProperties.GroupRenderingControl_GroupVolume != gpv || overrule)
                            {
                                PlayerProperties.GroupRenderingControl_GroupVolume = gpv;
                                LastChange = DateTime.Now;
                                Player_Changed(SonosEnums.EventingEnums.GroupVolume, this);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.ServerErrorsAdd("FillPlayerPropertiesDefaults:GroupRenderingControl", ex, Name);
                    retval = false;
                }
                try
                {
                    if (PlayerProperties.GroupCoordinatorIsLocal == true && PlayerProperties.LocalGroupUUID != UUID)
                    {
                        PlayerProperties.LocalGroupUUID = UUID;
                        LastChange = DateTime.Now;
                        Player_Changed(SonosEnums.EventingEnums.LocalGroupUUID, this);
                    }
                    if (PlayerProperties.GroupCoordinatorIsLocal == false && PlayerProperties.LocalGroupUUID == UUID || overrule)
                    {
                        if (ZoneGroupTopology != null)
                        {
                            //AlleZonen laden
                            var zgsreturn = await ZoneGroupTopology.GetZoneGroupState();
                            List<ZoneGroup> zgs = zgsreturn.ZoneGroupStates;
                            //Zonendurchlaufen
                            foreach (ZoneGroup zgm in zgs)
                            {
                                //Liste der Zonen durchlaufen.
                                var foundedmember = zgm.ZoneGroupMember.FirstOrDefault(x => x.UUID == UUID);
                                if (foundedmember != null)
                                {
                                    if (foundedmember.UUID == zgm.CoordinatorUUID)
                                    {
                                        //Player ist ZoneAdmin
                                        if (PlayerProperties.GroupCoordinatorIsLocal == false)
                                        {
                                            PlayerProperties.GroupCoordinatorIsLocal = true;
                                            LastChange = DateTime.Now;
                                            Player_Changed(SonosEnums.EventingEnums.GroupCoordinatorIsLocal, this);
                                        }
                                        if (PlayerProperties.LocalGroupUUID != UUID)
                                        {
                                            PlayerProperties.LocalGroupUUID = UUID;
                                            LastChange = DateTime.Now;
                                            Player_Changed(SonosEnums.EventingEnums.LocalGroupUUID, this);
                                        }
                                    }
                                    else
                                    {
                                        //Player ist KEIN ZoneAdmin
                                        if (PlayerProperties.GroupCoordinatorIsLocal == true)
                                        {
                                            PlayerProperties.GroupCoordinatorIsLocal = false;
                                            LastChange = DateTime.Now;
                                            Player_Changed(SonosEnums.EventingEnums.GroupCoordinatorIsLocal, this);
                                        }
                                        if (PlayerProperties.LocalGroupUUID != zgm.CoordinatorUUID)
                                        {
                                            PlayerProperties.LocalGroupUUID = zgm.CoordinatorUUID;
                                            LastChange = DateTime.Now;
                                            Player_Changed(SonosEnums.EventingEnums.LocalGroupUUID, this);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.ServerErrorsAdd("FillPlayerPropertiesDefaults:ZoneGroupTopology", ex, Name);
                    retval = false;
                }
                try
                {
                    if (PlayerProperties.Volume == 0 || overrule || CheckValues)
                    {
                        if (RenderingControl != null)
                        {
                            var vol = await RenderingControl.GetVolume();
                            if (PlayerProperties.Volume != vol && vol > 0 || overrule)
                            {
                                PlayerProperties.Volume = vol;
                                LastChange = DateTime.Now;
                                Player_Changed(SonosEnums.EventingEnums.Volume, this);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.ServerErrorsAdd("FillPlayerPropertiesDefaults:Volume", ex, Name);
                    retval = false;
                }
                try
                {
                    if (AVTransport != null)
                    {
                        PlayerInfo po = await AVTransport.GetPositionInfo();
                        if (!po.IsEmpty)
                        {
                            SonosItem checkcurrent = new();
                            if (!string.IsNullOrEmpty(po.TrackMetaData) && po.TrackMetaData != SonosConstants.NotImplemented)
                            {
                                checkcurrent = SonosItem.ParseSingleItem(po.TrackMetaData);
                            }
                            if (po.TrackIndex != PlayerProperties.CurrentTrackNumber || overrule)
                            {
                                PlayerProperties.CurrentTrackNumber = po.TrackIndex;
                                LastChange = DateTime.Now;
                                Player_Changed(SonosEnums.EventingEnums.CurrentTrackNumber, this);
                            }
                            if (PlayerProperties.CurrentTrack.MP3.IsEmpty() || checkcurrent.Uri != PlayerProperties.CurrentTrack.Uri || overrule)
                            {
                                if (!string.IsNullOrEmpty(po.TrackMetaData) && po.TrackMetaData != SonosConstants.NotImplemented)
                                {
                                    try
                                    {
                                        //Neu Wegen Stream
                                        //PlayerProperties.CurrentTrack = await SonosItemHelper.CheckItemForStreaming(checkcurrent, this);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.ServerErrorsAdd("FillPlayerPropertiesDefaults:Blockctp", ex, Name);

                                    }
                                    PlayerProperties.CurrentTrack.FillMP3AndItemFromHDD();
                                    LastChange = DateTime.Now;
                                    Player_Changed(SonosEnums.EventingEnums.CurrentTrack, this);
                                }
                            }
                            if (PlayerProperties.CurrentTrack.RelTime != po.RelTime || overrule)
                            {
                                PlayerProperties.CurrentTrack.RelTime = po.RelTime;
                                LastChange = DateTime.Now;
                                Player_Changed(SonosEnums.EventingEnums.RelTime, this);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.ServerErrorsAdd("FillPlayerPropertiesDefaults:GetPositionInfo", ex, Name);
                    retval = false;
                }
                //Playlist testen
                try
                {
                    if (string.IsNullOrEmpty(PlayerProperties.EnqueuedTransportURI))
                    {
                        //Es wurde kein EnqueuedTransportURI Eintrag gefunden. Wenn ich hier hin komme ist der der Timer durch und die Playlist befüllt. Daher kann ich meine Playlist befüllen. 
                        await GetPlayerPlaylist(false);
                    }
                    if (overrule)
                    {
                        await GetPlayerPlaylist(true);
                    }
                }
                catch (Exception ex)
                {
                    Logger.ServerErrorsAdd("FillPlayerPropertiesDefaults:EnqueuedTransportURI", ex, Name);
                    retval = false;
                }
                //Defaults bei Override testen 
                if (overrule || CheckValues)
                {
                    try
                    {
                        //Bei jeden Aufruf wird Grundsätzlich jeweils das Property geprüft und entsprechend gesetzt.
                        if (AVTransport != null)
                        {
                            try
                            {
                                PlayerProperties.CurrentCrossFadeMode = await AVTransport.GetCrossfadeMode();
                            }
                            catch (Exception ex)
                            {
                                Logger.ServerErrorsAdd("FillPlayerPropertiesDefaults:AVTransport2:1", ex, Name);
                            }
                            if (PlayerProperties.GroupCoordinatorIsLocal == true)
                            {
                                try
                                {
                                    await AVTransport.GetRemainingSleepTimerDuration();
                                }
                                catch (Exception ex)
                                {
                                    Logger.ServerErrorsAdd("FillPlayerPropertiesDefaults:AVTransport2:2", ex, Name);
                                }
                                if (overrule)
                                {
                                    Player_Changed(SonosEnums.EventingEnums.RemainingSleepTimerDuration, this);
                                    LastChange = DateTime.Now;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.ServerErrorsAdd("FillPlayerPropertiesDefaults:AVTransport2", ex, Name);
                    }
                    try
                    {
                        if (PlayerProperties.GroupCoordinatorIsLocal == true)
                        {
                            if (GroupRenderingControl != null)
                            {
                                await GroupRenderingControl.GetGroupMute();
                                if (overrule)
                                {
                                    Player_Changed(SonosEnums.EventingEnums.GroupMute, this);
                                    LastChange = DateTime.Now;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.ServerErrorsAdd("FillPlayerPropertiesDefaults:PlayerProperties.GroupCoordinatorIsLocal", ex, Name);
                    }
                    try
                    {
                        if (RenderingControl != null)
                        {
                            await RenderingControl.GetMute();
                            if (overrule)
                            {
                                Player_Changed(SonosEnums.EventingEnums.Mute, this);
                                LastChange = DateTime.Now;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.ServerErrorsAdd("FillPlayerPropertiesDefaults:RenderingControl", ex, Name);
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.ServerErrorsAdd("FillPlayerPropertiesDefaults:LastAndALL", ex, Name);
                retval = false;
            }
            Player_Changed(SonosEnums.EventingEnums.IsIdle, this);
            return retval;
        }
        public void CheckPlayerPropertiesWithClient(PlayerProperties pp)
        {
            if (PlayerProperties.TransportStateString != pp.TransportStateString)
            {
                Player_Changed(SonosEnums.EventingEnums.TransportState, this);
            }
            if (PlayerProperties.CurrentPlayModeString != pp.CurrentPlayModeString)
            {
                Player_Changed(SonosEnums.EventingEnums.CurrentPlayMode, this);
            }
            if (PlayerProperties.CurrentCrossFadeMode != pp.CurrentCrossFadeMode)
            {
                Player_Changed(SonosEnums.EventingEnums.CurrentCrossFadeMode, this);
            }
            if (PlayerProperties.GroupCoordinatorIsLocal != pp.GroupCoordinatorIsLocal)
            {
                Player_Changed(SonosEnums.EventingEnums.GroupCoordinatorIsLocal, this);
            }
            if (PlayerProperties.LocalGroupUUID != pp.LocalGroupUUID)
            {
                Player_Changed(SonosEnums.EventingEnums.LocalGroupUUID, this);
            }
            if (PlayerProperties.NumberOfTracks != pp.NumberOfTracks)
            {
                Player_Changed(SonosEnums.EventingEnums.NumberOfTracks, this);
            }
            if (PlayerProperties.CurrentTrackNumber != pp.CurrentTrackNumber)
            {
                Player_Changed(SonosEnums.EventingEnums.CurrentTrackNumber, this);
            }
            if (PlayerProperties.DeviceProperties_IsIdle != pp.DeviceProperties_IsIdle)
            {
                Player_Changed(SonosEnums.EventingEnums.IsIdle, this);
            }
            if (PlayerProperties.CurrentTrack.Uri != pp.CurrentTrack.Uri)
            {
                Player_Changed(SonosEnums.EventingEnums.CurrentTrack, this);
            }
            if (PlayerProperties.RemainingSleepTimerDuration != pp.RemainingSleepTimerDuration)
            {
                Player_Changed(SonosEnums.EventingEnums.RemainingSleepTimerDuration, this);
            }
            if (PlayerProperties.SleepTimerRunning != pp.SleepTimerRunning)
            {
                Player_Changed(SonosEnums.EventingEnums.SleepTimerRunning, this);
            }
            if (PlayerProperties.Volume != pp.Volume)
            {
                Player_Changed(SonosEnums.EventingEnums.Volume, this);
            }
            if (PlayerProperties.GroupRenderingControl_GroupVolume != pp.GroupRenderingControl_GroupVolume)
            {
                Player_Changed(SonosEnums.EventingEnums.GroupVolume, this);
            }
            if (PlayerProperties.Mute != pp.Mute)
            {
                Player_Changed(SonosEnums.EventingEnums.Mute, this);
            }
            if (PlayerProperties.GroupRenderingControl_GroupMute != pp.GroupRenderingControl_GroupMute)
            {
                Player_Changed(SonosEnums.EventingEnums.GroupMute, this);
            }
        }
        #endregion Public Methoden
        #region Eigenschaften
        [DataMember(Name = "Name")]
        public string Name { get; set; }
        [DataMember(Name = "UUID")]
        public string UUID { get; set; }
        [DataMember(Name = "LastChange")]
        public DateTime LastChange { get; set; }

        [DataMember(Name = "SoftwareGeneration")]
        public SoftwareGeneration SoftwareGeneration { get; set; } = SoftwareGeneration.ZG1;
        /// <summary>
        /// Eigenschaft welches Gerät aktiv ist
        /// </summary>
        [JsonIgnore]
        public UPnPDevice Device { get; set; }
        [JsonIgnore]
        public Uri DeviceLocation { get; set; }
        /// <summary>
        /// Rating Filter für das durchsuchen. Ist der Filter aktiv, wird dieser genommen um beim Browsen Songs zu filtern.
        /// </summary>
        [DataMember(Name = "RatingFilter")]
        public SonosRatingFilter RatingFilter { get; set; } = new SonosRatingFilter();
        [JsonIgnore]
        public SystemProperties SystemProperties { get; private set; }
        [JsonIgnore]
        public AVTransport AVTransport { get; private set; }
        [JsonIgnore]
        public Services.MediaRendererService.ConnectionManager ConnectionManagerMR { get; private set; }
        [JsonIgnore]
        public GroupRenderingControl GroupRenderingControl { get; private set; }
        [JsonIgnore]
        public Queue Queue { get; private set; }
        [JsonIgnore]
        public RenderingControl RenderingControl { get; private set; }
        [JsonIgnore]
        public Services.MediaServerServices.ConnectionManager ConnectionManagerMS { get; private set; }
        [JsonIgnore]
        public ContentDirectory ContentDirectory { get; private set; }
        [JsonIgnore]
        public ZoneGroupTopology ZoneGroupTopology { get; private set; }
        [JsonIgnore]
        public QPlay QPlay { get; private set; }
        [JsonIgnore]
        public AlarmClock AlarmClock { get; private set; }
        [JsonIgnore]
        public DeviceProperties DeviceProperties { get; private set; }
        [JsonIgnore]
        public GroupManagement GroupManagement { get; private set; }
        [JsonIgnore]
        public MusicServices MusicServices { get; private set; }
        [JsonIgnore]
        public AudioIn AudioIn { get; private set; }
        [DataMember(Name = "Name")]
        public PlayerProperties PlayerProperties { get; set; } = new PlayerProperties();
        /// <summary>
        /// Unbekannt
        /// </summary>
        [JsonIgnore]
        public UPnPSmartControlPoint ControlPoint { get; set; }
        //public List<Playlist> AllFilledPlaylist { get; set; } = new List<Playlist>();
        #endregion Eigenschaften
        #region private Methoden
        /// <summary>
        /// Feuert ein Event, dass die Playlist geändert wurde.
        /// </summary>
        private void GetPlaylistFireEvent()
        {
            LastChange = DateTime.Now;
            Player_Changed(SonosEnums.EventingEnums.QueueChangedNoRefillNeeded, this);
        }
        /// <summary>
        /// Gibt dem ControlPoint die anweisung das Gerät der Liste hinzuzufügen und somit den Player neu zu initialisieren.
        /// </summary>
        internal void LoadDevice()
        {
            if (ControlPoint != null && !string.IsNullOrEmpty(DeviceLocation.ToString()))
            {
                ControlPoint.ForceDeviceAddition(DeviceLocation);
            }
        }
        /// <summary>
        /// Wird aufgerufen, wenn ein Rating sich geändert hat. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RatingFilterChangedEvent(object sender, SonosRatingFilter e)
        {
            Player_Changed(SonosEnums.EventingEnums.RatingFilter, this);
        }
        /// <summary>
        /// Wird aufgerufen, wenn sich ein Service geändert hat.
        /// </summary>
        /// <param name="sender">EventingEnum</param>
        /// <param name="e">SonosPlayer</param>
        private async void Service_Changed(object sender, SonosPlayer e)
        {
            //zuIgnorierendeEvents definieren
            SonosEnums.EventingEnums ev = (SonosEnums.EventingEnums)sender;
            if (IgnoreEvent.Contains(ev)) return;
            LastChange = DateTime.Now;
            try
            {

                if (ev == SonosEnums.EventingEnums.QueueChangeResort || ev == SonosEnums.EventingEnums.CurrentPlayMode || (!e.PlayerProperties.CurrentTrack.IsEmtpy() && e.PlayerProperties.CurrentTrackNumber != 0 && e.PlayerProperties.Playlist.PlayListItems.Count > 0 && e.PlayerProperties.Playlist.PlayListItems.FirstOrDefault(x => x.Uri == e.PlayerProperties.CurrentTrack.Uri) == null))
                {
                    if (!string.IsNullOrEmpty(PlayerProperties.EnqueuedTransportURI))
                    {
                        PlayerProperties.EnqueuedTransportURI = String.Empty;
                        Player_Changed(SonosEnums.EventingEnums.EnqueuedTransportURI, this);
                    }
                    await GetPlayerPlaylist(true, true);
                }
            }
            catch (Exception ex)
            {
                var k = ex.Message;
            }
            if (ev == SonosEnums.EventingEnums.CurrentTrack)
            {
                PlayerProperties.CurrentTrack = await SonosItemHelper.CheckItemForStreaming(PlayerProperties.CurrentTrack, this);
                if (!PlayerProperties.CurrentTrack.Stream)
                    PlayerProperties.CurrentTrack.FillMP3AndItemFromHDD();
            }
            if (ev == SonosEnums.EventingEnums.QueueChanged)
            {
                //todo: hier evtl den kommentar einbauen um die AllPlaliyst Selection zu reduzieren.
                //|| ev == SonosEnums.EventingEnums.QueueChangedNoRefillNeeded
                //if (!string.IsNullOrEmpty(PlayerProperties.EnqueuedTransportURI))
                //{
                //    PlayerProperties.EnqueuedTransportURI = String.Empty;
                //    Player_Changed(SonosEnums.EventingEnums.EnqueuedTransportURI, this);
                //}
                //PlayerProperties.Playlist.NumberReturned = PlayerProperties.Playlist.TotalMatches = PlayerProperties.Playlist.PlayListItems.Count;
                PlayerProperties.Playlist.ResetPlaylist();
            }
            
            if (ev == SonosEnums.EventingEnums.TransportState || ev == SonosEnums.EventingEnums.IsIdle)
            {
                //Abspieler in Schleife laden.
                RelTimeTimer();
            }
            if (ev == SonosEnums.EventingEnums.SleepTimerRunning)
            {
                //RemainingSleepTimer Starten
                RemainingSleepTimer();
            }
            if (ev == SonosEnums.EventingEnums.GroupCoordinatorIsLocal)
            {
                if (PlayerProperties.GroupCoordinatorIsLocal == true)
                {
                    if (PlayerProperties.TransportState == SonosEnums.TransportState.PLAYING)
                    {
                        PlayerProperties.TransportState = SonosEnums.TransportState.PAUSED_PLAYBACK;
                        Player_Changed(SonosEnums.EventingEnums.TransportState, e);
                        return;
                    }
                }
            }
            Player_Changed(sender, e);
            Debug.WriteLine("Player:" + e.Name + " EventType:" + ev.ToString());
        }
        /// <summary>
        /// Liefert regelmäßig in die EventQueue die Dauer eines Songs. 
        /// </summary>
        private async void RelTimeTimer()
        {
            if (PlayerProperties.TransportState == SonosEnums.TransportState.PLAYING && PlayerProperties.LocalGroupUUID == UUID)
            {
                RelTimer.Change(3000, 3000);
                var po = await AVTransport.GetPositionInfo();
                if (!po.IsEmpty)
                {
                    if (PlayerProperties.CurrentTrack.RelTime != po.RelTime)
                    {
                        PlayerProperties.CurrentTrack.RelTime = po.RelTime;
                        LastChange = DateTime.Now;
                        Player_Changed(SonosEnums.EventingEnums.RelTime, this);
                    }
                    if (po.TrackURI != PlayerProperties.CurrentTrack.Uri && !string.IsNullOrEmpty(po.TrackMetaData) && po.TrackMetaData != SonosConstants.NotImplemented)
                    {
                        PlayerProperties.CurrentTrack = SonosItem.ParseSingleItem(po.TrackMetaData);
                        PlayerProperties.CurrentTrack.FillMP3AndItemFromHDD();
                        LastChange = DateTime.Now;
                        Player_Changed(SonosEnums.EventingEnums.CurrentTrack, this);
                    }
                    if (po.TrackIndex > 0 && po.TrackIndex != PlayerProperties.CurrentTrackNumber)
                    {
                        PlayerProperties.CurrentTrackNumber = po.TrackIndex;
                        LastChange = DateTime.Now;
                        Player_Changed(SonosEnums.EventingEnums.CurrentTrackNumber, this);
                    }
                    if (po.TrackDuration != PlayerProperties.CurrentTrack.Duration)
                    {
                        PlayerProperties.CurrentTrack.Duration = po.TrackDuration;
                        LastChange = DateTime.Now;
                        Player_Changed(SonosEnums.EventingEnums.CurrentTrack, this);
                    }
                }
            }
            else
            {
                RelTimer.Change(30000, 30000);
            }

        }
        /// <summary>
        /// Liefert regelmäßig den Remaining SleepTimer zurück
        /// </summary>
        private async void RemainingSleepTimer()
        {
            if (PlayerProperties.SleepTimerRunning)
            {
                RemSleeTimer.Change(2000, 2000);
                await AVTransport.GetRemainingSleepTimerDuration();
            }
            else
            {
                RemSleeTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }

        }
        /// <summary>
        /// Prüft ob die Services ordentlich initialisiert wurde.
        /// </summary>
        public void ServiceCheck()
        {
            if (ServiceInit) return;
            ServiceInit = true;
            AVTransport = new AVTransport(this);
            AVTransport.AVTransport_Changed += Service_Changed;
            Queue = new Queue(this);
            Queue.Queue_Changed += Service_Changed;
            RenderingControl = new RenderingControl(this);
            RenderingControl.RenderingControl_Changed += Service_Changed;
            GroupManagement = new GroupManagement(this);
            GroupManagement.GroupManagement_Changed += Service_Changed;
            DeviceProperties = new DeviceProperties(this);
            DeviceProperties.DeviceProperties_Changed += Service_Changed;
            GroupRenderingControl = new GroupRenderingControl(this);
            GroupRenderingControl.GroupRenderingControl_Changed += Service_Changed;
            ZoneGroupTopology = new ZoneGroupTopology(this);
            ZoneGroupTopology.ZoneGroupTopology_Changed += Service_Changed;
            MusicServices = new MusicServices(this);
            MusicServices.MusicServices_Changed += Service_Changed;
            SystemProperties = new SystemProperties(this);
            SystemProperties.SystemProperties_Changed += Service_Changed;
            AudioIn = new AudioIn(this);
            AudioIn.AudioIn_Changed += Service_Changed;
            if (useSubscription)
            {
                if (serviceEnums.Contains(SonosEnums.Services.AVTransport))
                {
                    AVTransport.SubscripeToEvents();
                }
                if (serviceEnums.Contains(SonosEnums.Services.Queue))
                {
                    Queue.SubscripeToEvents();
                }
                if (serviceEnums.Contains(SonosEnums.Services.RenderingControl))
                {
                    RenderingControl.SubscripeToEvents();
                }
                if (serviceEnums.Contains(SonosEnums.Services.GroupManagement))
                {
                    List<SonosEnums.EventingEnums> ee = new() { SonosEnums.EventingEnums.GroupCoordinatorIsLocal, SonosEnums.EventingEnums.ResetVolumeAfter, SonosEnums.EventingEnums.VirtualLineInGroupID, SonosEnums.EventingEnums.VolumeAVTransportURI };
                    GroupManagement.SubscripeToEvents(ee);
                }
                if (serviceEnums.Contains(SonosEnums.Services.DeviceProperties))
                {
                    DeviceProperties.SubscripeToEvents();
                }
                if (serviceEnums.Contains(SonosEnums.Services.GroupRenderingControl))
                {
                    GroupRenderingControl.SubscripeToEvents();
                }
                if (serviceEnums.Contains(SonosEnums.Services.ZoneGroupTopology))
                {
                    ZoneGroupTopology.SubscripeToEvents();
                }
                if (serviceEnums.Contains(SonosEnums.Services.MusicServices))
                {
                    MusicServices.SubscripeToEvents();
                }
                if (serviceEnums.Contains(SonosEnums.Services.SystemProperties))
                {
                    SystemProperties.SubscripeToEvents();
                }
                if (serviceEnums.Contains(SonosEnums.Services.AudioIn))
                {
                    AudioIn.SubscripeToEvents();
                }
            }
        }
        #endregion private Methoden
    }
}