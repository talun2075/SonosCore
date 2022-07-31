using OSTL.UPnP;
using SonosData.DataClasses;
using SonosUPnP.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SonosUPnP.Services.MediaRendererService
{
    public class RenderingControl
    {
        #region Klassenvariablen
        private const string ClassName = "RenderingControl";
        private UPnPService renderingControl;
        private readonly SonosPlayer pl;
        private UPnPDevice mediaRendererService;
        public UPnPStateVariable LastChange { get; set; }
        public event EventHandler<SonosPlayer> RenderingControl_Changed = delegate { };
        private readonly Dictionary<SonosEnums.EventingEnums, DateTime> LastChangeDates = new();
        public DateTime LastChangeByEvent { get; private set; }
        #endregion Klassenvariablen
        #region ctor und Service
        public RenderingControl(SonosPlayer sp)
        {
            pl = sp;
            LastChangeDates.Add(SonosEnums.EventingEnums.Volume, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.Mute, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.Bass, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.Treble, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.Loudness, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.OutputFixed, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.HeadphoneConnected, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.PresetNameList, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.SonarCalibrationAvailable, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.SonarEnabled, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.SubEnabled, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.SubPolarity, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.SubCrossover, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.SubGain, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.SpeakerSize, new DateTime());
        }

        /// <summary>
        /// Liefert den RenderingControl zurück
        /// </summary>
        public UPnPService RenderingControlService
        {
            get
            {
                if (renderingControl != null)
                    return renderingControl;
                if (mediaRendererService == null)
                    if (pl.Device == null)
                    {
                        pl.LoadDevice();
                        if (pl.Device == null)
                            return null;
                    }
                mediaRendererService = pl.Device.EmbeddedDevices.FirstOrDefault(d => d.DeviceURN == "urn:schemas-upnp-org:device:MediaRenderer:1");
                if (mediaRendererService == null)
                    return null;
                renderingControl = mediaRendererService.GetService("urn:upnp-org:serviceId:RenderingControl");
                return renderingControl;
            }
        }
        #endregion ctor und Service
        #region Eventing
        public void SubscripeToEvents()
        {
            RenderingControlService.Subscribe(600, (service, subscribeok) =>
            {
                if (!subscribeok)
                    return;

                LastChange = service.GetStateVariableObject("LastChange");
                LastChange.OnModified += EventFired_LastChange;
            });
        }
        private void EventFired_LastChange(UPnPStateVariable sender, object NewValue)
        {
            string newState = sender.Value.ToString();
            var xEvent = XElement.Parse(newState);
            XNamespace ns = "urn:schemas-upnp-org:metadata-1-0/RCS/";
            XElement instance = xEvent.Element(ns + "InstanceID");
            if (instance == null) return;
            //volume
            XElement vol = instance.Element(ns + "Volume");
            if (vol != null && vol.Attribute("channel").Value == "Master")
            {
                var tvol = Convert.ToInt16(vol.Attribute("val").Value);
                if (tvol != pl.PlayerProperties.Volume)
                {
                    pl.PlayerProperties.Volume = tvol;
                    if (LastChangeDates[SonosEnums.EventingEnums.Volume].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.Volume] = DateTime.Now;
                        LastChangeByEvent = DateTime.Now;
                        return;
                    }
                    ManuellStateChange(SonosEnums.EventingEnums.Volume, DateTime.Now);
                }
            }
            //Mute
            XElement mutElement = instance.Element(ns + "Mute");
            if (mutElement != null && mutElement.Attribute("channel").Value == "Master")
            {
                var tmute = (mutElement.Attribute("val").Value == "1");
                if (pl.PlayerProperties.Mute != tmute)
                {
                    pl.PlayerProperties.Mute = tmute;
                    if (LastChangeDates[SonosEnums.EventingEnums.Mute].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.Mute] = DateTime.Now;
                        LastChangeByEvent = DateTime.Now;
                        return;
                    }
                    ManuellStateChange(SonosEnums.EventingEnums.Mute, DateTime.Now);
                }
            }
            XElement basElement = instance.Element(ns + "Bass");
            if (basElement != null)
            {
                if (int.TryParse(basElement.Attribute("val").Value, out int bint) && pl.PlayerProperties.Bass != bint)
                {
                    pl.PlayerProperties.Bass = bint;
                    if (LastChangeDates[SonosEnums.EventingEnums.Bass].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.Bass] = DateTime.Now;
                        LastChangeByEvent = DateTime.Now;
                        return;
                    }
                    ManuellStateChange(SonosEnums.EventingEnums.Bass, DateTime.Now);
                }
            }
            XElement trebleElement = instance.Element(ns + "Treble");
            if (trebleElement != null)
            {
                if (int.TryParse(trebleElement.Attribute("val").Value, out int trebleint) && pl.PlayerProperties.Treble != trebleint)
                {
                    pl.PlayerProperties.Treble = trebleint;
                    if (LastChangeDates[SonosEnums.EventingEnums.Treble].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.Treble] = DateTime.Now;
                        LastChangeByEvent = DateTime.Now;
                        return;
                    }
                    ManuellStateChange(SonosEnums.EventingEnums.Treble, DateTime.Now);
                }
            }
            XElement loudnessElement = instance.Element(ns + "Loudness");
            if (loudnessElement != null)
            {
                if (bool.TryParse(loudnessElement.Attribute("val").Value, out bool loudness) && pl.PlayerProperties.Loudness != loudness)
                {
                    pl.PlayerProperties.Loudness = loudness;
                    if (LastChangeDates[SonosEnums.EventingEnums.Loudness].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.Loudness] = DateTime.Now;
                        LastChangeByEvent = DateTime.Now;
                        return;
                    }
                    ManuellStateChange(SonosEnums.EventingEnums.Loudness, DateTime.Now);
                }
            }
            XElement outputFixedeElement = instance.Element(ns + "OutputFixed");
            if (outputFixedeElement != null)
            {
                if (bool.TryParse(outputFixedeElement.Attribute("val").Value, out bool outputFixed) && pl.PlayerProperties.OutputFixed != outputFixed)
                {
                    pl.PlayerProperties.OutputFixed = outputFixed;
                    if (LastChangeDates[SonosEnums.EventingEnums.OutputFixed].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.OutputFixed] = DateTime.Now;
                        LastChangeByEvent = DateTime.Now;
                        return;
                    }
                    ManuellStateChange(SonosEnums.EventingEnums.OutputFixed, DateTime.Now);
                }
            }
            XElement headphoneConnectedeElement = instance.Element(ns + "HeadphoneConnected");
            if (headphoneConnectedeElement != null)
            {
                if (bool.TryParse(headphoneConnectedeElement.Attribute("val").Value, out bool headphoneConnected) && pl.PlayerProperties.HeadphoneConnected != headphoneConnected)
                {
                    pl.PlayerProperties.HeadphoneConnected = headphoneConnected;
                    if (LastChangeDates[SonosEnums.EventingEnums.HeadphoneConnected].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.HeadphoneConnected] = DateTime.Now;
                        LastChangeByEvent = DateTime.Now;
                        return;
                    }
                    ManuellStateChange(SonosEnums.EventingEnums.HeadphoneConnected, DateTime.Now);
                }
            }
            XElement presetNameListElemtElement = instance.Element(ns + "PresetNameList");
            if (presetNameListElemtElement != null)
            {
                var presetNameList = presetNameListElemtElement.Attribute("val").Value;
                if (pl.PlayerProperties.PresetNameList != presetNameList)
                {
                    pl.PlayerProperties.PresetNameList = presetNameList;
                    if (LastChangeDates[SonosEnums.EventingEnums.PresetNameList].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.PresetNameList] = DateTime.Now;
                        LastChangeByEvent = DateTime.Now;
                        return;
                    }
                    ManuellStateChange(SonosEnums.EventingEnums.PresetNameList, DateTime.Now);
                }
            }
            XElement SonarCalibrationAvailableElemtElement = instance.Element(ns + "SonarCalibrationAvailable");
            if (SonarCalibrationAvailableElemtElement != null)
            {
                if (bool.TryParse(SonarCalibrationAvailableElemtElement.Attribute("val").Value, out bool SonarCalibrationAvailable) && pl.PlayerProperties.SonarCalibrationAvailable != SonarCalibrationAvailable)
                {
                    pl.PlayerProperties.SonarCalibrationAvailable = SonarCalibrationAvailable;
                    if (LastChangeDates[SonosEnums.EventingEnums.SonarCalibrationAvailable].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.SonarCalibrationAvailable] = DateTime.Now;
                        LastChangeByEvent = DateTime.Now;
                        return;
                    }
                    ManuellStateChange(SonosEnums.EventingEnums.SonarCalibrationAvailable, DateTime.Now);
                }
            }
            XElement SonarEnabledElemtElement = instance.Element(ns + "SonarEnabled");
            if (SonarEnabledElemtElement != null)
            {
                var SonarEnabled = SonarEnabledElemtElement.Attribute("val").Value;
                if (pl.PlayerProperties.SonarEnabled != SonarEnabled)
                {
                    pl.PlayerProperties.SonarEnabled = SonarEnabled;
                    if (LastChangeDates[SonosEnums.EventingEnums.SonarEnabled].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.SonarEnabled] = DateTime.Now;
                        LastChangeByEvent = DateTime.Now;
                        return;
                    }
                    ManuellStateChange(SonosEnums.EventingEnums.SonarEnabled, DateTime.Now);
                }
            }
            XElement SubEnabledElemtElement = instance.Element(ns + "SubEnabled");
            if (SubEnabledElemtElement != null)
            {
                if (bool.TryParse(SubEnabledElemtElement.Attribute("val").Value, out bool SubEnabled) && pl.PlayerProperties.SubEnabled != SubEnabled)
                {
                    pl.PlayerProperties.SubEnabled = SubEnabled;
                    if (LastChangeDates[SonosEnums.EventingEnums.SubEnabled].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.SubEnabled] = DateTime.Now;
                        LastChangeByEvent = DateTime.Now;
                        return;
                    }
                    ManuellStateChange(SonosEnums.EventingEnums.SubEnabled, DateTime.Now);
                }
            }
            XElement SubPolarityElemtElement = instance.Element(ns + "SubPolarity");
            if (SubPolarityElemtElement != null)
            {
                var SubPolarity = SubPolarityElemtElement.Attribute("val").Value;
                if (pl.PlayerProperties.SubPolarity != SubPolarity)
                {
                    pl.PlayerProperties.SubPolarity = SubPolarity;
                    if (LastChangeDates[SonosEnums.EventingEnums.SubPolarity].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.SubPolarity] = DateTime.Now;
                        LastChangeByEvent = DateTime.Now;
                        return;
                    }
                    ManuellStateChange(SonosEnums.EventingEnums.SubPolarity, DateTime.Now);
                }
            }
            XElement SubCrossoverElemtElement = instance.Element(ns + "SubCrossover");
            if (SubCrossoverElemtElement != null)
            {
                var SubCrossover = SubCrossoverElemtElement.Attribute("val").Value;
                if (pl.PlayerProperties.SubCrossover != SubCrossover)
                {
                    pl.PlayerProperties.SubCrossover = SubCrossover;
                    if (LastChangeDates[SonosEnums.EventingEnums.SubCrossover].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.SubCrossover] = DateTime.Now;
                        LastChangeByEvent = DateTime.Now;
                        return;
                    }
                    ManuellStateChange(SonosEnums.EventingEnums.SubCrossover, DateTime.Now);
                }
            }
            XElement SubGainElemtElement = instance.Element(ns + "SubGain");
            if (SubGainElemtElement != null)
            {
                var SubGain = SubGainElemtElement.Attribute("val").Value;
                if (pl.PlayerProperties.SubGain != SubGain)
                {
                    pl.PlayerProperties.SubGain = SubGain;
                    if (LastChangeDates[SonosEnums.EventingEnums.SubGain].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.SubGain] = DateTime.Now;
                        LastChangeByEvent = DateTime.Now;
                        return;
                    }
                    ManuellStateChange(SonosEnums.EventingEnums.SubGain, DateTime.Now);
                }
            }
            XElement SpeakerSizeElemtElement = instance.Element(ns + "SpeakerSize");
            if (SpeakerSizeElemtElement != null)
            {
                if (int.TryParse(SpeakerSizeElemtElement.Attribute("val").Value, out int SpeakerSize) && pl.PlayerProperties.SpeakerSize != SpeakerSize)
                {
                    pl.PlayerProperties.SpeakerSize = SpeakerSize;
                    if (LastChangeDates[SonosEnums.EventingEnums.SpeakerSize].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.SpeakerSize] = DateTime.Now;
                        LastChangeByEvent = DateTime.Now;
                        return;
                    }
                    ManuellStateChange(SonosEnums.EventingEnums.SpeakerSize, DateTime.Now);
                }
            }
        }
        #endregion Eventing
        #region public Methoden
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="InstanceID"></param>
        /// <returns>CurrentBass</returns>
        public async Task<int> GetBass(UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("CurrentBass", null);
            await Invoke("GetBass", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            int intvalue = 0;
            if (arguments[1].DataValue != null && int.TryParse(arguments[1].DataValue.ToString(), out intvalue) && pl.PlayerProperties.Bass != intvalue)
            {
                pl.PlayerProperties.Bass = intvalue;
                ManuellStateChange(SonosEnums.EventingEnums.Bass, DateTime.Now);
            }
            return intvalue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="EQType"></param>
        /// <param name="InstanceID"></param>
        /// <returns>CurrentValue</returns>
        public async Task<int> GetEQ(string EQType, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("EQType", EQType);
            arguments[2] = new UPnPArgument("CurrentValue", null);
            await Invoke("GetEQ", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 2, 100, 10, WaiterTypes.String);
            int intvalue = 0;
            if (arguments[2].DataValue != null)
                int.TryParse(arguments[2].DataValue.ToString(), out intvalue);
            return intvalue;
        }
        public async Task<Boolean> GetHeadphoneConnected(UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("CurrentHeadphoneConnected", null);
            await Invoke("GetHeadphoneConnected", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            Boolean boolvalue = false;

            if (arguments[1].DataValue != null && Boolean.TryParse(arguments[1].DataValue.ToString(), out boolvalue) && pl.PlayerProperties.HeadphoneConnected != boolvalue)
            {
                pl.PlayerProperties.HeadphoneConnected = boolvalue;
                ManuellStateChange(SonosEnums.EventingEnums.HeadphoneConnected, DateTime.Now);
            }
            return boolvalue;
        }
        public async Task<Boolean> GetLoudness(SonosEnums.SpeakerSelection speakerSelection = SonosEnums.SpeakerSelection.Master, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("Channel", speakerSelection.ToString());
            arguments[2] = new UPnPArgument("CurrentLoudness", null);
            await Invoke("GetLoudness", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 2, 100, 10, WaiterTypes.String);
            Boolean boolvalue = false;

            if (arguments[2].DataValue != null && Boolean.TryParse(arguments[2].DataValue.ToString(), out boolvalue) && pl.PlayerProperties.Loudness != boolvalue)
            {
                pl.PlayerProperties.Loudness = boolvalue;
                ManuellStateChange(SonosEnums.EventingEnums.Loudness, DateTime.Now);
            }
            return boolvalue;
        }
        /// <summary>
        /// Ermittelt unabhängig von der Property die Stummschaltung.
        /// Setzt die Property
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="speakerSelection"></param>
        /// <returns></returns>
        public async Task<Boolean> GetMute(SonosEnums.SpeakerSelection speakerSelection = SonosEnums.SpeakerSelection.Master, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("Channel", speakerSelection.ToString());
            arguments[2] = new UPnPArgument("CurrentMute", null);
            await Invoke("GetMute", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 2, 100, 10, WaiterTypes.String);
            Boolean boolvalue = false;

            if (arguments[2].DataValue != null && Boolean.TryParse(arguments[2].DataValue.ToString(), out boolvalue) && pl.PlayerProperties.Mute != boolvalue)
            {
                pl.PlayerProperties.Mute = boolvalue;
                ManuellStateChange(SonosEnums.EventingEnums.Mute, DateTime.Now);
            }
            return boolvalue;
        }
        public async Task<Boolean> GetOutputFixed(UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("CurrentFixed", null);
            await Invoke("GetOutputFixed", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            Boolean boolvalue = false;

            if (arguments[1].DataValue != null && Boolean.TryParse(arguments[1].DataValue.ToString(), out boolvalue) && pl.PlayerProperties.OutputFixed != boolvalue)
            {
                pl.PlayerProperties.OutputFixed = boolvalue;
                ManuellStateChange(SonosEnums.EventingEnums.OutputFixed, DateTime.Now);
            }
            return boolvalue;
        }
        public async Task<RoomCalibrationStatus> GetRoomCalibrationStatus(UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("RoomCalibrationEnabled", null);
            arguments[2] = new UPnPArgument("RoomCalibrationAvailable", null);
            await Invoke("GetRoomCalibrationStatus", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 2, 100, 10, WaiterTypes.String);
            RoomCalibrationStatus rcs = new();
            if (arguments[2].DataValue != null && Boolean.TryParse(arguments[2].DataValue.ToString(), out bool boolvalue))
                rcs.RoomCalibrationAvailable = boolvalue;
            if (arguments[1].DataValue != null && Boolean.TryParse(arguments[1].DataValue.ToString(), out bool boolvalue2))
                rcs.RoomCalibrationEnabled = boolvalue2;
            return rcs;
        }
        public async Task<Boolean> GetSupportsOutputFixed(UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("CurrentSupportsFixed", null);
            await Invoke("GetSupportsOutputFixed", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            Boolean boolvalue = false;

            if (arguments[1].DataValue != null && Boolean.TryParse(arguments[1].DataValue.ToString(), out boolvalue) && pl.PlayerProperties.SupportOutputFixed != boolvalue)
            {
                pl.PlayerProperties.SupportOutputFixed = boolvalue;
                ManuellStateChange(SonosEnums.EventingEnums.SupportOutputFixed, DateTime.Now);
            }

            return boolvalue;
        }
        public async Task<int> GetTreble(UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("CurrentTreble", null);
            await Invoke("GetTreble", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            int intvalue = 0;
            if (arguments[1].DataValue != null && int.TryParse(arguments[1].DataValue.ToString(), out intvalue) && pl.PlayerProperties.Treble != intvalue)
            {
                pl.PlayerProperties.Treble = intvalue;
                ManuellStateChange(SonosEnums.EventingEnums.Treble, DateTime.Now);
            }
            return intvalue;
        }
        /// <summary>
        /// Lautstärke ermitteln
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="speakerSelection">Channel/Speaker Selection</param>
        /// <returns></returns>
        public async Task<int> GetVolume(SonosEnums.SpeakerSelection speakerSelection = SonosEnums.SpeakerSelection.Master, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("Channel", speakerSelection.ToString());
            arguments[2] = new UPnPArgument("CurrentVolume", null);
            await Invoke("GetVolume", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 2, 100, 10, WaiterTypes.String);
            int intvalue = 0;
            if (arguments[2].DataValue != null && int.TryParse(arguments[2].DataValue.ToString(), out intvalue) && pl.PlayerProperties.Volume != intvalue)
            {
                pl.PlayerProperties.Volume = intvalue;
                ManuellStateChange(SonosEnums.EventingEnums.Volume, DateTime.Now);
            }
            return intvalue;
        }
        public async Task<int> GetVolumeDB(SonosEnums.SpeakerSelection speakerSelection = SonosEnums.SpeakerSelection.Master, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("Channel", speakerSelection.ToString());
            arguments[2] = new UPnPArgument("CurrentVolume", null);
            await Invoke("GetVolumeDB", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 2, 100, 10, WaiterTypes.String);
            int intvalue = 0;
            if (arguments[2].DataValue != null)
                int.TryParse(arguments[2].DataValue.ToString(), out intvalue);

            return intvalue;
        }
        public async Task<MinMaxInt> GetVolumeDBRange(SonosEnums.SpeakerSelection speakerSelection = SonosEnums.SpeakerSelection.Master, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[4];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("Channel", speakerSelection.ToString());
            arguments[2] = new UPnPArgument("MinValue", null);
            arguments[3] = new UPnPArgument("MaxValue", null);
            await Invoke("GetVolumeDBRange", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 2, WaiterTypes.String);
            MinMaxInt mmi = new();
            if (arguments[2].DataValue != null && int.TryParse(arguments[2].DataValue.ToString(), out int intvalue))
                mmi.MinValue = intvalue;

            if (arguments[3].DataValue != null && int.TryParse(arguments[3].DataValue.ToString(), out int intvalue2))
                mmi.MaxValue = intvalue2;
            return mmi;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="ProgramURI"></param>
        /// <param name="ResetVolumeAfter"></param>
        /// <param name="DesiredVolume"></param>
        /// <param name="RampType"></param>
        /// <param name="speakerSelection"></param>
        /// <param name="InstanceID"></param>
        /// <returns>RampTime</returns>
        public async Task<int> RampToVolume(string ProgramURI, Boolean ResetVolumeAfter, UInt16 DesiredVolume, SonosEnums.RampTypes RampType, SonosEnums.SpeakerSelection speakerSelection = SonosEnums.SpeakerSelection.Master, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[7];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("Channel", speakerSelection.ToString());
            arguments[2] = new UPnPArgument("RampType", RampType.ToString());
            arguments[3] = new UPnPArgument("DesiredVolume", DesiredVolume);
            arguments[4] = new UPnPArgument("ResetVolumeAfter", ResetVolumeAfter);
            arguments[5] = new UPnPArgument("ProgramURI", ProgramURI);
            arguments[6] = new UPnPArgument("RampTime", null);
            await Invoke("RampToVolume", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 6, 100, 10, WaiterTypes.String);
            int intvalue = 0;
            if (arguments[7].DataValue != null)
                int.TryParse(arguments[7].DataValue.ToString(), out intvalue);
            return intvalue;
        }
        public async Task<BasicEQ> ResetBasicEQ(UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[6];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("Bass", null);
            arguments[2] = new UPnPArgument("Treble", null);
            arguments[3] = new UPnPArgument("Loudness", null);
            arguments[4] = new UPnPArgument("LeftVolume", null);
            arguments[5] = new UPnPArgument("RightVolume", null);
            await Invoke("ResetBasicEQ", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            BasicEQ eq = new();
            if (arguments[1].DataValue != null && int.TryParse(arguments[1].DataValue.ToString(), out int bass))
                eq.Bass = bass;
            if (arguments[2].DataValue != null && int.TryParse(arguments[2].DataValue.ToString(), out int treble))
                eq.Treble = treble;
            if (arguments[4].DataValue != null && int.TryParse(arguments[4].DataValue.ToString(), out int lvol))
                eq.LeftVolume = lvol;
            if (arguments[5].DataValue != null && int.TryParse(arguments[5].DataValue.ToString(), out int rvol))
                eq.RightVolume = rvol;
            if (arguments[3].DataValue != null && Boolean.TryParse(arguments[3].DataValue.ToString(), out bool loudness))
                eq.Loudness = loudness;

            return eq;
        }
        public async Task<Boolean> ResetExtEQ(string EQType, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("EQType", EQType);
            return await Invoke("ResetExtEQ", arguments);
        }
        public async Task<Boolean> RestoreVolumePriorToRamp(SonosEnums.SpeakerSelection speakerSelection = SonosEnums.SpeakerSelection.Master, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("Channel", speakerSelection.ToString());
            return await Invoke("RestoreVolumePriorToRamp", arguments);
        }
        public async Task<Boolean> SetBass(Int16 DesiredBass, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("DesiredBass", DesiredBass);
            if (await Invoke("SetBass", arguments))
            {
                if (pl.PlayerProperties.Bass != DesiredBass)
                {
                    pl.PlayerProperties.Bass = DesiredBass;
                    ManuellStateChange(SonosEnums.EventingEnums.Bass, DateTime.Now);
                }
                return true;
            }
            return false;
        }
        public async Task<Boolean> SetChannelMap(string ChannelMap, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("ChannelMap", ChannelMap);
            return await Invoke("SetChannelMap", arguments);
        }
        public async Task<Boolean> SetEQ(string EQType, UInt16 DesiredValue, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("EQType", EQType);
            arguments[2] = new UPnPArgument("DesiredValue", DesiredValue);
            return await Invoke("SetEQ", arguments);
        }
        public async Task<Boolean> SetLoudness(Boolean DesiredLoudness = true, SonosEnums.SpeakerSelection speakerSelection = SonosEnums.SpeakerSelection.Master, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("Channel", speakerSelection.ToString());
            arguments[2] = new UPnPArgument("DesiredLoudness", DesiredLoudness);
            return await Invoke("SetLoudness", arguments);
        }
        /// <summary>
        /// Setzt die Stummschaltung nach der negierten Version der SonosPlayer.Mute Property. 
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="speakerSelection"></param>
        /// <returns></returns>
        public async Task<Boolean> SetMute(bool setmute, SonosEnums.SpeakerSelection speakerSelection = SonosEnums.SpeakerSelection.Master, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("Channel", speakerSelection.ToString());
            arguments[2] = new UPnPArgument("DesiredMute", setmute);
            var ret = await Invoke("SetMute", arguments);
            if (ret && pl.PlayerProperties.Mute != setmute)
            {
                pl.PlayerProperties.Mute = setmute;
                ManuellStateChange(SonosEnums.EventingEnums.Mute, DateTime.Now);
            }
            return ret;
        }
        public async Task<Boolean> SetOutputFixed(Boolean DesiredFixed = true, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("DesiredFixed", DesiredFixed);
            return await Invoke("SetOutputFixed", arguments);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="Adjustment"></param>
        /// <param name="speakerSelection"></param>
        /// <param name="InstanceID"></param>
        /// <returns>NewVolume</returns>
        public async Task<int> SetRelativeVolume(UInt16 Adjustment, SonosEnums.SpeakerSelection speakerSelection = SonosEnums.SpeakerSelection.Master, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[4];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("Channel", speakerSelection);
            arguments[2] = new UPnPArgument("Adjustment", Adjustment);
            arguments[3] = new UPnPArgument("NewVolume", null);
            await Invoke("SetRelativeVolume", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 3, 100, 10, WaiterTypes.String);
            int intvalue = 0;
            if (arguments[3].DataValue != null)
                int.TryParse(arguments[3].DataValue.ToString(), out intvalue);
            return intvalue;
        }
        public async Task<Boolean> SetRoomCalibrationStatus(Boolean RoomCalibrationEnabled = true, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[2] = new UPnPArgument("RoomCalibrationEnabled", RoomCalibrationEnabled);
            return await Invoke("SetRoomCalibrationStatus", arguments);
        }
        public async Task<Boolean> SetRoomCalibrationX(string CalibrationID, string Coefficients, string CalibrationMode, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[4];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("CalibrationID", CalibrationID);
            arguments[2] = new UPnPArgument("Coefficients", Coefficients);
            arguments[3] = new UPnPArgument("CalibrationMode", CalibrationMode);
            return await Invoke("SetRoomCalibrationX", arguments);
        }
        public async Task<Boolean> SetTreble(Int16 DesiredTreble, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("DesiredTreble", DesiredTreble);
            return await Invoke("SetTreble", arguments);
        }
        /// <summary>
        /// Lautstärke anpassen 
        /// </summary>
        /// <param name="pl">Sonosplayer</param>
        /// <param name="vol">(Wert von 1 - 100)</param>
        /// <returns></returns>
        public async Task<Boolean> SetVolume(int vol, SonosEnums.SpeakerSelection speakerSelection = SonosEnums.SpeakerSelection.Master, UInt32 InstanceID = 0)
        {
            if (vol > 0 && vol < 101)
            {
                UInt16.TryParse(vol.ToString(), out ushort volarg);
                var arguments = new UPnPArgument[3];
                arguments[0] = new UPnPArgument("InstanceID", InstanceID);
                arguments[1] = new UPnPArgument("Channel", speakerSelection.ToString());
                arguments[2] = new UPnPArgument("DesiredVolume", volarg);
                var ret = await Invoke("SetVolume", arguments);
                if (ret && pl.PlayerProperties.Volume != vol)
                {
                    pl.PlayerProperties.Volume = vol;
                    ManuellStateChange(SonosEnums.EventingEnums.Volume, DateTime.Now);
                }
                return ret;
            }
            else { throw new Exception("The Volume is out of Range"); }
        }
        public async Task<Boolean> SetVolumeDB(UInt16 vol, SonosEnums.SpeakerSelection speakerSelection = SonosEnums.SpeakerSelection.Master, UInt32 InstanceID = 0)
        {
            if (vol > 0 && vol < 101)
            {
                var arguments = new UPnPArgument[3];
                arguments[0] = new UPnPArgument("InstanceID", InstanceID);
                arguments[1] = new UPnPArgument("Channel", speakerSelection.ToString());
                arguments[2] = new UPnPArgument("DesiredVolume", vol);
                return await Invoke("SetVolumeDB", arguments);
            }
            else { throw new Exception("The Volume is out of Range"); }
        }
        #endregion public Methoden
        #region private Methoden
        private async Task<Boolean> Invoke(String Method, UPnPArgument[] arguments, int Sleep = 0)
        {
            try
            {
                if (RenderingControlService == null)
                {
                    pl.ServerErrorsAdd(Method, ClassName, new Exception(Method + " " + ClassName + " ist null"));
                    return false;
                }
                RenderingControlService.InvokeAsync(Method, arguments);
                await Task.Delay(Sleep);
                return true;
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd(Method, ClassName, ex);
                return false;
            }
        }
        private void ManuellStateChange(SonosEnums.EventingEnums t, DateTime _lastchange)
        {
            try
            {
                if (RenderingControl_Changed == null) return;
                LastChangeDates[t] = _lastchange;
                LastChangeByEvent = _lastchange;
                RenderingControl_Changed(t, pl);
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("DeviceProperties_Changed", ClassName, ex);
            }
        }
        #endregion private Methoden
    }
}
