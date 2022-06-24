using OSTL.UPnP;
using SonosUPnP.Classes;
using SonosUPnP.DataClasses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SonosUPnP.Services
{
    public class DeviceProperties
    {
        #region Klassenvariablen
        public UPnPStateVariable AirPlayEnabled { get; set; }
        public UPnPStateVariable AvailableRoomCalibration { get; set; }
        public UPnPStateVariable BehindWifiExtender { get; set; }
        public UPnPStateVariable ChannelFreq { get; set; }
        public UPnPStateVariable ChannelMapSet { get; set; }
        public UPnPStateVariable ConfigMode { get; set; }

        public UPnPStateVariable Configuration { get; set; }
        public UPnPStateVariable HasConfiguredSSID { get; set; }
        public UPnPStateVariable HdmiCecAvailable { get; set; }
        public UPnPStateVariable HTBondedZoneCommitState { get; set; }
        public UPnPStateVariable HTFreq { get; set; }
        public UPnPStateVariable HTSatChanMapSet { get; set; }
        public UPnPStateVariable Icon { get; set; }

        public UPnPStateVariable Invisible { get; set; }
        public UPnPStateVariable IsIdle { get; set; }
        public UPnPStateVariable IsZoneBridge { get; set; }
        public UPnPStateVariable LastChangedPlayState { get; set; }

        public UPnPStateVariable Orientation { get; set; }
        public UPnPStateVariable RoomCalibrationState { get; set; }
        public UPnPStateVariable SecureRegState { get; set; }
        public UPnPStateVariable SettingsReplicationState { get; set; }
        public UPnPStateVariable SupportsAudioIn { get; set; }
        public UPnPStateVariable TVConfigurationError { get; set; }

        public UPnPStateVariable VoiceControlState { get; set; }
        public UPnPStateVariable WifiEnabled { get; set; }
        public UPnPStateVariable WirelessLeafOnly { get; set; }
        public UPnPStateVariable WirelessMode { get; set; }
        public UPnPStateVariable ZoneName { get; set; }
        public event EventHandler<SonosPlayer> DeviceProperties_Changed = delegate { };
        private readonly Dictionary<SonosEnums.EventingEnums, DateTime> LastChangeDates = new();
        private UPnPService deviceproperty;
        private readonly SonosPlayer pl;
        public DateTime LastChangeByEvent { get; private set; }
        #endregion Klassenvariablen
        #region ctor und Service
        public DeviceProperties(SonosPlayer sp)
        {
            pl = sp;
            LastChangeDates.Add(SonosEnums.EventingEnums.AirPlayEnabled, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.DeviceProperties_Icon, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.AvailableRoomCalibration, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.BehindWifiExtender, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.ChannelFreq, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.ChannelMapSet, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.ConfigMode, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.DeviceProperties_Configuration, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.HasConfiguredSSID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.HdmiCecAvailable, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.HTBondedZoneCommitState, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.HTFreq, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.HTSatChanMapSet, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.Invisible, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.IsIdle, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.IsZoneBridge, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.LastChangedPlayState, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.Orientation, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.RoomCalibrationState, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.SecureRegState, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.SupportsAudioIn, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.SettingsReplicationState, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.TVConfigurationError, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.VoiceControlState, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.WifiEnabled, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.WirelessLeafOnly, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.WirelessMode, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.ZoneName, new DateTime());
        }

        /// <summary>
        /// Liefert den Devicepropertie zurück (UPNP)
        /// </summary>
        public UPnPService DevicePropertiesService
        {
            get
            {
                if (deviceproperty != null)
                    return deviceproperty;
                if (pl.Device == null)
                {
                    pl.LoadDevice();
                    pl.LoadDevice();
                    if (pl.Device == null)
                        return null;
                }
                deviceproperty = pl.Device.GetService("urn:upnp-org:serviceId:DeviceProperties");
                return deviceproperty;
            }
        }
        #endregion ctor und Service
        #region Eventing
        public void SubscripeToEvents()
        {
            if (DevicePropertiesService == null) return;
            DevicePropertiesService.Subscribe(600, (service, subscribeok) =>
            {
                if (!subscribeok)
                    return;

                AirPlayEnabled = service.GetStateVariableObject("AirPlayEnabled");
                AirPlayEnabled.OnModified += EventFired_AirPlayEnabled;
                Icon = service.GetStateVariableObject("Icon");
                Icon.OnModified += EventFired_Icon;
                AvailableRoomCalibration = service.GetStateVariableObject("AvailableRoomCalibration");
                AvailableRoomCalibration.OnModified += EventFired_AvailableRoomCalibration;
                BehindWifiExtender = service.GetStateVariableObject("BehindWifiExtender");
                BehindWifiExtender.OnModified += EventFired_BehindWifiExtender;
                ChannelFreq = service.GetStateVariableObject("ChannelFreq");
                ChannelFreq.OnModified += EventFired_ChannelFreq;
                ChannelMapSet = service.GetStateVariableObject("ChannelMapSet");
                ChannelMapSet.OnModified += EventFired_ChannelMapSet;
                ConfigMode = service.GetStateVariableObject("ConfigMode");
                ConfigMode.OnModified += EventFired_ConfigMode;
                Configuration = service.GetStateVariableObject("Configuration");
                Configuration.OnModified += EventFired_Configuration;
                HasConfiguredSSID = service.GetStateVariableObject("HasConfiguredSSID");
                HasConfiguredSSID.OnModified += EventFired_HasConfiguredSSID;
                HdmiCecAvailable = service.GetStateVariableObject("HdmiCecAvailable");
                HdmiCecAvailable.OnModified += EventFired_HdmiCecAvailable;
                HTBondedZoneCommitState = service.GetStateVariableObject("HTBondedZoneCommitState");
                HTBondedZoneCommitState.OnModified += EventFired_HTBondedZoneCommitState;
                HTFreq = service.GetStateVariableObject("HTFreq");
                HTFreq.OnModified += EventFired_HTFreq;
                HTSatChanMapSet = service.GetStateVariableObject("HTSatChanMapSet");
                HTSatChanMapSet.OnModified += EventFired_HTSatChanMapSet;
                Invisible = service.GetStateVariableObject("Invisible");
                Invisible.OnModified += EventFired_Invisible;
                IsIdle = service.GetStateVariableObject("IsIdle");
                IsIdle.OnModified += EventFired_IsIdle;
                IsZoneBridge = service.GetStateVariableObject("IsZoneBridge");
                IsZoneBridge.OnModified += EventFired_IsZoneBridge;
                LastChangedPlayState = service.GetStateVariableObject("LastChangedPlayState");
                LastChangedPlayState.OnModified += EventFired_LastChangedPlayState;
                Orientation = service.GetStateVariableObject("Orientation");
                Orientation.OnModified += EventFired_Orientation;
                RoomCalibrationState = service.GetStateVariableObject("RoomCalibrationState");
                RoomCalibrationState.OnModified += EventFired_RoomCalibrationState;
                SecureRegState = service.GetStateVariableObject("SecureRegState");
                SecureRegState.OnModified += EventFired_SecureRegState;
                SupportsAudioIn = service.GetStateVariableObject("SupportsAudioIn");
                SupportsAudioIn.OnModified += EventFired_SupportsAudioIn;
                SettingsReplicationState = service.GetStateVariableObject("SettingsReplicationState");
                SettingsReplicationState.OnModified += EventFired_SettingsReplicationState;
                TVConfigurationError = service.GetStateVariableObject("TVConfigurationError");
                TVConfigurationError.OnModified += EventFired_TVConfigurationError;
                VoiceControlState = service.GetStateVariableObject("VoiceConfigState");
                VoiceControlState.OnModified += EventFired_VoiceControlState;
                WifiEnabled = service.GetStateVariableObject("WifiEnabled");
                WifiEnabled.OnModified += EventFired_WifiEnabled;
                WirelessLeafOnly = service.GetStateVariableObject("WirelessLeafOnly");
                WirelessLeafOnly.OnModified += EventFired_WirelessLeafOnly;
                WirelessMode = service.GetStateVariableObject("WirelessMode");
                WirelessMode.OnModified += EventFired_WirelessMode;
                ZoneName = service.GetStateVariableObject("ZoneName");
                ZoneName.OnModified += EventFired_ZoneName;
            });
        }

        private void EventFired_ZoneName(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.DeviceProperties_ZoneName != nv)
            {
                pl.PlayerProperties.DeviceProperties_ZoneName = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.ZoneName].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.ZoneName] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.ZoneName, DateTime.Now);
            }
        }

        private void EventFired_WirelessMode(UPnPStateVariable sender, object NewValue)
        {
            
            if (int.TryParse(NewValue.ToString(), out int wm) && pl.PlayerProperties.DeviceProperties_WirelessMode != wm)
            {
                pl.PlayerProperties.DeviceProperties_WirelessMode = wm;

                if (LastChangeDates[SonosEnums.EventingEnums.WirelessMode].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.WirelessMode] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.WirelessMode, DateTime.Now);
            }
        }

        private void EventFired_WirelessLeafOnly(UPnPStateVariable sender, object NewValue)
        {
            if (Boolean.TryParse(NewValue.ToString(), out bool nv) && pl.PlayerProperties.DeviceProperties_WirelessLeafOnly != nv)
            {
                pl.PlayerProperties.DeviceProperties_WirelessLeafOnly = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.WirelessLeafOnly].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.WirelessLeafOnly] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.WirelessLeafOnly, DateTime.Now);
            }
        }

        private void EventFired_WifiEnabled(UPnPStateVariable sender, object NewValue)
        {
            if (Boolean.TryParse(NewValue.ToString(), out bool nv) && pl.PlayerProperties.DeviceProperties_WifiEnabled != nv)
            {
                pl.PlayerProperties.DeviceProperties_WifiEnabled = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.WifiEnabled].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.WifiEnabled] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.WifiEnabled, DateTime.Now);
            }
        }

        private void EventFired_VoiceControlState(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.DeviceProperties_VoiceControlState != nv)
            {
                pl.PlayerProperties.DeviceProperties_VoiceControlState = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.VoiceControlState].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.VoiceControlState] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.VoiceControlState, DateTime.Now);
            }
        }

        private void EventFired_TVConfigurationError(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.DeviceProperties_TVConfigurationError != nv)
            {
                pl.PlayerProperties.DeviceProperties_TVConfigurationError = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.TVConfigurationError].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.TVConfigurationError] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.TVConfigurationError, DateTime.Now);
            }
        }

        private void EventFired_SettingsReplicationState(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            pl.PlayerProperties.DeviceProperties_SettingsReplicationState = nv;
            if (pl.PlayerProperties.DeviceProperties_SettingsReplicationState != nv)
            {
                pl.PlayerProperties.DeviceProperties_SettingsReplicationState = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.SettingsReplicationState].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.SettingsReplicationState] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.SettingsReplicationState, DateTime.Now);
            }
        }

        private void EventFired_SupportsAudioIn(UPnPStateVariable sender, object NewValue)
        {
            
            if (Boolean.TryParse(NewValue.ToString(), out bool nv) && pl.PlayerProperties.DeviceProperties_SupportsAudioIn != nv)
            {
                pl.PlayerProperties.DeviceProperties_SupportsAudioIn = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.SupportsAudioIn].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.SupportsAudioIn] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.SupportsAudioIn, DateTime.Now);
            }
        }

        private void EventFired_SecureRegState(UPnPStateVariable sender, object NewValue)
        {
            
            if (int.TryParse(NewValue.ToString(), out int nv) && pl.PlayerProperties.DeviceProperties_SecureRegState != nv)
            {
                pl.PlayerProperties.DeviceProperties_SecureRegState = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.SecureRegState].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.SecureRegState] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.SecureRegState, DateTime.Now);
            }
        }

        private void EventFired_RoomCalibrationState(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.DeviceProperties_RoomCalibrationState != nv)
            {
                pl.PlayerProperties.DeviceProperties_RoomCalibrationState = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.RoomCalibrationState].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.RoomCalibrationState] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.RoomCalibrationState, DateTime.Now);
            }
        }

        private void EventFired_Orientation(UPnPStateVariable sender, object NewValue)
        {
            
            if (int.TryParse(NewValue.ToString(), out int nv) && pl.PlayerProperties.DeviceProperties_Orientation != nv)
            {
                pl.PlayerProperties.DeviceProperties_Orientation = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.Orientation].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.Orientation] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.Orientation, DateTime.Now);
            }
        }

        private void EventFired_LastChangedPlayState(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.DeviceProperties_LastChangedPlayState != nv)
            {
                pl.PlayerProperties.DeviceProperties_LastChangedPlayState = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.LastChangedPlayState].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.LastChangedPlayState] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.LastChangedPlayState, DateTime.Now);
            }
        }

        private void EventFired_IsZoneBridge(UPnPStateVariable sender, object NewValue)
        {
            
            if (Boolean.TryParse(NewValue.ToString(), out bool nv) && pl.PlayerProperties.DeviceProperties_IsZoneBridge != nv)
            {
                pl.PlayerProperties.DeviceProperties_IsZoneBridge = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.IsZoneBridge].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.IsZoneBridge] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.IsZoneBridge, DateTime.Now);
            }
        }

        private void EventFired_IsIdle(UPnPStateVariable sender, object NewValue)
        {
            
            if (Boolean.TryParse(NewValue.ToString(), out bool nv) && pl.PlayerProperties.DeviceProperties_IsIdle != nv)
            {
                pl.PlayerProperties.DeviceProperties_IsIdle = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.IsIdle].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.IsIdle] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.IsIdle, DateTime.Now);
            }
        }

        private void EventFired_Invisible(UPnPStateVariable sender, object NewValue)
        {
            
            if (Boolean.TryParse(NewValue.ToString(), out bool nv) && pl.PlayerProperties.DeviceProperties_Invisible != nv)
            {
                pl.PlayerProperties.DeviceProperties_Invisible = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.Invisible].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.Invisible] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.Invisible, DateTime.Now);
            }
        }

        private void EventFired_HTSatChanMapSet(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.DeviceProperties_HTSatChanMapSet != nv)
            {
                pl.PlayerProperties.DeviceProperties_HTSatChanMapSet = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.HTSatChanMapSet].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.HTSatChanMapSet] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.HTSatChanMapSet, DateTime.Now);
            }
        }

        private void EventFired_HTFreq(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.DeviceProperties_HTFreq != nv)
            {
                pl.PlayerProperties.DeviceProperties_HTFreq = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.HTFreq].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.HTFreq] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.HTFreq, DateTime.Now);
            }
        }

        private void EventFired_HTBondedZoneCommitState(UPnPStateVariable sender, object NewValue)
        {
            
            if (int.TryParse(NewValue.ToString(), out int nv) && pl.PlayerProperties.DeviceProperties_HTBondedZoneCommitState != nv)
            {
                pl.PlayerProperties.DeviceProperties_HTBondedZoneCommitState = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.HTBondedZoneCommitState].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.HTBondedZoneCommitState] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.HTBondedZoneCommitState, DateTime.Now);
            }
        }

        private void EventFired_HdmiCecAvailable(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.DeviceProperties_HdmiCecAvailable != nv)
            {
                pl.PlayerProperties.DeviceProperties_HdmiCecAvailable = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.HdmiCecAvailable].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.HdmiCecAvailable] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.HdmiCecAvailable, DateTime.Now);
            }
        }

        private void EventFired_HasConfiguredSSID(UPnPStateVariable sender, object NewValue)
        {
            
            if (Boolean.TryParse(NewValue.ToString(), out bool nv) && pl.PlayerProperties.DeviceProperties_HasConfiguredSSID != nv)
            {
                pl.PlayerProperties.DeviceProperties_HasConfiguredSSID = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.HasConfiguredSSID].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.HasConfiguredSSID] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.HasConfiguredSSID, DateTime.Now);
            }
        }

        private void EventFired_Configuration(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.DeviceProperties_Configuration != nv)
            {
                pl.PlayerProperties.DeviceProperties_Configuration = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.DeviceProperties_Configuration].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.DeviceProperties_Configuration] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.DeviceProperties_Configuration, DateTime.Now);
            }
        }

        private void EventFired_ConfigMode(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.DeviceProperties_ConfigMode != nv)
            {
                pl.PlayerProperties.DeviceProperties_ConfigMode = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.ConfigMode].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.ConfigMode] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.ConfigMode, DateTime.Now);
            }
        }

        private void EventFired_ChannelMapSet(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.DeviceProperties_ChannelMapSet != nv)
            {
                pl.PlayerProperties.DeviceProperties_ChannelMapSet = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.ChannelMapSet].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.ChannelMapSet] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.ChannelMapSet, DateTime.Now);
            }
        }

        private void EventFired_ChannelFreq(UPnPStateVariable sender, object NewValue)
        {
            
            if (int.TryParse(NewValue.ToString(), out int nv) && pl.PlayerProperties.DeviceProperties_ChannelFreq != nv)
            {
                pl.PlayerProperties.DeviceProperties_ChannelFreq = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.ChannelFreq].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.ChannelFreq] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.ChannelFreq, DateTime.Now);
            }
        }

        private void EventFired_BehindWifiExtender(UPnPStateVariable sender, object NewValue)
        {
            
            if (Boolean.TryParse(NewValue.ToString(), out bool nv) && pl.PlayerProperties.DeviceProperties_BehindWifiExtender != nv)
            {
                pl.PlayerProperties.DeviceProperties_BehindWifiExtender = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.BehindWifiExtender].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.BehindWifiExtender] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.BehindWifiExtender, DateTime.Now);
            }
        }

        private void EventFired_AvailableRoomCalibration(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.DeviceProperties_AvailableRoomCalibration != nv)
            {
                pl.PlayerProperties.DeviceProperties_AvailableRoomCalibration = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.AvailableRoomCalibration].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.AvailableRoomCalibration] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.AvailableRoomCalibration, DateTime.Now);
            }
        }

        private void EventFired_Icon(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.DeviceProperties_Icon != nv)
            {
                pl.PlayerProperties.DeviceProperties_Icon = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.DeviceProperties_Icon].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.DeviceProperties_Icon] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.DeviceProperties_Icon, DateTime.Now);
            }
        }

        private void EventFired_AirPlayEnabled(UPnPStateVariable sender, object NewValue)
        {
            if (Boolean.TryParse(NewValue.ToString(), out bool nv) && pl.PlayerProperties.DeviceProperties_AirPlayEnabled != nv)
            {
                pl.PlayerProperties.DeviceProperties_AirPlayEnabled = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.AirPlayEnabled].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.AirPlayEnabled] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.AirPlayEnabled, DateTime.Now);
            }
        }
        #endregion Eventing
        #region public Methoden
        public async Task<Boolean> AddBondedZones(string channelMapSet)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("ChannelMapSet", channelMapSet);
            return await Invoke("AddBondedZones", arguments);
        }
        public async Task<Boolean> AddHTSatellite(string hTSatChanMapSet)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("HTSatChanMapSet", hTSatChanMapSet);
            return await Invoke("AddHTSatellite", arguments);
        }
        public async Task<Boolean> CreateStereoPair(string channelMapSet)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("ChannelMapSet", channelMapSet);
            return await Invoke("CreateStereoPair", arguments);
        }
        public async Task<String> EnterConfigMode(string Mode, string Options)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("Mode", Mode);
            arguments[1] = new UPnPArgument("Options", Options);
            arguments[2] = new UPnPArgument("State", null);
            await Invoke("EnterConfigMode", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 2, 100, 10, WaiterTypes.String);
            return arguments[2].DataValue.ToString();

        }
        public async Task<Boolean> ExitConfigMode(string Options)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("Options", Options);
            return await Invoke("ExitConfigMode", arguments);
        }
        public async Task<Boolean> GetAutoplayLinkedZones(string Source)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("IncludeLinkedZones", null);
            arguments[1] = new UPnPArgument("Source", Source);
            await Invoke("GetAutoplayLinkedZones", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            Boolean.TryParse(arguments[0].DataValue.ToString(), out bool IncludeLinkedZones);
            return IncludeLinkedZones;
        }
        public async Task<string> GetAutoplayRoomUUID(string Source)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("RoomUUID", null);
            arguments[1] = new UPnPArgument("Source", Source);
            await Invoke("GetAutoplayRoomUUID", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 3, WaiterTypes.String);
            return arguments[0].DataValue.ToString();
        }
        public async Task<int> GetAutoplayVolume(string Source)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("CurrentVolume", null);
            arguments[1] = new UPnPArgument("Source", Source);
            await Invoke("GetAutoplayVolume", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            int.TryParse(arguments[0].DataValue.ToString(), out int vol);
            return vol;
        }
        public async Task<String> GetButtonLockState()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("CurrentButtonLockState", null);
            await Invoke("GetButtonLockState", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            return arguments[0].DataValue.ToString();
        }
        public async Task<String> GetButtonState()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("State", null);
            await Invoke("GetButtonLockState", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 2, WaiterTypes.String);
            return arguments[0].DataValue.ToString();
        }
        public async Task<String> GetHouseholdID()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("CurrentHouseholdID", null);
            await Invoke("GetHouseholdID", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            return arguments[0].DataValue.ToString();
        }
        /// <summary>
        /// Ermittelt ob die LED Lampe aktiv ist.
        /// </summary>
        /// <returns>On/Off or unknowing on exceptions</returns>
        public async Task<String> GetLEDState()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("CurrentLEDState", null);
            await Invoke("GetLEDState", arguments, 150);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            return arguments[0].DataValue.ToString();
        }
        public async Task<Boolean> GetUseAutoplayVolume(string Source)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("UseVolume", null);
            arguments[2] = new UPnPArgument("Source", Source);
            await Invoke("GetUseAutoplayVolume", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            Boolean.TryParse(arguments[0].DataValue.ToString(), out bool usevolume);
            return usevolume;
        }
        public async Task<ZoneAttributes> GetZoneAttributes()
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("CurrentZoneName", null);
            arguments[1] = new UPnPArgument("CurrentIcon", null);
            arguments[2] = new UPnPArgument("CurrentConfiguration", null);
            await Invoke("GetZoneAttributes", arguments, 10);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            return new ZoneAttributes
            {
                ZoneName = arguments[0].DataValue.ToString(),
                Configuration = arguments[2].DataValue.ToString(),
                Icon = arguments[1].DataValue.ToString()
            };
        }
        public async Task<ZoneInfo> GetZoneInfo()
        {
            var arguments = new UPnPArgument[10];
            arguments[0] = new UPnPArgument("SerialNumber", null);
            arguments[1] = new UPnPArgument("SoftwareVersion", null);
            arguments[2] = new UPnPArgument("DisplaySoftwareVersion", null);
            arguments[3] = new UPnPArgument("HardwareVersion", null);
            arguments[4] = new UPnPArgument("IPAddress", null);
            arguments[5] = new UPnPArgument("MACAddress", null);
            arguments[6] = new UPnPArgument("CopyrightInfo", null);
            arguments[7] = new UPnPArgument("ExtraInfo", null);
            arguments[8] = new UPnPArgument("HTAudioIn", null);
            arguments[9] = new UPnPArgument("Flags", null);
            await Invoke("GetZoneInfo", arguments, 20);
            await ServiceWaiter.WaitWhileAsync(arguments, 3, 100, 10, WaiterTypes.String);
            return new ZoneInfo
            {
                SerialNumber = arguments[0].DataValue.ToString(),
                SoftwareVersion = arguments[1].DataValue.ToString(),
                CopyrightInfo = arguments[6].DataValue.ToString(),
                DisplaySoftwareVersion = arguments[2].DataValue.ToString(),
                ExtraInfo = arguments[7].DataValue.ToString(),
                Flags = arguments[9].DataValue.ToString(),
                HardwareVersion = arguments[3].DataValue.ToString(),
                HTAudioIn = arguments[8].DataValue.ToString(),
                IPAddress = arguments[4].DataValue.ToString(),
                MACAddress = arguments[5].DataValue.ToString()
            };
        }
        public async Task<Boolean> ImportSetting(UInt16 SettingID, string SettingURI)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("SettingID", SettingID);
            arguments[1] = new UPnPArgument("SettingURI", SettingURI);
            return await Invoke("ImportSetting", arguments);
        }
        public async Task<Boolean> RemoveBondedZones(string ChannelMapSet)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("ChannelMapSet", ChannelMapSet);
            return await Invoke("RemoveBondedZones", arguments);
        }
        public async Task<Boolean> RemoveHTSatellite(string SatRoomUUID)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("SatRoomUUID", SatRoomUUID);
            return await Invoke("RemoveHTSatellite", arguments);
        }
        public async Task<Boolean> SeparateStereoPair(string ChannelMapSet)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("ChannelMapSet", ChannelMapSet);
            return await Invoke("SeparateStereoPair", arguments);
        }
        public async Task<Boolean> SetAutoplayLinkedZones(Boolean AutoplayIncludeLinkedZones, string Source)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("AutoplayIncludeLinkedZones", AutoplayIncludeLinkedZones);
            arguments[1] = new UPnPArgument("Source", Source);
            return await Invoke("SetAutoplayLinkedZones", arguments);
        }
        public async Task<Boolean> SetAutoplayRoomUUID(string RoomUUID, string Source)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("RoomUUID", RoomUUID);
            arguments[1] = new UPnPArgument("Source", Source);
            return await Invoke("SetAutoplayRoomUUID", arguments);
        }
        public async Task<Boolean> SetAutoplayVolume(UInt16 Volume, string Source)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("Volume", Volume);
            arguments[1] = new UPnPArgument("Source", Source);
            return await Invoke("SetAutoplayVolume", arguments);
        }
        public async Task<Boolean> SetButtonLockState(SonosEnums.OnOff DesiredButtonLockState = SonosEnums.OnOff.Off)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("DesiredButtonLockState", DesiredButtonLockState.ToString());
            return await Invoke("SetButtonLockState", arguments);
        }
        /// <summary>
        /// Schaltet die LED an und aus.
        /// </summary>
        public async Task<Boolean> SetLEDState(SonosEnums.OnOff DesiredLEDState = SonosEnums.OnOff.Off)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("DesiredLEDState", DesiredLEDState.ToString());
            return await Invoke("SetLEDState", arguments);
        }
        public async Task<Boolean> SetUseAutoplayVolume(Boolean UseVolume, String Source)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("UseVolume", UseVolume);
            arguments[1] = new UPnPArgument("Source", Source);
            return await Invoke("SetUseAutoplayVolume", arguments);
        }
        public async Task<Boolean> SetZoneAttributes(ZoneAttributes za)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("DesiredZoneName", za.ZoneName);
            arguments[1] = new UPnPArgument("DesiredIcon", za.Icon);
            arguments[2] = new UPnPArgument("DesiredConfiguration", za.Configuration);
            return await Invoke("SetZoneAttributes", arguments);
        }
        #endregion public Methoden
        #region private Methoden
        private async Task<Boolean> Invoke(String Method, UPnPArgument[] arguments, int Sleep = 0)
        {
            try
            {
                if (DevicePropertiesService == null)
                {
                    pl.ServerErrorsAdd(Method, "DeviceProperties", new Exception(Method + " AlarmClock ist null"));
                    return false;
                }
                DevicePropertiesService.InvokeAsync(Method, arguments);
                await Task.Delay(Sleep);
                return true;
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd(Method, "DeviceProperties", ex);
                return false;
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
                if (DeviceProperties_Changed == null) return;
                LastChangeDates[t] = _lastchange;
                LastChangeByEvent = _lastchange;
                DeviceProperties_Changed(t, pl);
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("DeviceProperties_Changed", "DeviceProperties", ex);
            }
        }
        #endregion public Methoden
    }
}

