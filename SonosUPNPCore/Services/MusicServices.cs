using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OSTL.UPnP;
using SonosUPnP.Classes;
using SonosUPnP.DataClasses;

namespace SonosUPnP.Services
{
    public class MusicServices
    {
        #region Klassenvariablen
        private const string ClassName = "MusicServices";
        public UPnPStateVariable ServiceListVersion { get; set; }
        private UPnPService musicServices;
        private readonly SonosPlayer pl;
        public event EventHandler<SonosPlayer> MusicServices_Changed = delegate { };
        private readonly Dictionary<SonosEnums.EventingEnums, DateTime> LastChangeDates = new();
        public DateTime LastChangeByEvent { get; private set; }
        #endregion Klassenvariablen
        #region ctor und Service
        public MusicServices(SonosPlayer sp)
        {
            pl = sp;
            LastChangeDates.Add(SonosEnums.EventingEnums.MusicServiceListVersion, new DateTime());
        }
        /// <summary>
        /// Liefert den MusicService zurück (UPNP)
        /// </summary>
        public UPnPService MusicServicesService
        {
            get
            {
                if (musicServices != null)
                    return musicServices;
                if (pl.Device == null)
                {
                    pl.LoadDevice();
                    pl.LoadDevice();
                    if (pl.Device == null)
                        return null;
                }
                musicServices = pl.Device.GetService("urn:upnp-org:serviceId:MusicServices");
                return musicServices;
            }
        }
        #endregion ctor und Service
        #region Eventing
        public void SubscripeToEvents()
        {
            MusicServicesService.Subscribe(600, (service, subscribeok) =>
            {
                if (!subscribeok)
                    return;

                ServiceListVersion = service.GetStateVariableObject("ServiceListVersion");
                ServiceListVersion.OnModified += EventFired_ServiceListVersion;

            });
        }

        private void EventFired_ServiceListVersion(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.MusicServices_ServiceListVersion != nv)
            {
                pl.PlayerProperties.MusicServices_ServiceListVersion = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.MusicServiceListVersion].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.MusicServiceListVersion] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.MusicServiceListVersion, DateTime.Now);
            }
        }
        #endregion Eventing
        #region public Methoden
        public async Task<Boolean> GetSessionId(UInt16 ServiceId, String Username)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("ServiceId", ServiceId);
            arguments[1] = new UPnPArgument("Username", Username);
            arguments[2] = new UPnPArgument("CoordinatorID", null);
            var retval = await Invoke("GetSessionId", arguments);
            await ServiceWaiter.WaitWhileAsync(arguments, 2, 100, 10, WaiterTypes.String);
            return retval;
        }
        public async Task<AvailableServices> ListAvailableServices()
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("AvailableServiceDescriptorList", null);
            arguments[1] = new UPnPArgument("AvailableServiceTypeList", null);
            arguments[2] = new UPnPArgument("AvailableServiceListVersion", null);
            await Invoke("ListAvailableServices", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            return new AvailableServices(arguments[0].DataValue.ToString(), arguments[1].DataValue.ToString(), arguments[2].DataValue.ToString());
        }
        public async Task<Boolean> UpdateAvailableServices()
        {
            return await Invoke("UpdateAvailableServices", null);
        }
        #endregion public Methoden
        #region private Methoden
        private async Task<Boolean> Invoke(String Method, UPnPArgument[] arguments, int Sleep = 0)
        {
            try
            {
                if (MusicServicesService == null)
                {
                    pl.ServerErrorsAdd(Method, ClassName, new Exception(Method + " " + ClassName + " ist null"));
                    return false;
                }
                MusicServicesService.InvokeAsync(Method, arguments);
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
        /// Dient dazu manuelle Änderungen als Event zu feuern und den LastChange entsprechend zu setzen.
        /// </summary>
        /// <param name="_lastchange"></param>
        private void ManuellStateChange(SonosEnums.EventingEnums t, DateTime _lastchange)
        {
            try
            {
                if (MusicServices_Changed == null) return;
                LastChangeDates[t] = _lastchange;
                LastChangeByEvent = _lastchange;
                MusicServices_Changed(t, pl);
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("GroupManagement_Changed", ClassName, ex);
            }
        }
        #endregion private Methoden
    }
}
