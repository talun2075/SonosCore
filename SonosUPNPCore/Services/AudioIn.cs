using OSTL.UPnP;
using SonosUPnP.Classes;
using SonosUPnP.DataClasses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SonosUPnP.Services
{
    /// <summary>
    /// Klasse für den AudioIn Service
    /// Nur Player mit Eingang haben ein AudioIn
    /// </summary>
    public class AudioIn
    {
        #region Klassenvariablen
        public UPnPStateVariable AudioInputName { get; set; }
        public UPnPStateVariable Icon { get; set; }
        public UPnPStateVariable LeftLineInLevel { get; set; }
        /// <summary>
        /// Ändert sich, wenn der 3,5 Klinken-Anschluß abgezogen oder angesteckt wird.
        /// </summary>
        public UPnPStateVariable LineInConnected { get; set; }
        public UPnPStateVariable Playing { get; set; }
        public UPnPStateVariable RightLineInLevel { get; set; }
        public event EventHandler<SonosPlayer> AudioIn_Changed = delegate { };
        private readonly Dictionary<SonosEnums.EventingEnums, DateTime> LastChangeDates = new();
        private UPnPService audioIn;
        private readonly SonosPlayer pl;
        public DateTime LastChangeByEvent { get; private set; }
        #endregion Klassenvariablen
        #region ctor und Service
        public AudioIn(SonosPlayer sp)
        {
            pl = sp;
            LastChangeDates.Add(SonosEnums.EventingEnums.LineInConnected, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.AudioIn_Playing, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.RightLineInLevel, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.LeftLineInLevel, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.AudioIn_Icon, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.AudioInputName, new DateTime());
        }
        /// <summary>
        /// Liefert den AudioIn Service zurück.
        /// </summary>
        public UPnPService AudioInService
        {
            get
            {
                if (audioIn != null)
                    return audioIn;
                if (pl.Device == null)
                {
                    pl.LoadDevice();
                    if (pl.Device == null)
                        return null;
                }
                audioIn = pl.Device.GetService("urn:upnp-org:serviceId:AudioIn");
                return audioIn;
            }
        }
        #endregion ctor und Service
        #region Eventing
        public void SubscripeToEvents()
        {
            if (AudioInService == null) return;
            AudioInService.Subscribe(600, (service, subscribeok) =>
            {
                if (!subscribeok)
                    return;

                AudioInputName = service.GetStateVariableObject("AudioInputName");
                AudioInputName.OnModified += EventFired_AudioInputName;
                Icon = service.GetStateVariableObject("Icon");
                Icon.OnModified += EventFired_Icon;
                LeftLineInLevel = service.GetStateVariableObject("LeftLineInLevel");
                LeftLineInLevel.OnModified += EventFired_LeftLineInLevel;
                RightLineInLevel = service.GetStateVariableObject("RightLineInLevel");
                RightLineInLevel.OnModified += EventFired_RightLineInLevel;
                Playing = service.GetStateVariableObject("Playing");
                Playing.OnModified += EventFired_Playing;
                LineInConnected = service.GetStateVariableObject("LineInConnected");
                LineInConnected.OnModified += EventFired_LineInConnected;
            });
        }

        private void EventFired_LineInConnected(UPnPStateVariable sender, object NewValue)
        {

            if (bool.TryParse(NewValue.ToString(), out bool lic) && pl.PlayerProperties.AudioInput_LineInConnected != lic)
            {
                pl.PlayerProperties.AudioInput_LineInConnected = lic;
                if (LastChangeDates[SonosEnums.EventingEnums.LineInConnected].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.LineInConnected] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.LineInConnected, DateTime.Now);
            }
        }

        private void EventFired_Playing(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.AudioInput_Playing != nv)
            {
                pl.PlayerProperties.AudioInput_Playing = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.AudioIn_Playing].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.AudioIn_Playing] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.AudioIn_Playing, DateTime.Now);
            }
        }

        private void EventFired_RightLineInLevel(UPnPStateVariable sender, object NewValue)
        {
            
            if (int.TryParse(NewValue.ToString(), out int rll) && pl.PlayerProperties.AudioInput_RightLineInLevel != rll)
            {
                pl.PlayerProperties.AudioInput_RightLineInLevel = rll;
                if (LastChangeDates[SonosEnums.EventingEnums.RightLineInLevel].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.RightLineInLevel] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.RightLineInLevel, DateTime.Now);
            }
        }

        private void EventFired_LeftLineInLevel(UPnPStateVariable sender, object NewValue)
        {
            
            if (int.TryParse(NewValue.ToString(), out int rll) && pl.PlayerProperties.AudioInput_LeftLineInLevel != rll)
            {
                pl.PlayerProperties.AudioInput_LeftLineInLevel = rll;
                if (LastChangeDates[SonosEnums.EventingEnums.LeftLineInLevel].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.LeftLineInLevel] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.LeftLineInLevel, DateTime.Now);
            }
        }

        private void EventFired_Icon(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.AudioInput_Icon != nv)
            {
                pl.PlayerProperties.AudioInput_Icon = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.AudioIn_Icon].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.AudioIn_Icon] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.AudioIn_Icon, DateTime.Now);
            }
        }

        private void EventFired_AudioInputName(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.AudioInput_Name != nv)
            {
                pl.PlayerProperties.AudioInput_Name = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.AudioInputName].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.AudioInputName] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.AudioInputName, DateTime.Now);
            }
        }
        #endregion Eventing
        #region public Methoden
        /// <summary>
        /// Liefert die Komponenten Eingangs Information für den übermittelten Player. 
        /// </summary>
        /// <param name="pl"></param>
        /// <returns>AudioInputAttributes</returns>
        public async Task<AudioInputAttributes> GetAudioInputAttributes()
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("CurrentName", null);
            arguments[1] = new UPnPArgument("CurrentIcon", null);
            await Invoke("GetFormat", arguments, 300);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            return new AudioInputAttributes(arguments[0].DataValue.ToString(), arguments[1].DataValue.ToString());
        }
        /// <summary>
        /// Liefert den LineInLevel des Players.
        /// Bisher immer 1/1
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public async Task<LineInLevel> GetLineInLevel()
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("CurrentLeftLineInLevel", null);
            arguments[1] = new UPnPArgument("CurrentRightLineInLevel", null);
            await Invoke("GetLineInLevel", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            LineInLevel lil = new();
            if (int.TryParse(arguments[0].DataValue.ToString(), out int left) && int.TryParse(arguments[1].DataValue.ToString(), out int right))
            {
                lil.Left = left;
                lil.Right = right;
            }
            return lil;
        }
        /// <summary>
        /// Unbekannt
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="ObjectID"></param>
        /// <returns></returns>
        public async Task<bool> SelectAudio(string ObjectID)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("ObjectID", ObjectID);
            return await Invoke("SelectAudio", arguments);
        }
        /// <summary>
        /// Setzt den Namen in der Oberfläche für Audio IN abspielen. 
        /// </summary>
        /// <param name="pl">Sonosplayer für den das umgesetzt werden soll</param>
        /// <param name="DesiredName">Name (Default: AudioComponent)</param>
        /// <param name="DesiredIcon">Bild (internes von Sonos) (Default: AudioComponent)</param>
        /// <returns></returns>
        public async Task<bool> SetAudioInputAttributes(string DesiredName, string DesiredIcon)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("DesiredName", DesiredName);
            arguments[0] = new UPnPArgument("DesiredIcon", DesiredIcon);
            return await Invoke("SetAudioInputAttributes", arguments);
        }
        /// <summary>
        /// Line In Level setzen. Unbekannt, welche Auswirkung. Da immer 1/1
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="lil"></param>
        /// <returns></returns>
        public async Task<bool> SetLineInLevel(LineInLevel lil)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("DesiredLeftLineInLevel", lil.Left);
            arguments[0] = new UPnPArgument("DesiredRightLineInLevel", lil.Right);
            return await Invoke("SetLineInLevel", arguments);
        }
        /// <summary>
        /// Unbekannt, Startet das Audiosignal bei einem übergebenen Player? 
        /// </summary>
        /// <param name="plAudioIn"></param>
        /// <param name="PlayerToPlay"></param>
        /// <returns></returns>
        public async Task<bool> StartTransmissionToGroup(SonosPlayer PlayerToPlay)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("CoordinatorID", PlayerToPlay.UUID);
            arguments[1] = new UPnPArgument("CurrentTransportSettings", null);
            var retval =  await Invoke("StartTransmissionToGroup", arguments);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            return retval;
        }
        /// <summary>
        /// Unbekannt, stoppt die Übertragung des gewählten Players zum übergebennen Players
        /// </summary>
        /// <param name="plAudioIn"></param>
        /// <param name="PlayerToPlay"></param>
        /// <returns></returns>
        public async Task<bool> StopTransmissionToGroup(SonosPlayer PlayerToPlay)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("CoordinatorID", PlayerToPlay.UUID);
            return await Invoke("StopTransmissionToGroup", arguments);
        }
        #endregion public Methoden
        #region private Methoden
        private async Task<bool> Invoke(string Method, UPnPArgument[] arguments, int Sleep = 0)
        {
            try
            {
                if (AudioInService == null)
                {
                    pl.ServerErrorsAdd(Method, "AudioIn", new Exception(Method + " AlarmClock ist null"));
                    return false;
                }
                AudioInService.InvokeAsync(Method, arguments);
                await Task.Delay(Sleep);
                return true;
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd(Method, "AudioIn", ex);
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
                if (AudioIn_Changed == null) return;
                LastChangeDates[t] = _lastchange;
                LastChangeByEvent = _lastchange;
                AudioIn_Changed(t, pl);
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("AlarmClock_Changed", "AudioIn", ex);
            }
        }
        #endregion private Methoden
    }
}
