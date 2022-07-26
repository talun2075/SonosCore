using SonosConst;
using SonosUPnP.DataClasses;
using SonosUPNPCore;
using SonosUPNPCore.Enums;
using SonosUPNPCore.Props;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SonosUPnP.Props
{
    public class PlayerProperties
    {
        #region AvTransport
        [IgnoreDataMember]
        public int InstanceID { get; set; } = 0;
        /// <summary>
        /// Stellt dar, ob gerade eine Wiedergabe läuft oder ob Pausiert wird.
        /// </summary>
        public SonosEnums.TransportState TransportState { get; set; } = SonosEnums.TransportState.TRANSITIONING;
        /// <summary>
        /// Stellt dar, ob gerade eine Wiedergabe läuft oder ob Pausiert wird.
        /// </summary>
        public String TransportStateString => TransportState.ToString();
        /// <summary>
        /// Aktuelle Wiedergabeart
        /// </summary>
        public SonosEnums.PlayModes CurrentPlayMode { get; set; } = SonosEnums.PlayModes.UNKNOWING;
        /// <summary>
        /// Aktuelle Wiedergabeart asl String
        /// </summary>
        public String CurrentPlayModeString => CurrentPlayMode.ToString();
        /// <summary>
        /// Gibt an, ob Fade aktiviert ist.
        /// </summary>
        public Boolean? CurrentCrossFadeMode { get; set; } = null;
        /// <summary>
        /// Anzahl der Tracks in der aktuellen Wiedergabeliste
        /// </summary>
        public int NumberOfTracks { get; set; } = 0;
        /// <summary>
        /// Song nummer die gerade abgespielt wird in der Wiedergabeliste
        /// </summary>
        public int CurrentTrackNumber { get; set; } = 0;
        /// <summary>
        /// Aktueller Song als SonosItem
        /// </summary>
        public SonosItem CurrentTrack { get; set; } = new SonosItem();
        [IgnoreDataMember]
        public string CurrentSection { get; set; } = "0";
        /// <summary>
        /// Liefert den nächsten Track aus.
        /// </summary>
        public SonosItem NextTrack { get; set; } = new SonosItem();
        /// <summary>
        /// Zeigt die Quell URI an die abgespielt wird. Nicht AV Transport wie z.B. aktuelle Wiedergabe oder Eingang
        /// </summary>
        public string EnqueuedTransportURI { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string EnqueuedTransportURIMetaDataString { get; set; } = String.Empty;
        public SonosItem EnqueuedTransportURIMetaData { get; set; } = new SonosItem();
        public SonosEnums.PlaybackStorageMedium PlaybackStorageMedium { get; set; }
        public string PlaybackStorageMediumString => PlaybackStorageMedium.ToString();
        /// <summary>
        /// Zeigt die Physikalische Quelle des Abspielens an wie z.B. Wiedergabeliste oder Eingang aber nicht was gespielt wird.
        /// </summary>
        public string AVTransportURI { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string AVTransportURIMetaData { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string NextAVTransportURI { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string NextAVTransportURIMetaData { get; set; } = String.Empty;
        [IgnoreDataMember]
        public List<string> CurrentTransportActions { get; set; } = new List<string>();
        [IgnoreDataMember]
        public List<string> CurrentValidPlayModes { get; set; } = new List<string>();
        [IgnoreDataMember]
        public string MuseSessions { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string DirectControlClientID { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string DirectControlIsSuspended { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string DirectControlAccountID { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string SleepTimerGeneration { get; set; } = String.Empty;
        /// <summary>
        /// Liefert ob ein Sleeptimer am laufen ist.
        /// </summary>
        public Boolean SleepTimerRunning { get; set; } = false;
        public Boolean AlarmRunning { get; set; } = false;
        public Boolean SnoozeRunning { get; set; } = false;
        public Boolean RestartPending { get; set; } = false;
        [IgnoreDataMember]
        public string TransportPlaySpeed { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string CurrentMediaDuration { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string RecordStorageMedium { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string PossiblePlaybackStorageMedia { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string PossibleRecordStorageMedia { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string RecordMediumWriteStatus { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string CurrentRecordQualityMode { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string PossibleRecordQualityModes { get; set; } = String.Empty;
        #endregion AvTransport
        #region AlarmClock
        /// <summary>
        /// Datumsformat: YMD / MDY / DMY
        /// </summary>
        [IgnoreDataMember]
        public String DateFormat { get; set; } = String.Empty;
        /// <summary>
        /// Zeitformat 12H oder 24H
        /// </summary>
        [IgnoreDataMember]
        public String TimeFormat { get; set; } = String.Empty;
        /// <summary>
        /// Unbekannt
        /// </summary>
        [IgnoreDataMember]
        public int TimeGeneration { get; set; } = 0;
        /// <summary>
        /// Wenn leer wird die Zeit Manuell gestellt ansonsten vom Server geholt.
        /// </summary>
        [IgnoreDataMember]
        public String TimeServer { get; set; } = String.Empty;
        /// <summary>
        /// InternalString der SonosTimeZone Klasse
        /// </summary>
        [IgnoreDataMember]
        public String TimeZone { get; set; } = String.Empty;
        /// <summary>
        /// Index AKtualisierungszeitpunkt im Format HH:MM:SS
        /// </summary>
        [IgnoreDataMember]
        public String DailyIndexRefreshTime { get; set; } = String.Empty;
        /// <summary>
        /// Wird bei Änderungen hochgezählt
        /// </summary>
        [IgnoreDataMember]
        public int AlarmListVersion { get; set; } = 0;
        /// <summary>
        /// Name der an der Oberfläche angezeigt werden soll.
        /// </summary>
        #endregion AlarmClock
        #region AudioIn
        public string AudioInput_Name { get; set; } = String.Empty;
        /// <summary>
        /// Bild das in der Oberfläche angezeigt werden soll. 
        /// Bisher bekannt: HomeTheater und AudioComponent
        /// </summary>
        public string AudioInput_Icon { get; set; } = String.Empty;
        /// <summary>
        /// Vermutlich Lautstärke Werte zwischen 1 und 12 erhalten
        /// </summary>
        public int AudioInput_LeftLineInLevel { get; set; } = 0;
        /// <summary>
        /// Prüft ob ein AudioIn angeschlossen ist. 
        /// </summary>
        public Boolean AudioInput_LineInConnected { get; set; } = false;
        /// <summary>
        /// Bisher unbekannt wird scheinbar nicht genutzt.
        /// </summary>
        [IgnoreDataMember]
        public string AudioInput_Playing { get; set; } = String.Empty;
        /// <summary>
        /// Vermutlich Lautstärke Werte zwischen 1 und 12 erhalten
        /// </summary>
        public int AudioInput_RightLineInLevel { get; set; } = 0;
        #endregion AudioIn
        #region DeviceProperties
        public Boolean DeviceProperties_AirPlayEnabled { get; set; } = false;
        [IgnoreDataMember]
        public string DeviceProperties_AvailableRoomCalibration { get; set; } = String.Empty;
        public Boolean DeviceProperties_BehindWifiExtender { get; set; } = false;
        [IgnoreDataMember]
        public int DeviceProperties_ChannelFreq { get; set; } = 2462;
        public string DeviceProperties_ChannelMapSet { get; set; } = String.Empty;
        public string DeviceProperties_ConfigMode { get; set; } = String.Empty;

        public string DeviceProperties_Configuration { get; set; } = "1";
        public Boolean DeviceProperties_HasConfiguredSSID { get; set; } = true;
        public string DeviceProperties_HdmiCecAvailable { get; set; } = "0";
        public int DeviceProperties_HTBondedZoneCommitState { get; set; } = 0;
        public string DeviceProperties_HTFreq { get; set; } = String.Empty;
        public string DeviceProperties_HTSatChanMapSet { get; set; } = String.Empty;
        public string DeviceProperties_Icon { get; set; } = String.Empty;
        public Boolean DeviceProperties_Invisible { get; set; } = false;
        public Boolean DeviceProperties_IsIdle { get; set; } = true;
        public Boolean DeviceProperties_IsZoneBridge { get; set; } = false;
        [IgnoreDataMember]
        public string DeviceProperties_LastChangedPlayState { get; set; } = String.Empty;
        [IgnoreDataMember]
        public int DeviceProperties_Orientation { get; set; } = 0;
        public string DeviceProperties_RoomCalibrationState { get; set; } = "1";
        [IgnoreDataMember]
        public int DeviceProperties_SecureRegState { get; set; } = 3;
        [IgnoreDataMember]
        public string DeviceProperties_SettingsReplicationState { get; set; } = String.Empty;
        public Boolean DeviceProperties_SupportsAudioIn { get; set; } = false;
        public string DeviceProperties_TVConfigurationError { get; set; } = "0";
        public string DeviceProperties_VoiceControlState { get; set; } = "0";
        public Boolean DeviceProperties_WifiEnabled { get; set; } = false;
        public Boolean DeviceProperties_WirelessLeafOnly { get; set; } = false;
        public int DeviceProperties_WirelessMode { get; set; } = 0;
        public string DeviceProperties_ZoneName { get; set; } = String.Empty;
        #endregion DeviceProperties
        #region GroupManagement
        /// <summary>
        /// Liefert zurück ob es der Zonecoordinator ist.
        /// </summary>
        public Boolean GroupCoordinatorIsLocal { get; set; } = true;
        /// <summary>
        /// Definiert den Coordinator der Gruppe
        /// </summary>
        public string LocalGroupUUID { get; set; } = String.Empty;
        public string GroupManagement_ResetVolumeAfter { get; set; } = String.Empty;

        public string GroupManagement_VirtualLineInGroupID { get; set; } = String.Empty;
        public string GroupManagement_VolumeAVTransportURI { get; set; } = String.Empty;
        #endregion GroupManagement
        #region Systemproperties
        public string SystemProperties_CustomerID { get; set; } = String.Empty;
        public string SystemProperties_ThirdPartyHash { get; set; } = String.Empty;
        public string SystemProperties_UpdateID { get; set; } = String.Empty;
        public string SystemProperties_UpdateIDX { get; set; } = String.Empty;
        public string SystemProperties_VoiceUpdateID { get; set; } = String.Empty;
        public string MusicServices_ServiceListVersion { get; set; } = String.Empty;
        #endregion Systemproperties
        #region ZoneGroupTopology
        public string ZoneGroupTopology_AlarmRunSequence { get; set; } = String.Empty;
        public string ZoneGroupTopology_ZoneGroupID { get; set; } = String.Empty;
        public string ZoneGroupTopology_ZoneGroupName { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string ZoneGroupTopology_AvailableSoftwareUpdate { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string ZoneGroupTopology_MuseHouseholdId { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string ZoneGroupTopology_ThirdPartyMediaServersX { get; set; } = String.Empty;
        public List<String> ZoneGroupTopology_ZonePlayerUUIDsInGroup { get; set; } = new List<string>();
        [IgnoreDataMember]
        internal string ZoneGroupTopology_ZonePlayerUUIDsInGroupAsString
        {
            get
            {

                return String.Join(",", ZoneGroupTopology_ZonePlayerUUIDsInGroup);
            }
        }
        #endregion ZoneGroupTopology
        #region Mediarenderer Connectionmanager
        [IgnoreDataMember]
        public string MR_ConnectionManager_CurrentConnectionIDs { get; set; } = String.Empty;
        [IgnoreDataMember]
        public List<String> MR_ConnectionManager_SinkProtocolInfo { get; set; }
        [IgnoreDataMember]
        public string MR_ConnectionManager_SourceProtocolInfo { get; set; } = String.Empty;
        #endregion Mediarenderer Connectionmanager
        #region MediaServer Connectionmanager
        [IgnoreDataMember]
        public string MS_ConnectionManager_CurrentConnectionIDs { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string MS_ConnectionManager_SinkProtocolInfo { get; set; } = String.Empty;
        [IgnoreDataMember]
        public List<String> MS_ConnectionManager_SourceProtocolInfo { get; set; }
        #endregion MediaServer Connectionmanager
        #region GroupRenderingControl
        public Boolean GroupRenderingControl_GroupMute { get; set; } = false;
        public int GroupRenderingControl_GroupVolume { get; set; } = 0;
        public Boolean GroupRenderingControl_GroupVolumeChangeable { get; set; } = true;
        #endregion GroupRenderingControl
        #region ContentDirectory
        //Wird nicht ausgegeben und auch nicht abnoiert, da dies in der Discovery gemacht wird.
        [IgnoreDataMember]
        public string ContentDirectory_Browseable { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string ContentDirectory_ContainerUpdateIDs { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string ContentDirectory_FavoritePresetsUpdateID { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string ContentDirectory_FavoritesUpdateID { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string ContentDirectory_RadioFavoritesUpdateID { get; set; } = String.Empty;
        [IgnoreDataMember]
        public int ContentDirectory_RadioLocationUpdateID { get; set; } = 0;
        [IgnoreDataMember]
        public string ContentDirectory_RecentlyPlayedUpdateID { get; set; } = String.Empty;
        [IgnoreDataMember]
        public int ContentDirectory_SavedQueuesUpdateID { get; set; } = 0;
        /// <summary>
        /// Gibt an ob der Musikindex gerade aktualisiert wird.
        /// </summary>
        [IgnoreDataMember]
        public Boolean ContentDirectory_ShareIndexInProgress { get; set; } = false;
        [IgnoreDataMember]
        public string ContentDirectory_ShareIndexLastError { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string ContentDirectory_ShareListUpdateID { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string ContentDirectory_SystemUpdateID { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string ContentDirectory_UserRadioUpdateID { get; set; } = String.Empty;
        #endregion ContentDirectory
        #region Eigene Eigenschaften
        /// <summary>
        /// Das Icon des Playertyps
        /// </summary>
        public String Icon { get; internal set; } = String.Empty;
        /// <summary>
        /// Pfadangaben des Sonos Geräts (ohne XML)
        /// </summary>
        public string BaseUrl { get; set; } = String.Empty;
        public Playlist Playlist { get; set; } = new Playlist();
        /// <summary>
        /// Buttons am Gerät aktiv oder nicht.
        /// </summary>
        public OnOffSwitch ButtonLockState { get; set; } = OnOffSwitch.NotSet;
        /// <summary>
        /// LED Status an/aus
        /// </summary>
        public OnOffSwitch LEDState { get; set; } = OnOffSwitch.NotSet;

        #endregion Eigene Eigenschaften
        #region RenderingControl
        /// <summary>
        /// Aktuelle Lautstärke
        /// </summary>
        public int Volume { get; set; } = 0;
        /// <summary>
        /// Hält als String die Infos, wie lange noch abgespielt wird, bis die Musik aus ist.
        /// </summary>
        public String RemainingSleepTimerDuration { get; set; } = SonosConstants.Off;
        
        /// <summary>
        /// Stummschaltung
        /// </summary>
        public Boolean Mute { get; set; } = false;
        /// <summary>
        /// Werte zwischen -10 und +10
        /// </summary>
        public int Bass { get; set; } = -2;
        /// <summary>
        /// Werte zwischen -10 und +10
        /// </summary>
        public int Treble { get; set; } = -2;
        public bool Loudness { get; set; } = false;
        public bool OutputFixed { get; set; } = false;
        public bool SupportOutputFixed { get; set; } = false;
        public bool HeadphoneConnected { get; set; } = false;
        [IgnoreDataMember]
        public string PresetNameList { get; set; } = "FactoryDefaults";
        public int SpeakerSize { get; set; } = 0;
        [IgnoreDataMember]
        public string SubGain { get; internal set; } = "0";
        [IgnoreDataMember]
        public string SubCrossover { get; internal set; } = "0";
        [IgnoreDataMember]
        public string SubPolarity { get; internal set; } = "0";
        public bool SubEnabled { get; internal set; } = false;
        [IgnoreDataMember]
        public string SonarEnabled { get; internal set; } = "FactoryDefaults";
        [IgnoreDataMember]
        public bool SonarCalibrationAvailable { get; internal set; } = false;
        #endregion RenderingControl
        #region Queue
        /// <summary>
        /// Wird aufgrerufen, wenn sich die Playlist ändert
        /// </summary>
        public int QueueChanged { get; internal set; } = 0;
        #endregion Queue
        #region Methoden

        internal SonosTimeSpan ParseDuration(string value)
        {
            if (TimeSpan.TryParse(value, out TimeSpan pd))
            {
                return new SonosTimeSpan(pd);
            }
            return new SonosTimeSpan();
        }
        #endregion Methoden

    }
}
