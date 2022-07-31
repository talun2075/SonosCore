using SonosConst;
using SonosData;
using SonosData.DataClasses;
using SonosData.Props;
using SonosData.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SonosUPNPCore.Classes
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
        public string TransportStateString => TransportState.ToString();
        /// <summary>
        /// Aktuelle Wiedergabeart
        /// </summary>
        public SonosEnums.PlayModes CurrentPlayMode { get; set; } = SonosEnums.PlayModes.UNKNOWING;
        /// <summary>
        /// Aktuelle Wiedergabeart asl String
        /// </summary>
        public string CurrentPlayModeString => CurrentPlayMode.ToString();
        /// <summary>
        /// Gibt an, ob Fade aktiviert ist.
        /// </summary>
        public bool? CurrentCrossFadeMode { get; set; } = null;
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
        public string EnqueuedTransportURI { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string EnqueuedTransportURIMetaDataString { get; set; } = string.Empty;
        public SonosItem EnqueuedTransportURIMetaData { get; set; } = new SonosItem();
        public SonosEnums.PlaybackStorageMedium PlaybackStorageMedium { get; set; }
        public string PlaybackStorageMediumString => PlaybackStorageMedium.ToString();
        /// <summary>
        /// Zeigt die Physikalische Quelle des Abspielens an wie z.B. Wiedergabeliste oder Eingang aber nicht was gespielt wird.
        /// </summary>
        public string AVTransportURI { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string AVTransportURIMetaData { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string NextAVTransportURI { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string NextAVTransportURIMetaData { get; set; } = string.Empty;
        [IgnoreDataMember]
        public List<string> CurrentTransportActions { get; set; } = new List<string>();
        [IgnoreDataMember]
        public List<string> CurrentValidPlayModes { get; set; } = new List<string>();
        [IgnoreDataMember]
        public string MuseSessions { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string DirectControlClientID { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string DirectControlIsSuspended { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string DirectControlAccountID { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string SleepTimerGeneration { get; set; } = string.Empty;
        /// <summary>
        /// Liefert ob ein Sleeptimer am laufen ist.
        /// </summary>
        public bool SleepTimerRunning { get; set; } = false;
        public bool AlarmRunning { get; set; } = false;
        public bool SnoozeRunning { get; set; } = false;
        public bool RestartPending { get; set; } = false;
        [IgnoreDataMember]
        public string TransportPlaySpeed { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string CurrentMediaDuration { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string RecordStorageMedium { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string PossiblePlaybackStorageMedia { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string PossibleRecordStorageMedia { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string RecordMediumWriteStatus { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string CurrentRecordQualityMode { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string PossibleRecordQualityModes { get; set; } = string.Empty;
        #endregion AvTransport
        #region AlarmClock
        /// <summary>
        /// Datumsformat: YMD / MDY / DMY
        /// </summary>
        [IgnoreDataMember]
        public string DateFormat { get; set; } = string.Empty;
        /// <summary>
        /// Zeitformat 12H oder 24H
        /// </summary>
        [IgnoreDataMember]
        public string TimeFormat { get; set; } = string.Empty;
        /// <summary>
        /// Unbekannt
        /// </summary>
        [IgnoreDataMember]
        public int TimeGeneration { get; set; } = 0;
        /// <summary>
        /// Wenn leer wird die Zeit Manuell gestellt ansonsten vom Server geholt.
        /// </summary>
        [IgnoreDataMember]
        public string TimeServer { get; set; } = string.Empty;
        /// <summary>
        /// InternalString der SonosTimeZone Klasse
        /// </summary>
        [IgnoreDataMember]
        public string TimeZone { get; set; } = string.Empty;
        /// <summary>
        /// Index AKtualisierungszeitpunkt im Format HH:MM:SS
        /// </summary>
        [IgnoreDataMember]
        public string DailyIndexRefreshTime { get; set; } = string.Empty;
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
        public string AudioInput_Name { get; set; } = string.Empty;
        /// <summary>
        /// Bild das in der Oberfläche angezeigt werden soll. 
        /// Bisher bekannt: HomeTheater und AudioComponent
        /// </summary>
        public string AudioInput_Icon { get; set; } = string.Empty;
        /// <summary>
        /// Vermutlich Lautstärke Werte zwischen 1 und 12 erhalten
        /// </summary>
        public int AudioInput_LeftLineInLevel { get; set; } = 0;
        /// <summary>
        /// Prüft ob ein AudioIn angeschlossen ist. 
        /// </summary>
        public bool AudioInput_LineInConnected { get; set; } = false;
        /// <summary>
        /// Bisher unbekannt wird scheinbar nicht genutzt.
        /// </summary>
        [IgnoreDataMember]
        public string AudioInput_Playing { get; set; } = string.Empty;
        /// <summary>
        /// Vermutlich Lautstärke Werte zwischen 1 und 12 erhalten
        /// </summary>
        public int AudioInput_RightLineInLevel { get; set; } = 0;
        #endregion AudioIn
        #region DeviceProperties
        public bool DeviceProperties_AirPlayEnabled { get; set; } = false;
        [IgnoreDataMember]
        public string DeviceProperties_AvailableRoomCalibration { get; set; } = string.Empty;
        public bool DeviceProperties_BehindWifiExtender { get; set; } = false;
        [IgnoreDataMember]
        public int DeviceProperties_ChannelFreq { get; set; } = 2462;
        public string DeviceProperties_ChannelMapSet { get; set; } = string.Empty;
        public string DeviceProperties_ConfigMode { get; set; } = string.Empty;

        public string DeviceProperties_Configuration { get; set; } = "1";
        public bool DeviceProperties_HasConfiguredSSID { get; set; } = true;
        public string DeviceProperties_HdmiCecAvailable { get; set; } = "0";
        public int DeviceProperties_HTBondedZoneCommitState { get; set; } = 0;
        public string DeviceProperties_HTFreq { get; set; } = string.Empty;
        public string DeviceProperties_HTSatChanMapSet { get; set; } = string.Empty;
        public string DeviceProperties_Icon { get; set; } = string.Empty;
        public bool DeviceProperties_Invisible { get; set; } = false;
        public bool DeviceProperties_IsIdle { get; set; } = true;
        public bool DeviceProperties_IsZoneBridge { get; set; } = false;
        [IgnoreDataMember]
        public string DeviceProperties_LastChangedPlayState { get; set; } = string.Empty;
        [IgnoreDataMember]
        public int DeviceProperties_Orientation { get; set; } = 0;
        public string DeviceProperties_RoomCalibrationState { get; set; } = "1";
        [IgnoreDataMember]
        public int DeviceProperties_SecureRegState { get; set; } = 3;
        [IgnoreDataMember]
        public string DeviceProperties_SettingsReplicationState { get; set; } = string.Empty;
        public bool DeviceProperties_SupportsAudioIn { get; set; } = false;
        public string DeviceProperties_TVConfigurationError { get; set; } = "0";
        public string DeviceProperties_VoiceControlState { get; set; } = "0";
        public bool DeviceProperties_WifiEnabled { get; set; } = false;
        public bool DeviceProperties_WirelessLeafOnly { get; set; } = false;
        public int DeviceProperties_WirelessMode { get; set; } = 0;
        public string DeviceProperties_ZoneName { get; set; } = string.Empty;
        #endregion DeviceProperties
        #region GroupManagement
        /// <summary>
        /// Liefert zurück ob es der Zonecoordinator ist.
        /// </summary>
        public bool GroupCoordinatorIsLocal { get; set; } = true;
        /// <summary>
        /// Definiert den Coordinator der Gruppe
        /// </summary>
        public string LocalGroupUUID { get; set; } = string.Empty;
        public string GroupManagement_ResetVolumeAfter { get; set; } = string.Empty;

        public string GroupManagement_VirtualLineInGroupID { get; set; } = string.Empty;
        public string GroupManagement_VolumeAVTransportURI { get; set; } = string.Empty;
        #endregion GroupManagement
        #region Systemproperties
        public string SystemProperties_CustomerID { get; set; } = string.Empty;
        public string SystemProperties_ThirdPartyHash { get; set; } = string.Empty;
        public string SystemProperties_UpdateID { get; set; } = string.Empty;
        public string SystemProperties_UpdateIDX { get; set; } = string.Empty;
        public string SystemProperties_VoiceUpdateID { get; set; } = string.Empty;
        public string MusicServices_ServiceListVersion { get; set; } = string.Empty;
        #endregion Systemproperties
        #region ZoneGroupTopology
        public string ZoneGroupTopology_AlarmRunSequence { get; set; } = string.Empty;
        public string ZoneGroupTopology_ZoneGroupID { get; set; } = string.Empty;
        public string ZoneGroupTopology_ZoneGroupName { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string ZoneGroupTopology_AvailableSoftwareUpdate { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string ZoneGroupTopology_MuseHouseholdId { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string ZoneGroupTopology_ThirdPartyMediaServersX { get; set; } = string.Empty;
        public List<string> ZoneGroupTopology_ZonePlayerUUIDsInGroup { get; set; } = new List<string>();
        [IgnoreDataMember]
        internal string ZoneGroupTopology_ZonePlayerUUIDsInGroupAsString
        {
            get
            {

                return string.Join(",", ZoneGroupTopology_ZonePlayerUUIDsInGroup);
            }
        }
        #endregion ZoneGroupTopology
        #region Mediarenderer Connectionmanager
        [IgnoreDataMember]
        public string MR_ConnectionManager_CurrentConnectionIDs { get; set; } = string.Empty;
        [IgnoreDataMember]
        public List<string> MR_ConnectionManager_SinkProtocolInfo { get; set; }
        [IgnoreDataMember]
        public string MR_ConnectionManager_SourceProtocolInfo { get; set; } = string.Empty;
        #endregion Mediarenderer Connectionmanager
        #region MediaServer Connectionmanager
        [IgnoreDataMember]
        public string MS_ConnectionManager_CurrentConnectionIDs { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string MS_ConnectionManager_SinkProtocolInfo { get; set; } = string.Empty;
        [IgnoreDataMember]
        public List<string> MS_ConnectionManager_SourceProtocolInfo { get; set; }
        #endregion MediaServer Connectionmanager
        #region GroupRenderingControl
        public bool GroupRenderingControl_GroupMute { get; set; } = false;
        public int GroupRenderingControl_GroupVolume { get; set; } = 0;
        public bool GroupRenderingControl_GroupVolumeChangeable { get; set; } = true;
        #endregion GroupRenderingControl
        #region ContentDirectory
        //Wird nicht ausgegeben und auch nicht abnoiert, da dies in der Discovery gemacht wird.
        [IgnoreDataMember]
        public string ContentDirectory_Browseable { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string ContentDirectory_ContainerUpdateIDs { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string ContentDirectory_FavoritePresetsUpdateID { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string ContentDirectory_FavoritesUpdateID { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string ContentDirectory_RadioFavoritesUpdateID { get; set; } = string.Empty;
        [IgnoreDataMember]
        public int ContentDirectory_RadioLocationUpdateID { get; set; } = 0;
        [IgnoreDataMember]
        public string ContentDirectory_RecentlyPlayedUpdateID { get; set; } = string.Empty;
        [IgnoreDataMember]
        public int ContentDirectory_SavedQueuesUpdateID { get; set; } = 0;
        /// <summary>
        /// Gibt an ob der Musikindex gerade aktualisiert wird.
        /// </summary>
        [IgnoreDataMember]
        public bool ContentDirectory_ShareIndexInProgress { get; set; } = false;
        [IgnoreDataMember]
        public string ContentDirectory_ShareIndexLastError { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string ContentDirectory_ShareListUpdateID { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string ContentDirectory_SystemUpdateID { get; set; } = string.Empty;
        [IgnoreDataMember]
        public string ContentDirectory_UserRadioUpdateID { get; set; } = string.Empty;
        #endregion ContentDirectory
        #region Eigene Eigenschaften
        /// <summary>
        /// Das Icon des Playertyps
        /// </summary>
        public string Icon { get; internal set; } = string.Empty;
        /// <summary>
        /// Pfadangaben des Sonos Geräts (ohne XML)
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;
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
        public string RemainingSleepTimerDuration { get; set; } = SonosConstants.Off;

        /// <summary>
        /// Stummschaltung
        /// </summary>
        public bool Mute { get; set; } = false;
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
