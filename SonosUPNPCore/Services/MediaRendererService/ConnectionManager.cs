using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OSTL.UPnP;
using SonosData.DataClasses;
using SonosUPnP.Classes;

namespace SonosUPnP.Services.MediaRendererService
{
    public class ConnectionManager
    {
        #region Klassenvariablen
        private const string ClassName = "ConnectionManager";
        private UPnPDevice mediaRendererService;
        private UPnPService connectionManager;
        private readonly SonosPlayer pl;
        public UPnPStateVariable CurrentConnectionIDs { get; set; }
        public UPnPStateVariable SinkProtocolInfo { get; set; }
        public UPnPStateVariable SourceProtocolInfo { get; set; }
        public event EventHandler<SonosPlayer> ConnectionManager_Changed = delegate { };
        public DateTime LastChangeByEvent { get; private set; }
        #endregion Klassenvariablen
        #region ctor und Service
        public UPnPService ConnectionManagerService
        {
            get
            {
                if (connectionManager != null)
                    return connectionManager;
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
                connectionManager = mediaRendererService.GetService("urn:upnp-org:serviceId:ConnectionManager");
                return connectionManager;
            }
        }

        public ConnectionManager(SonosPlayer sp)
        {
            pl = sp;
        }
        #endregion ctor und Service
        #region Eventing
        public void SubscripeToEvents()
        {
            if (ConnectionManagerService == null) return;
            ConnectionManagerService.Subscribe(600, (service, subscribeok) =>
            {
                if (!subscribeok)
                    return;

                CurrentConnectionIDs = service.GetStateVariableObject("CurrentConnectionIDs");
                CurrentConnectionIDs.OnModified += EventFired_CurrentConnectionIDs;
                SinkProtocolInfo = service.GetStateVariableObject("SinkProtocolInfo");
                SinkProtocolInfo.OnModified += EventFired_SinkProtocolInfo;
                SourceProtocolInfo = service.GetStateVariableObject("SourceProtocolInfo");
                SourceProtocolInfo.OnModified += EventFired_SourceProtocolInfo;
            });
        }

        private void EventFired_SourceProtocolInfo(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            pl.PlayerProperties.MR_ConnectionManager_SourceProtocolInfo = nv;
        }

        private void EventFired_CurrentConnectionIDs(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            pl.PlayerProperties.MR_ConnectionManager_CurrentConnectionIDs = nv;
        }

        private void EventFired_SinkProtocolInfo(UPnPStateVariable sender, object NewValue)
        {
            List<String> nv = new();
            var nvstring = NewValue.ToString();
            if (nvstring.Contains(','))
            {
                nv = nvstring.Split(',').ToList();
            }
            if (pl.PlayerProperties.MR_ConnectionManager_SinkProtocolInfo != nv)
                pl.PlayerProperties.MR_ConnectionManager_SinkProtocolInfo = nv;

        }
        #endregion Eventing
        #region public Methoden
        public async Task<String> GetCurrentConnectionIDs()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("ConnectionIDs", null);
            await Invoke("GetCurrentConnectionIDs", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 3, WaiterTypes.String);
            return arguments[0].DataValue.ToString();
        }
        public async Task<ConnectionInfo> GetCurrentConnectionInfo(ConnectionInfo cf)
        {
            var arguments = new UPnPArgument[8];
            arguments[0] = new UPnPArgument("ConnectionID", cf.ConnectionID);
            arguments[1] = new UPnPArgument("RcsID", null);
            arguments[2] = new UPnPArgument("AVTransportID", null);
            arguments[3] = new UPnPArgument("ProtocolInfo", null);
            arguments[4] = new UPnPArgument("PeerConnectionManager", null);
            arguments[5] = new UPnPArgument("PeerConnectionID", null);
            arguments[6] = new UPnPArgument("Direction", null);
            arguments[7] = new UPnPArgument("Status", null);
            await Invoke("GetCurrentConnectionInfo", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 7, 100, 10, WaiterTypes.String);
            if(int.TryParse(arguments[1].DataValue.ToString(), out int RcsID))
                cf.RcsID = RcsID;
            if (int.TryParse(arguments[2].DataValue.ToString(), out int AVTransportID))
                cf.AVTransportID = AVTransportID;
            if (int.TryParse(arguments[5].DataValue.ToString(), out int PeerConnectionID))
                cf.PeerConnectionID = PeerConnectionID;

            cf.ProtocolInfo = arguments[3].DataValue.ToString();
            cf.PeerConnectionManager = arguments[4].DataValue.ToString();
            
            cf.Direction = arguments[6].DataValue.ToString();
            cf.Status = arguments[7].DataValue.ToString();
            return cf;
        }
        public async Task<List<String>>  GetProtocolInfo()
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("Source", null);
            arguments[1] = new UPnPArgument("Sink", null);
            await Invoke("GetProtocolInfo", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            return arguments[1].DataValue.ToString().Split(',').ToList();
        }
        #endregion public Methoden
        #region private Methoden
        private async Task<Boolean> Invoke(String Method, UPnPArgument[] arguments, int Sleep = 0)
        {
            try
            {
                if (ConnectionManagerService == null)
                {
                    pl.ServerErrorsAdd(Method, ClassName, new Exception(Method + " "+ ClassName+" ist null"));
                    return false;
                }
                ConnectionManagerService.InvokeAsync(Method, arguments);
                await Task.Delay(Sleep);
                return true;
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd(Method,ClassName, ex);
                return false;
            }
        }
        //private void ManuellStateChange(DateTime _lastchange)
        //{
        //    try
        //    {
        //        if (ConnectionManager_Changed == null) return;
        //        LastChangeByEvent = _lastchange;
        //        ConnectionManager_Changed(this, pl);
        //    }
        //    catch (Exception ex)
        //    {
        //        pl.ServerErrorsAdd("AvTRansport_ManuellStateChange", ClassName, ex);
        //    }
        //}
        #endregion private Methoden

    }
}
