using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OSTL.UPnP;
using SonosUPnP.Classes;
using SonosUPnP.DataClasses;

namespace SonosUPnP.Services
{
    public class GroupManagement
    {
        #region Klassenvariablen
        private const string ClassName = "GroupManagement";
        private UPnPService groupManagement;
        private readonly SonosPlayer pl;
        public UPnPStateVariable GroupCoordinatorIsLocal { get; set; }
        public UPnPStateVariable LocalGroupUUID { get; set; }
        public UPnPStateVariable ResetVolumeAfter { get; set; }

        public UPnPStateVariable VirtualLineInGroupID { get; set; }
        public UPnPStateVariable VolumeAVTransportURI { get; set; }

        public event EventHandler<SonosPlayer> GroupManagement_Changed = delegate { };
        private readonly Dictionary<SonosEnums.EventingEnums, DateTime> LastChangeDates = new();
        public DateTime LastChangeByEvent { get; private set; }
        #endregion Klassenvariablen
        #region ctor und Service
        public GroupManagement(SonosPlayer sp)
        {
            pl = sp;
            LastChangeDates.Add(SonosEnums.EventingEnums.VolumeAVTransportURI, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.VirtualLineInGroupID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.ResetVolumeAfter, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.LocalGroupUUID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.GroupCoordinatorIsLocal, new DateTime());
        }
        /// <summary>
        /// Liefert den GroupManagement zurück (UPNP)
        /// </summary>
        public UPnPService GroupManagementService
        {
            get
            {
                if (groupManagement != null)
                    return groupManagement;
                if (pl.Device == null)
                {
                    pl.LoadDevice();
                    if (pl.Device == null)
                        return null;
                }
                groupManagement = pl.Device.GetService("urn:upnp-org:serviceId:GroupManagement");
                return groupManagement;
            }
        }
        #endregion ctor und Service
        #region Eventing
        public void SubscripeToEvents(List<SonosEnums.EventingEnums> AllowedEvents)
        {

            GroupManagementService.Subscribe(600, (service, subscribeok) =>
            {
                if (!subscribeok)
                    return;

                if (AllowedEvents.Contains(SonosEnums.EventingEnums.GroupCoordinatorIsLocal))
                {
                    GroupCoordinatorIsLocal = service.GetStateVariableObject("GroupCoordinatorIsLocal");
                    GroupCoordinatorIsLocal.OnModified += EventFired_GroupCoordinatorIsLocal;
                }
                if (AllowedEvents.Contains(SonosEnums.EventingEnums.LocalGroupUUID))
                {
                    LocalGroupUUID = service.GetStateVariableObject("LocalGroupUUID");
                    LocalGroupUUID.OnModified += EventFired_LocalGroupUUID;
                }
                if (AllowedEvents.Contains(SonosEnums.EventingEnums.ResetVolumeAfter))
                {
                    ResetVolumeAfter = service.GetStateVariableObject("ResetVolumeAfter");
                    ResetVolumeAfter.OnModified += EventFired_ResetVolumeAfter;
                }
                if (AllowedEvents.Contains(SonosEnums.EventingEnums.VirtualLineInGroupID))
                {
                    VirtualLineInGroupID = service.GetStateVariableObject("VirtualLineInGroupID");
                    VirtualLineInGroupID.OnModified += EventFired_VirtualLineInGroupID;
                }
                if (AllowedEvents.Contains(SonosEnums.EventingEnums.VolumeAVTransportURI))
                {
                    VolumeAVTransportURI = service.GetStateVariableObject("VolumeAVTransportURI");
                    VolumeAVTransportURI.OnModified += EventFired_VolumeAVTransportURI;
                }
            });
        }

        private void EventFired_VolumeAVTransportURI(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.GroupManagement_VolumeAVTransportURI != nv)
            {
                pl.PlayerProperties.GroupManagement_VolumeAVTransportURI = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.VolumeAVTransportURI].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.VolumeAVTransportURI] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.VolumeAVTransportURI, DateTime.Now);
            }
        }

        private void EventFired_VirtualLineInGroupID(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.GroupManagement_VirtualLineInGroupID != nv)
            {
                pl.PlayerProperties.GroupManagement_VirtualLineInGroupID = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.VirtualLineInGroupID].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.VirtualLineInGroupID] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.VirtualLineInGroupID, DateTime.Now);
            }
        }

        private void EventFired_ResetVolumeAfter(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.GroupManagement_ResetVolumeAfter != nv)
            {
                pl.PlayerProperties.GroupManagement_ResetVolumeAfter = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.ResetVolumeAfter].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.ResetVolumeAfter] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.ResetVolumeAfter, DateTime.Now);
            }
        }

        private void EventFired_LocalGroupUUID(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (nv.Contains(":"))//NV ist dann RINCON_xxx:YYY yyy= eine Zahl, die nicht benötigt wird.
            {
                nv = nv.Split(':')[0];
            }
            if (pl.PlayerProperties.LocalGroupUUID != nv)
            {
                pl.PlayerProperties.LocalGroupUUID = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.LocalGroupUUID].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.LocalGroupUUID] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.LocalGroupUUID, DateTime.Now);
            }
        }

        private void EventFired_GroupCoordinatorIsLocal(UPnPStateVariable sender, object NewValue)
        {
            
            if (bool.TryParse(NewValue.ToString(), out bool nv) && pl.PlayerProperties.GroupCoordinatorIsLocal != nv)
            {
                pl.PlayerProperties.GroupCoordinatorIsLocal = nv;

                if (LastChangeDates[SonosEnums.EventingEnums.GroupCoordinatorIsLocal].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.GroupCoordinatorIsLocal] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.GroupCoordinatorIsLocal, DateTime.Now);
            }
        }
        #endregion Eventing
        #region public Methoden
        public async Task<Boolean> AddMember(string MemberID, UInt16 BootSeq)
        {
            var arguments = new UPnPArgument[7];
            arguments[0] = new UPnPArgument("MemberID", MemberID);
            arguments[1] = new UPnPArgument("BootSeq", BootSeq);
            arguments[2] = new UPnPArgument("CurrentTransportSettings", null);
            arguments[3] = new UPnPArgument("CurrentURI", null);
            arguments[4] = new UPnPArgument("GroupUUIDJoined", null);
            arguments[5] = new UPnPArgument("ResetVolumeAfter", null);
            arguments[6] = new UPnPArgument("VolumeAVTransportURI", null);
            var retval = await Invoke("AddMember", arguments);
            await ServiceWaiter.WaitWhileAsync(arguments, 2, 100, 10, WaiterTypes.String);
            return retval;
        }
        public async Task<Boolean> RemoveMember(string MemberID)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("MemberID", MemberID);
            return await Invoke("RemoveMember", arguments);
        }
        public async Task<Boolean> ReportTrackBufferingResult(string MemberID, UInt16 ResultCode)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("MemberID", MemberID);
            arguments[1] = new UPnPArgument("ResultCode", ResultCode);
            return await Invoke("ReportTrackBufferingResult", arguments);
        }
        #endregion public Methoden
        #region private Methoden
        private async Task<Boolean> Invoke(String Method, UPnPArgument[] arguments, int Sleep = 0)
        {
            try
            {
                if (GroupManagementService == null)
                {
                    pl.ServerErrorsAdd(Method,ClassName, new Exception(Method + " "+ ClassName+" ist null"));
                    return false;
                }
                GroupManagementService.InvokeAsync(Method, arguments);
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
                if (GroupManagement_Changed == null) return;
                LastChangeDates[t] = _lastchange;
                LastChangeByEvent = _lastchange;
                GroupManagement_Changed(t, pl);
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("GroupManagement_Changed",ClassName, ex);
            }
        }
        #endregion private Methoden
    }
}
