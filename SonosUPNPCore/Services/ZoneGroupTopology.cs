using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OSTL.UPnP;
using SonosUPnP.Classes;
using SonosUPnP.DataClasses;

namespace SonosUPnP.Services
{
    public class ZoneGroupTopology
    {
        #region Klassenvariablen
        private const string ClassName = "ZoneGroupTopology";
        private UPnPService zoneGroupTopology;
        private readonly SonosPlayer pl;
        public UPnPStateVariable AlarmRunSequence { get; set; }
        public UPnPStateVariable ZoneGroupID { get; set; }
        public UPnPStateVariable ZoneGroupName { get; set; }
        public UPnPStateVariable ZoneGroupState { get; set; }
        public UPnPStateVariable ZonePlayerUUIDsInGroup { get; set; }
        public UPnPStateVariable AvailableSoftwareUpdate { get; set; }
        public UPnPStateVariable MuseHouseholdId { get; set; }
        public UPnPStateVariable ThirdPartyMediaServersX { get; set; }

        public event EventHandler<SonosPlayer> ZoneGroupTopology_Changed = delegate { };
        private readonly Dictionary<SonosEnums.EventingEnums, DateTime> LastChangeDates = new();
        public DateTime LastChangeByEvent { get; private set; }
        #endregion Klassenvariablen
        #region ctor und Service
        public UPnPService ZoneGroupTopologyService
        {
            get
            {
                if (zoneGroupTopology != null)
                    return zoneGroupTopology;
                if (pl.Device == null)
                {
                    pl.LoadDevice();
                    if (pl.Device == null)
                        return null;
                }
                zoneGroupTopology = pl.Device.GetService("urn:upnp-org:serviceId:ZoneGroupTopology");
                return zoneGroupTopology;
            }
        }

        public ZoneGroupTopology(SonosPlayer sp)
        {
            pl = sp;
            LastChangeDates.Add(SonosEnums.EventingEnums.ThirdPartyMediaServersX, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.MuseHouseholdId, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.AvailableSoftwareUpdate, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.ZonePlayerUUIDsInGroup, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.ZoneGroupName, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.ZoneGroupID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.AlarmRunSequence, new DateTime());
        }
        #endregion ctor und Service
        #region Eventing
        public void SubscripeToEvents()
        {
            if (ZoneGroupTopologyService == null) return;
            ZoneGroupTopologyService.Subscribe(600, (service, subscribeok) =>
            {
                if (!subscribeok)
                    return;

                AlarmRunSequence = service.GetStateVariableObject("AlarmRunSequence");
                AlarmRunSequence.OnModified += EventFired_AlarmRunSequence;
                ZoneGroupID = service.GetStateVariableObject("ZoneGroupID");
                ZoneGroupID.OnModified += EventFired_ZoneGroupID;
                ZoneGroupName = service.GetStateVariableObject("ZoneGroupName");
                ZoneGroupName.OnModified += EventFired_ZoneGroupName;
                AvailableSoftwareUpdate = service.GetStateVariableObject("AvailableSoftwareUpdate");
                AvailableSoftwareUpdate.OnModified += EventFired_AvailableSoftwareUpdate;
                MuseHouseholdId = service.GetStateVariableObject("MuseHouseholdId");
                MuseHouseholdId.OnModified += EventFired_MuseHouseholdId;
                ThirdPartyMediaServersX = service.GetStateVariableObject("ThirdPartyMediaServersX");
                ThirdPartyMediaServersX.OnModified += EventFired_ThirdPartyMediaServersX;
                ZonePlayerUUIDsInGroup = service.GetStateVariableObject("ZonePlayerUUIDsInGroup");
                ZonePlayerUUIDsInGroup.OnModified += EventFired_ZonePlayerUUIDsInGroup;
            });
        }
        private void EventFired_ThirdPartyMediaServersX(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.ZoneGroupTopology_ThirdPartyMediaServersX != nv)
            {
                pl.PlayerProperties.ZoneGroupTopology_ThirdPartyMediaServersX = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.ThirdPartyMediaServersX].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.ThirdPartyMediaServersX] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.ThirdPartyMediaServersX, DateTime.Now);
            }
        }

        private void EventFired_MuseHouseholdId(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.ZoneGroupTopology_MuseHouseholdId != nv)
            {
                pl.PlayerProperties.ZoneGroupTopology_MuseHouseholdId = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.MuseHouseholdId].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.MuseHouseholdId] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.MuseHouseholdId, DateTime.Now);
            }
        }

        private void EventFired_AvailableSoftwareUpdate(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.ZoneGroupTopology_AvailableSoftwareUpdate != nv)
            {
                pl.PlayerProperties.ZoneGroupTopology_AvailableSoftwareUpdate = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.AvailableSoftwareUpdate].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.AvailableSoftwareUpdate] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.AvailableSoftwareUpdate, DateTime.Now);
            }
        }
        private void EventFired_ZonePlayerUUIDsInGroup(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroupAsString != nv)
            {
                if (nv.Contains(","))
                {
                    pl.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup = nv.Split(',').ToList();
                }
                else
                {
                    pl.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup.Clear();
                    pl.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup.Add(nv);
                }
                if (LastChangeDates[SonosEnums.EventingEnums.ZonePlayerUUIDsInGroup].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.ZonePlayerUUIDsInGroup] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.ZonePlayerUUIDsInGroup, DateTime.Now);
            }

        }
        private void EventFired_ZoneGroupName(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.ZoneGroupTopology_ZoneGroupName != nv)
            {
                pl.PlayerProperties.ZoneGroupTopology_ZoneGroupName = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.ZoneGroupName].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.ZoneGroupName] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.ZoneGroupName, DateTime.Now);
            }
        }
        private void EventFired_ZoneGroupID(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.ZoneGroupTopology_ZoneGroupID != nv)
            {
                pl.PlayerProperties.ZoneGroupTopology_ZoneGroupID = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.ZoneGroupID].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.ZoneGroupID] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.ZoneGroupID, DateTime.Now);
            }
        }
        private void EventFired_AlarmRunSequence(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.ZoneGroupTopology_AlarmRunSequence != nv)
            {
                pl.PlayerProperties.ZoneGroupTopology_AlarmRunSequence = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.AlarmRunSequence].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.AlarmRunSequence] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.AlarmRunSequence, DateTime.Now);
            }
        }
        #endregion Eventing
        #region public Methoden
        public async Task<Boolean> BeginSoftwareUpdate(string UpdateURL, UInt16 Flags, string ExtraOptions)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("UpdateURL", UpdateURL);
            arguments[1] = new UPnPArgument("Flags", Flags);
            arguments[2] = new UPnPArgument("ExtraOptions", ExtraOptions);
            return await Invoke("BeginSoftwareUpdate", arguments);
        }
        public async Task<Boolean> CheckForUpdate(Boolean CachedOnly, string Version)
        {
            var arguments = new UPnPArgument[4];
            arguments[0] = new UPnPArgument("UpdateType", null);
            arguments[1] = new UPnPArgument("CachedOnly", CachedOnly);
            arguments[2] = new UPnPArgument("Version", Version);
            arguments[3] = new UPnPArgument("UpdateItem", null);
            var retval =  await Invoke("CheckForUpdate", arguments);
            await ServiceWaiter.WaitWhileAsync(arguments, 3, 100, 10, WaiterTypes.String);
            return retval;
        }
        public async Task<ZoneGroupAttributes> GetZoneGroupAttributes()
        {
            var arguments = new UPnPArgument[4];
            arguments[0] = new UPnPArgument("CurrentZoneGroupName", null);
            arguments[1] = new UPnPArgument("CurrentZoneGroupID", null);
            arguments[2] = new UPnPArgument("CurrentZonePlayerUUIDsInGroup", null);
            arguments[3] = new UPnPArgument("CurrentMuseHouseholdId", null);
            await Invoke("GetZoneGroupAttributes", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            ZoneGroupAttributes za = new();
            string gid = arguments[1].DataValue.ToString();
            if (gid.Contains(":"))
            {
                gid = gid.Split(':')[0];
            }
            za.GroupID = gid;
            za.ZonePlayerUUID = arguments[2].DataValue.ToString().Split(',').ToList();
            za.MuseHouseholdId = arguments[3].DataValue.ToString();
            za.GroupName = arguments[0].DataValue.ToString();
            return za;
        }
        public async Task<ZoneGroupStateList> GetZoneGroupState(int timer = 100)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("ZoneGroupState", null);
            await Invoke("GetZoneGroupState", arguments, timer);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            if (arguments[0].DataValue == null)
            {
                return await GetZoneGroupState(timer + 200);
            }
            return new ZoneGroupStateList(arguments[0].DataValue.ToString());
        }
        public async Task<Boolean> RegisterMobileDevice(string MobileDeviceName, string MobileDeviceUDN, string MobileIPAndPort)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("MobileDeviceName", MobileDeviceName);
            arguments[1] = new UPnPArgument("MobileDeviceUDN", MobileDeviceUDN);
            arguments[2] = new UPnPArgument("MobileIPAndPort", MobileIPAndPort);
            return await Invoke("RegisterMobileDevice", arguments);
        }
        public async Task<Boolean> ReportAlarmStartedRunning()
        {
            return await Invoke("ReportAlarmStartedRunning", null);
        }
        public async Task<Boolean> ReportUnresponsiveDevice(string DeviceUUID, SonosEnums.UnresponsiveDeviceAction DesiredAction)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("DeviceUUID", DeviceUUID);
            arguments[1] = new UPnPArgument("DesiredAction", DesiredAction.ToString());
            return await Invoke("ReportUnresponsiveDevice", arguments);
        }
        public async Task<Boolean> SubmitDiagnostics(Boolean IncludeControllers, string Type)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("DiagnosticID", null);
            arguments[1] = new UPnPArgument("IncludeControllers", IncludeControllers);
            arguments[2] = new UPnPArgument("Type", Type);
            return await Invoke("SubmitDiagnostics", arguments);
        }
        #endregion public Methoden
        #region private Methoden
        private async Task<Boolean> Invoke(String Method, UPnPArgument[] arguments, int Sleep = 0)
        {
            try
            {
                if (ZoneGroupTopologyService == null)
                {
                    pl.ServerErrorsAdd(Method, ClassName, new Exception(Method + " "+ ClassName+" ist null"));
                    return false;
                }
                ZoneGroupTopologyService.InvokeAsync(Method, arguments);
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
                if (ZoneGroupTopology_Changed == null) return;
                LastChangeDates[t] = _lastchange;
                LastChangeByEvent = _lastchange;
                ZoneGroupTopology_Changed(t, pl);
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("DeviceProperties_Changed", ClassName, ex);
            }
        }
        #endregion private Methoden
    }
}
