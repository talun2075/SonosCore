namespace SonosData.DataClasses
{
    /// <summary>
    /// The Sonos used Enum Class
    /// </summary>
    public static class SonosEnums
    {
        /// <summary>
        /// All UPNP Services 
        /// </summary>
        public enum Services
        {
            AlarmClock,
            AudioIn,
            DeviceProperties,
            GroupManagement,
            MusicServices,
            QPlay,
            SystemProperties,
            ZoneGroupTopology,
            ConnectionManager_MS,
            ConnectionManager_MR,
            ContentDirectory,
            RenderingControl,
            Queue,
            GroupRenderingControl,
            AVTransport
        }
        /// <summary>
        /// Policy XML Element Auth Atributte
        /// </summary>
        public enum PolicyAuth
        {
            AppLink,
            DeviceLink,
            UserID,
            Anonymous
        }
        /// <summary>
        /// ContainerType Attribute
        /// </summary>
        public enum ContainerTypes
        {
            SoundLab,
            MService
        }
        /// <summary>
        /// Browse Art Definieren
        /// </summary>
        public enum BrowseFlagData
        {
            /// <summary>
            /// Auslesen der Kindselemente
            /// </summary>
            BrowseDirectChildren,
            /// <summary>
            /// Auslesen der MetaDaten
            /// </summary>
            BrowseMetadata
        }
        /// <summary>
        /// Enums für das Eventing
        /// </summary>
        public enum EventingEnums
        {
            /// <summary>
            /// Kann die Gruppenlautstärke geändert werden
            /// </summary>
            GroupVolumeChangeable,
            /// <summary>
            /// Gruppenlautstärke
            /// </summary>
            GroupVolume,
            /// <summary>
            /// Gruppenstummschaltung
            /// </summary>
            GroupMute,
            /// <summary>
            /// Lautstärke des Players
            /// </summary>
            Volume,
            /// <summary>
            /// Stummschlatung des Players
            /// </summary>
            Mute,
            /// <summary>
            /// Bass Einstellungen des Players
            /// </summary>
            Bass,
            /// <summary>
            /// Treble des Players
            /// </summary>
            Treble,
            Loudness,
            OutputFixed,
            SupportOutputFixed,
            HeadphoneConnected,
            PresetNameList,
            SonarCalibrationAvailable,
            SonarEnabled,
            SubEnabled,
            SubPolarity,
            SubCrossover,
            SubGain,
            SpeakerSize,
            /// <summary>
            /// Wird genutzt, wenn sich die Playlist geändert hat.
            /// </summary>
            QueueChanged,
            QueueChangedNoRefillNeeded,
            QueueChangedEmpty,
            QueueChangedSaved,
            QueueChangeResort,
            /// <summary>
            /// Wird genutzt, wenn der Sleeptimer Status sich ändert 
            /// </summary>
            SleepTimerRunning,
            /// <summary>
            /// Die Dauer des RemainingSleepTimer hat sich geändert
            /// </summary>
            RemainingSleepTimerDuration,
            RatingFilter,
            /// <summary>
            /// Crossfade Modus
            /// </summary>
            CurrentCrossFadeMode,
            /// <summary>
            /// Wiedergabearten wie Pause und Play
            /// </summary>
            TransportState,
            /// <summary>
            /// Shuffle, Repeat etc.
            /// </summary>
            CurrentPlayMode,
            /// <summary>
            /// Anzahl der Lieder in Playlist
            /// </summary>
            NumberOfTracks,
            /// <summary>
            /// Song in der Playlist
            /// </summary>
            CurrentTrackNumber,
            CurrentSection,
            PlaybackStorageMedium,
            AVTransportURI,
            AVTransportURIMetaData,
            NextAVTransportURI,
            NextAVTransportURIMetaData,
            CurrentTransportActions,
            TransportPlaySpeed,
            CurrentMediaDuration,
            RecordStorageMedium,
            PossiblePlaybackStorageMedia,
            PossibleRecordStorageMedia,
            RecordMediumWriteStatus,
            CurrentRecordQualityMode,
            PossibleRecordQualityModes,
            EnqueuedTransportURI,
            EnqueuedTransportURIMetaData,
            CurrentValidPlayModes,
            MuseSessions,
            DirectControlClientID,
            DirectControlIsSuspended,
            DirectControlAccountID,
            AlarmRunning,
            SnoozeRunning,
            RestartPending,
            /// <summary>
            /// NextTRack ist anders
            /// </summary>
            NextTrack,
            /// <summary>
            /// CurrentTrack ist anders
            /// </summary>
            CurrentTrack,
            LineInConnected,
            AudioIn_Playing,
            RightLineInLevel,
            LeftLineInLevel,
            AudioIn_Icon,
            AudioInputName,
            AirPlayEnabled,
            DeviceProperties_Icon,
            AvailableRoomCalibration,
            BehindWifiExtender,
            ChannelFreq,
            ChannelMapSet,
            ConfigMode,
            DeviceProperties_Configuration,
            HasConfiguredSSID,
            HdmiCecAvailable,
            HTBondedZoneCommitState,
            HTFreq,
            HTSatChanMapSet,
            Invisible,
            IsIdle,
            IsZoneBridge,
            LastChangedPlayState,
            Orientation,
            RoomCalibrationState,
            SecureRegState,
            SupportsAudioIn,
            SettingsReplicationState,
            TVConfigurationError,
            VoiceControlState,
            WifiEnabled,
            WirelessLeafOnly,
            WirelessMode,
            ZoneName,
            VolumeAVTransportURI,
            VirtualLineInGroupID,
            ResetVolumeAfter,
            /// <summary>
            /// ID der ZONE
            /// </summary>
            LocalGroupUUID,
            /// <summary>
            /// Ist der Player der Coordinator der Gruppe
            /// </summary>
            GroupCoordinatorIsLocal,
            ThirdPartyMediaServersX,
            MuseHouseholdId,
            AvailableSoftwareUpdate,
            ZonePlayerUUIDsInGroup,
            ZoneGroupName,
            ZoneGroupID,
            AlarmRunSequence,
            ShareListUpdateID,
            FavoritesUpdateID,
            ShareIndexLastError,
            ShareIndexInProgress,
            SavedQueuesUpdateID,
            TimeZone,
            TimeServer,
            TimeGeneration,
            TimeFormat,
            DateFormat,
            DailyIndexRefreshTime,
            AlarmListVersion,
            /// <summary>
            /// Abgespielte Dauer eines Songs
            /// </summary>
            RelTime,
            /// <summary>
            /// Event von MusicService
            /// </summary>
            MusicServiceListVersion,
            /// <summary>
            /// SystemProperties Event
            /// </summary>
            VoiceUpdateID,
            /// <summary>
            /// SystemProperties Event
            /// </summary>
            UpdateIDX,
            /// <summary>
            /// SystemProperties Event
            /// </summary>
            UpdateID,
            /// <summary>
            /// SystemProperties Event
            /// </summary>
            ThirdPartyHash,
            /// <summary>
            /// SystemProperties Event
            /// </summary>
            CustomerID,
            /// <summary>
            /// ContentDirectory Event
            /// </summary>
            Browseable,
            /// <summary>
            /// ContentDirectory Event
            /// </summary>
            UserRadioUpdateID,
            /// <summary>
            /// ContentDirectory Event
            /// </summary>
            SystemUpdateID,
            /// <summary>
            /// ContentDirectory Event
            /// </summary>
            RecentlyPlayedUpdateID,
            /// <summary>
            /// ContentDirectory Event
            /// </summary>
            RadioLocationUpdateID,
            /// <summary>
            /// ContentDirectory Event
            /// </summary>
            RadioFavoritesUpdateID,
            /// <summary>
            /// ContentDirectory Event
            /// </summary>
            FavoritePresetsUpdateID,
            /// <summary>
            /// ContentDirectory Event
            /// </summary>
            ContainerUpdateIDs,
            CurrentTrackUri,
            ReloadNeeded

        }
        /// <summary>
        /// An Aus Enum
        /// </summary>
        public enum OnOff
        {
            On,
            Off
        }
        /// <summary>
        /// Mögliche abspiel Quellen
        /// </summary>
        public enum PlaybackStorageMedium
        {
            NONE, NETWORK
        }
        /// <summary>
        /// Mögliche Abspiel arten
        /// </summary>
        public enum PlayModes
        {
            NORMAL,
            REPEAT_ALL,
            SHUFFLE_NOREPEAT,
            SHUFFLE,
            REPEAT_ONE,
            SHUFFLE_REPEAT_ONE,
            UNKNOWING
        }
        public enum RampTypes
        {
            SLEEP_TIMER_RAMP_TYPE,
            ALARM_RAMP_TYPE,
            AUTOPLAY_RAMP_TYPE
        }
        /// <summary>
        /// Typ der Positionierung
        /// </summary>
        public enum SeekUnit
        {
            TRACK_NR,
            REL_TIME,
            TIME_DELTA
        }
        /// <summary>
        /// Auswahl des Lautsprechers (Master bei Single, Links und Rechts bei Stereo)
        /// </summary>
        public enum SpeakerSelection
        {
            /// <summary>
            /// Beide
            /// </summary>
            Master,
            /// <summary>
            /// Links
            /// </summary>
            LF,
            /// <summary>
            /// Rechts
            /// </summary>
            RF,
            /// <summary>
            /// Scheint wie Master zu sein
            /// </summary>
            SpeakerOnly
        }
        /// <summary>
        /// Wiedergabestatus
        /// </summary>
        public enum TransportState
        {
            STOPPED, PLAYING, PAUSED_PLAYBACK, TRANSITIONING, UNKNOWING
        }
        public enum UnresponsiveDeviceAction
        {
            Remove,
            TopologyMonitorProbe,
            VerifyThenRemoveSystemwide
        }
    }
}
