using OSTL.UPnP;
using SonosUPnP.Classes;
using SonosUPnP.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SonosUPnP.Services.MediaRendererService
{
    public class GroupRenderingControl
    {
        #region Klassenvariablen
        private const string ClassName = "GroupRenderingControl";
        private UPnPService grouprenderingControl;
        private readonly SonosPlayer pl;
        private UPnPDevice mediaRendererService;
        public UPnPStateVariable GroupMute { get; set; }
        public UPnPStateVariable GroupVolume { get; set; }
        public UPnPStateVariable GroupVolumeChangeable { get; set; }
        public event EventHandler<SonosPlayer> GroupRenderingControl_Changed = delegate { };
        private readonly Dictionary<SonosEnums.EventingEnums, DateTime> LastChangeDates = new();
        public DateTime LastChangeByEvent { get; private set; }
        #endregion Klassenvariablen
        #region ctor und Service
        public GroupRenderingControl(SonosPlayer sp)
        {
            pl = sp;
            LastChangeDates.Add(SonosEnums.EventingEnums.GroupMute, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.GroupVolume, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.GroupVolumeChangeable, new DateTime());
        }
        public UPnPService GroupRenderingControlService
        {
            get
            {
                if (grouprenderingControl != null)
                    return grouprenderingControl;
                if (mediaRendererService == null)
                    if (pl.Device == null)
                    {
                        pl.LoadDevice();
                    }
                mediaRendererService = pl.Device.EmbeddedDevices.FirstOrDefault(d => d.DeviceURN == "urn:schemas-upnp-org:device:MediaRenderer:1");
                if (mediaRendererService == null)
                    return null;
                grouprenderingControl = mediaRendererService.GetService("urn:upnp-org:serviceId:GroupRenderingControl");
                return grouprenderingControl;
            }
        }
        #endregion ctor und Service
        #region Eventing
        public void SubscripeToEvents()
        {
            if (GroupRenderingControlService == null) return;
            GroupRenderingControlService.Subscribe(600, (service, subscribeok) =>
            {
                if (!subscribeok)
                    return;

                GroupMute = service.GetStateVariableObject("GroupMute");
                GroupMute.OnModified += EventFired_GroupMute;
                GroupVolume = service.GetStateVariableObject("GroupVolume");
                GroupVolume.OnModified += EventFired_GroupVolume;
                GroupVolumeChangeable = service.GetStateVariableObject("GroupVolumeChangeable");
                GroupVolumeChangeable.OnModified += EventFired_GroupVolumeChangeable;
            });
        }

        private void EventFired_GroupVolumeChangeable(UPnPStateVariable sender, object NewValue)
        {
            
            if (Boolean.TryParse(NewValue.ToString(), out bool nv) && pl.PlayerProperties.GroupRenderingControl_GroupVolumeChangeable != nv)
            {
                pl.PlayerProperties.GroupRenderingControl_GroupVolumeChangeable = nv;
                if(LastChangeDates[SonosEnums.EventingEnums.GroupVolumeChangeable].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.GroupVolumeChangeable] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                
                ManuellStateChange(SonosEnums.EventingEnums.GroupVolumeChangeable, DateTime.Now);
            }
        }

        private void EventFired_GroupVolume(UPnPStateVariable sender, object NewValue)
        {
            
            if (int.TryParse(NewValue.ToString(), out int nv) && pl.PlayerProperties.GroupRenderingControl_GroupVolume != nv)
            {
                pl.PlayerProperties.GroupRenderingControl_GroupVolume = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.GroupVolume].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.GroupVolume] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.GroupVolume, DateTime.Now);
            }
        }

        private void EventFired_GroupMute(UPnPStateVariable sender, object NewValue)
        {
            
            if (Boolean.TryParse(NewValue.ToString(), out bool nv) && pl.PlayerProperties.GroupRenderingControl_GroupMute != nv)
            {
                pl.PlayerProperties.GroupRenderingControl_GroupMute = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.GroupMute].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.GroupMute] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.GroupMute, DateTime.Now);
            }
        }
        #endregion Eventing
        #region public Methoden
        public async Task<Boolean> GetGroupMute(UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("CurrentMute", null);
            await Invoke("GetGroupMute", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            Boolean cvalue = false;
            
            if(arguments[1].DataValue != null&& Boolean.TryParse(arguments[1].DataValue.ToString(), out cvalue)&&pl.PlayerProperties.GroupRenderingControl_GroupMute != cvalue)
            {
                pl.PlayerProperties.GroupRenderingControl_GroupMute = cvalue;
                ManuellStateChange(SonosEnums.EventingEnums.GroupMute, DateTime.Now);
            }
            return cvalue;
        }
        public async Task<int> GetGroupVolume()
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("CurrentVolume", null);
            await Invoke("GetGroupVolume", arguments, 50);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            int curvalue = 0;
            if (arguments[1].DataValue != null && int.TryParse(arguments[1].DataValue?.ToString(), out curvalue) && pl.PlayerProperties.GroupRenderingControl_GroupVolume != curvalue)
            {
                pl.PlayerProperties.GroupRenderingControl_GroupVolume = curvalue;
                ManuellStateChange(SonosEnums.EventingEnums.GroupVolume, DateTime.Now);
            }
            return curvalue;
        }
        public async Task<Boolean> SetGroupMute(Boolean DesiredMute, UInt32 InstanceID = 0)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("InstanceID", InstanceID);
            arguments[1] = new UPnPArgument("DesiredMute", DesiredMute);
            var ret = await Invoke("SetGroupMute", arguments);
            if (ret && pl.PlayerProperties.GroupRenderingControl_GroupMute != DesiredMute)
            {
                pl.PlayerProperties.GroupRenderingControl_GroupMute = DesiredMute;
                ManuellStateChange(SonosEnums.EventingEnums.GroupMute, DateTime.Now);
            }
            return ret;
        }
        /// <summary>
        /// Setzt die Gruppenlautstärke
        /// </summary>
        /// <param name="pl">SonosPlayer</param>
        /// <param name="vol">Wert zwischen 1 und 100</param>
        public async Task<Boolean> SetGroupVolume(int vol, UInt32 InstanceID = 0)
        {
            if (vol > 0 && vol < 101)
            {

                var arguments = new UPnPArgument[2];
                arguments[0] = new UPnPArgument("InstanceID", InstanceID);
                arguments[1] = new UPnPArgument("DesiredVolume", Convert.ToUInt16(vol));
                var ret = await Invoke("SetGroupVolume", arguments);
                if (ret && pl.PlayerProperties.GroupRenderingControl_GroupVolume != vol)
                {
                    pl.PlayerProperties.GroupRenderingControl_GroupVolume = vol;
                    ManuellStateChange(SonosEnums.EventingEnums.GroupVolume, DateTime.Now);
                }
                return ret;
            }
            else
            {
                throw new Exception("The Volume is out of Range");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="Adjustment"></param>
        /// <param name="InstanceID"></param>
        /// <returns>NewVolume</returns>
        public async Task<int> SetRelativeGroupVolume(UInt16 Adjustment, UInt32 InstanceID = 0)
        {
                var arguments = new UPnPArgument[3];
                arguments[0] = new UPnPArgument("InstanceID", InstanceID);
                arguments[1] = new UPnPArgument("Adjustment", Adjustment);
                arguments[2] = new UPnPArgument("NewVolume", null);
                await Invoke("SetRelativeGroupVolume", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 2, 100, 10, WaiterTypes.String);
            int curvalue=0;
            if (arguments[2].DataValue != null)
                int.TryParse(arguments[2].DataValue.ToString(), out curvalue);
            return curvalue;
        }
        public async Task<Boolean> SnapshotGroupVolume(UInt32 InstanceID = 0)
        {
                var arguments = new UPnPArgument[1];
                arguments[0] = new UPnPArgument("InstanceID", InstanceID);
                return await Invoke("SnapshotGroupVolume", arguments);
          }
        #endregion public Methoden
        #region private Methoden
        private async Task<Boolean> Invoke(String Method, UPnPArgument[] arguments, int Sleep = 0)
        {
            try
            {
                if (GroupRenderingControlService == null)
                {
                    pl.ServerErrorsAdd(Method, ClassName, new Exception(Method + " "+ ClassName+" ist null"));
                    return false;
                }
                GroupRenderingControlService.InvokeAsync(Method, arguments);
                await Task.Delay(Sleep);
                return true;
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd(Method, ClassName, ex);
                return false;
            }
        }
        /// <summary>
        /// Alle Änderungen in Renderingcontrol
        /// </summary>
        /// <param name="t">Typ was geändert wurde.</param>
        /// <param name="_lastchange"></param>
        private void ManuellStateChange(SonosEnums.EventingEnums t, DateTime _lastchange)
        {
            try
            {
                if (GroupRenderingControl_Changed == null) return;
                LastChangeDates[t] = _lastchange;
                LastChangeByEvent = _lastchange;
                GroupRenderingControl_Changed(t, pl);
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("DeviceProperties_Changed", ClassName, ex);
            }
        }
        #endregion private Methoden
    }
}

