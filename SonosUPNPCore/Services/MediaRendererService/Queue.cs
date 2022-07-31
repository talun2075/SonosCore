using OSTL.UPnP;
using SonosData;
using SonosData.DataClasses;
using SonosUPnP.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SonosUPnP.Services.MediaRendererService
{
    public class Queue
    {
        #region Klassenvariablen
        private const string ClassName = "Queue";
        private UPnPService queueControl;
        private readonly SonosPlayer pl;
        private UPnPDevice mediaRendererService;
        public UPnPStateVariable LastChange { get; set; }
        public event EventHandler<SonosPlayer> Queue_Changed = delegate { };
        private readonly Dictionary<SonosEnums.EventingEnums, DateTime> LastChangeDates = new();
        public DateTime LastChangeByEvent { get; private set; }
        #endregion Klassenvariablen
        #region ctor und Service
        public Queue(SonosPlayer sp)
        {
            pl = sp;
            LastChangeDates.Add(SonosEnums.EventingEnums.QueueChanged, new DateTime());
        }
        public UPnPService QueueService
        {
            get
            {
                if (queueControl != null)
                    return queueControl;
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
                queueControl = mediaRendererService.GetService("urn:sonos-com:serviceId:Queue");
                return queueControl;
            }
        }
        #endregion ctor und Service
        #region Eventing
        public void SubscripeToEvents()
        {
            QueueService.Subscribe(600, (service, subscribeok) =>
            {
                if (!subscribeok)
                    return;

                LastChange = service.GetStateVariableObject("LastChange");
                LastChange.OnModified += EventFired_LastChange;
            });
        }

        private void EventFired_LastChange(UPnPStateVariable sender, object NewValue)
        {
            String newState = sender.Value.ToString();
            try
            {
                XNamespace ns = "urn:schemas-sonos-com:metadata-1-0/Queue/";
                XElement instance;
                var xEvent = XElement.Parse(newState);
                instance = xEvent.Element(ns + "QueueID");
                XElement updateid = instance.Element(ns + "UpdateID");
                if (int.TryParse(updateid.Attribute("val").Value, out int converval) && pl.PlayerProperties.QueueChanged != converval)
                {
                    pl.PlayerProperties.QueueChanged = converval;
                    if (LastChangeDates[SonosEnums.EventingEnums.QueueChanged].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.QueueChanged] = DateTime.Now;
                        LastChangeByEvent = DateTime.Now;
                        return;
                    }
                    ManuellStateChange(SonosEnums.EventingEnums.QueueChanged, DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("Queue:EventFired_LastChange", ClassName, ex);
            }
        }


        #endregion Eventing
        #region public Methoden
        public async Task<QueueData> AddMultipleURIs(UInt16 QueueID, UInt16 UpdateID, string ContainerURI, string ContainerMetaData, UInt16 DesiredFirstTrackNumberEnqueued, Boolean EnqueueAsNext, UInt16 NumberOfURIs, string EnqueuedURIsAndMetaData)
        {
            var arguments = new UPnPArgument[12];
            arguments[0] = new UPnPArgument("QueueID", QueueID);
            arguments[1] = new UPnPArgument("UpdateID", UpdateID);
            arguments[2] = new UPnPArgument("ContainerURI", ContainerURI);
            arguments[3] = new UPnPArgument("ContainerMetaData", ContainerMetaData);
            arguments[4] = new UPnPArgument("DesiredFirstTrackNumberEnqueued", DesiredFirstTrackNumberEnqueued);
            arguments[5] = new UPnPArgument("EnqueueAsNext", EnqueueAsNext);
            arguments[6] = new UPnPArgument("NumberOfURIs", NumberOfURIs);
            arguments[7] = new UPnPArgument("EnqueuedURIsAndMetaData", EnqueuedURIsAndMetaData);
            arguments[8] = new UPnPArgument("FirstTrackNumberEnqueued", null);
            arguments[9] = new UPnPArgument("NumTracksAdded", null);
            arguments[10] = new UPnPArgument("NewQueueLength", null);
            arguments[11] = new UPnPArgument("NewUpdateID", null);
            await Invoke("AddMultipleURIs", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 11, 100, 10, WaiterTypes.String);
            var qd = new QueueData();
            if (int.TryParse(arguments[8].DataValue.ToString(), out int FirstTrackNumberEnqueued))
                qd.FirstTrackNumberEnqueued = FirstTrackNumberEnqueued;
            if (int.TryParse(arguments[9].DataValue.ToString(), out int NumTracksAdded))
                qd.NumTracksAdded = NumTracksAdded;
            if (int.TryParse(arguments[10].DataValue.ToString(), out int NewQueueLength))
                qd.NewQueueLength = NewQueueLength;
            if (ushort.TryParse(arguments[11].DataValue.ToString(), out ushort NewUpdateID))
                qd.NewUpdateID = NewUpdateID;
            qd.QueueID = QueueID;
            return qd;
        }
        public async Task<QueueData> AddURI(UInt16 QueueID, UInt16 UpdateID, string EnqueuedURI, string EnqueuedURIMetaData, UInt16 DesiredFirstTrackNumberEnqueued, Boolean EnqueueAsNext)
        {
            var arguments = new UPnPArgument[10];
            arguments[0] = new UPnPArgument("QueueID", QueueID);
            arguments[1] = new UPnPArgument("UpdateID", UpdateID);
            arguments[2] = new UPnPArgument("EnqueuedURI", EnqueuedURI);
            arguments[3] = new UPnPArgument("EnqueuedURIMetaData", EnqueuedURIMetaData);
            arguments[4] = new UPnPArgument("DesiredFirstTrackNumberEnqueued", DesiredFirstTrackNumberEnqueued);
            arguments[5] = new UPnPArgument("EnqueueAsNext", EnqueueAsNext);
            arguments[6] = new UPnPArgument("FirstTrackNumberEnqueued", null);
            arguments[7] = new UPnPArgument("NumTracksAdded", null);
            arguments[8] = new UPnPArgument("NewQueueLength", null);
            arguments[9] = new UPnPArgument("NewUpdateID", null);
            await Invoke("AddURI", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 9, 100, 10, WaiterTypes.String);
            var qd = new QueueData();
            if (int.TryParse(arguments[6].DataValue.ToString(), out int FirstTrackNumberEnqueued))
                qd.FirstTrackNumberEnqueued = FirstTrackNumberEnqueued;
            if (int.TryParse(arguments[7].DataValue.ToString(), out int NumTracksAdded))
                qd.NumTracksAdded = NumTracksAdded;
            if (int.TryParse(arguments[8].DataValue.ToString(), out int NewQueueLength))
                qd.NewQueueLength = NewQueueLength;
            if (ushort.TryParse(arguments[9].DataValue.ToString(), out ushort NewUpdateID))
                qd.NewUpdateID = NewUpdateID;

            qd.QueueID = QueueID;
            return qd;
        }
        public async Task<QueueData> AttachQueue(string QueueOwnerID)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("QueueOwnerID", QueueOwnerID);
            arguments[1] = new UPnPArgument("QueueID", null);
            arguments[2] = new UPnPArgument("QueueOwnerContext", null);
            await Invoke("AttachQueue", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            QueueData qd = new();
            if (int.TryParse(arguments[1].DataValue.ToString(), out int QueueID))
                qd.QueueID = QueueID;
            qd.QueueOwnerContext = arguments[2].DataValue.ToString();
            return qd;
        }
        public async Task<Boolean> Backup()
        {
            return await Invoke("Backup", null);
        }
        /// <summary>
        /// Liefert die aktuelle Playlist zurück (Q:0)
        /// </summary>
        /// <param name="QueueID"></param>
        /// <param name="StartingIndex"></param>
        /// <param name="RequestedCount"></param>
        /// <returns></returns>
        public async Task<BrowseResults> Browse(UInt16 QueueID, UInt16 StartingIndex, UInt16 RequestedCount)
        {

            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("QueueID", QueueID);
            arguments[1] = new UPnPArgument("StartingIndex", StartingIndex);
            arguments[2] = new UPnPArgument("RequestedCount", RequestedCount);
            arguments[3] = new UPnPArgument("Result", null);
            arguments[4] = new UPnPArgument("NumberReturned", null);
            arguments[5] = new UPnPArgument("(TotalMatches", null);
            arguments[6] = new UPnPArgument("UpdateID", null);
            await Invoke("CreateQueue", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 6, 100, 10, WaiterTypes.String);
            BrowseResults br = new();
            if (int.TryParse(arguments[4].DataValue.ToString(), out int NumberReturned))
                br.NumberReturned = NumberReturned;
            if (int.TryParse(arguments[5].DataValue.ToString(), out int TotalMatches))
                br.TotalMatches = TotalMatches;
            if (ushort.TryParse(arguments[6].DataValue.ToString(), out ushort UpdateID))
                br.UpdateID = UpdateID;
            br.Result = SonosItem.Parse(arguments[3].DataValue.ToString());
            return br;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="QueueOwnerID"></param>
        /// <param name="QueueOwnerContext"></param>
        /// <param name="QueuePolicy"></param>
        /// <returns>QueueID</returns>
        public async Task<int> CreateQueue(string QueueOwnerID, string QueueOwnerContext, string QueuePolicy)
        {
            var arguments = new UPnPArgument[4];
            arguments[0] = new UPnPArgument("QueueOwnerID", QueueOwnerID);
            arguments[1] = new UPnPArgument("QueueOwnerContext", QueueOwnerContext);
            arguments[2] = new UPnPArgument("QueueID", null);
            arguments[3] = new UPnPArgument("QueuePolicy", QueuePolicy);
            await Invoke("CreateQueue", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 2, 100, 10, WaiterTypes.String);
            int.TryParse(arguments[2].DataValue.ToString(), out int QueueID);
            return QueueID;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="QueueID"></param>
        /// <param name="UpdateID"></param>
        /// <returns>NewUpdateID</returns>
        public async Task<int> RemoveAllTracks(UInt16 QueueID, UInt16 UpdateID)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("QueueID", QueueID);
            arguments[1] = new UPnPArgument("UpdateID", UpdateID);
            arguments[2] = new UPnPArgument("NewUpdateID", null);
            await Invoke("RemoveAllTracks", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 2, 100, 10, WaiterTypes.String);
            int.TryParse(arguments[2].DataValue.ToString(), out int NewUpdateID);
            return NewUpdateID;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="QueueID"></param>
        /// <param name="UpdateID"></param>
        /// <param name="StartingIndex"></param>
        /// <param name="NumberOfTracks"></param>
        /// <returns>NewUpdateID</returns>
        public async Task<int> RemoveTrackRange(UInt16 QueueID, UInt16 UpdateID, UInt16 StartingIndex, UInt16 NumberOfTracks)
        {
            var arguments = new UPnPArgument[5];
            arguments[0] = new UPnPArgument("QueueID", QueueID);
            arguments[1] = new UPnPArgument("UpdateID", UpdateID);
            arguments[2] = new UPnPArgument("StartingIndex", StartingIndex);
            arguments[3] = new UPnPArgument("NumberOfTracks", NumberOfTracks);
            arguments[4] = new UPnPArgument("NewUpdateID", null);
            await Invoke("RemoveTrackRange", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 4, 100, 10, WaiterTypes.String);
            int.TryParse(arguments[4].DataValue.ToString(), out int NewUpdateID);
            return NewUpdateID;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="QueueID"></param>
        /// <param name="UpdateID"></param>
        /// <param name="StartingIndex"></param>
        /// <param name="NumberOfTracks"></param>
        /// <param name="InsertBefore"></param>
        /// <returns>NewUpdateID</returns>
        public async Task<int> ReorderTracks(UInt16 QueueID, UInt16 UpdateID, UInt16 StartingIndex, UInt16 NumberOfTracks, UInt16 InsertBefore)
        {
            var arguments = new UPnPArgument[6];
            arguments[0] = new UPnPArgument("QueueID", QueueID);
            arguments[1] = new UPnPArgument("StartingIndex", StartingIndex);
            arguments[2] = new UPnPArgument("NumberOfTracks", NumberOfTracks);
            arguments[3] = new UPnPArgument("InsertBefore", InsertBefore);
            arguments[4] = new UPnPArgument("UpdateID", UpdateID);
            arguments[5] = new UPnPArgument("NewUpdateID", null);
            await Invoke("ReorderTracks", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 5, 100, 10, WaiterTypes.String);
            int.TryParse(arguments[5].DataValue.ToString(), out int NewUpdateID);
            return NewUpdateID;
        }
        public async Task<QueueData> ReplaceAllTracks(UInt16 QueueID, UInt16 UpdateID, string ContainerURI, string ContainerMetaData, UInt16 CurrentTrackIndex, string NewCurrentTrackIndices, UInt16 NumberOfURIs, string EnqueuedURIsAndMetaData)
        {
            var arguments = new UPnPArgument[10];
            arguments[0] = new UPnPArgument("QueueID", QueueID);
            arguments[1] = new UPnPArgument("UpdateID", UpdateID);
            arguments[2] = new UPnPArgument("ContainerURI", ContainerURI);
            arguments[3] = new UPnPArgument("ContainerMetaData", ContainerMetaData);
            arguments[4] = new UPnPArgument("CurrentTrackIndex", CurrentTrackIndex);
            arguments[5] = new UPnPArgument("NewCurrentTrackIndices", NewCurrentTrackIndices);
            arguments[6] = new UPnPArgument("NumberOfURIs", NumberOfURIs);
            arguments[7] = new UPnPArgument("EnqueuedURIsAndMetaData", EnqueuedURIsAndMetaData);
            arguments[8] = new UPnPArgument("NewQueueLength", null);
            arguments[9] = new UPnPArgument("NewUpdateID", null);
            await Invoke("ReplaceAllTracks", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 9, 100, 10, WaiterTypes.String);
            QueueData qd = new();
            if(int.TryParse(arguments[8].DataValue.ToString(), out int NewQueueLength))
            qd.NewQueueLength = NewQueueLength;
            if(ushort.TryParse(arguments[9].DataValue.ToString(), out ushort NewUpdateID))
            qd.NewUpdateID = NewUpdateID;
            qd.QueueID = QueueID;
            
            return qd;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="QueueID"></param>
        /// <param name="Title"></param>
        /// <param name="ObjectID"></param>
        /// <returns>(string) AssignedObjectID</returns>
        public async Task<String> SaveAsSonosPlaylist(UInt16 QueueID, string Title, string ObjectID)
        {
            var arguments = new UPnPArgument[4];
            arguments[0] = new UPnPArgument("QueueID", QueueID);
            arguments[1] = new UPnPArgument("Title", Title);
            arguments[2] = new UPnPArgument("ObjectID", ObjectID);
            arguments[3] = new UPnPArgument("AssignedObjectID", null);
            await Invoke("SaveAsSonosPlaylist", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 3, 100, 10, WaiterTypes.String);
            return arguments[3].DataValue.ToString();
        }
        #endregion public Methoden
        #region private Methoden
        private async Task<Boolean> Invoke(String Method, UPnPArgument[] arguments, int Sleep = 0)
        {
            try
            {
                if (QueueService == null)
                {
                    pl.ServerErrorsAdd(Method, ClassName, new Exception(Method + " " + ClassName + " ist null"));
                    return false;
                }
                QueueService.InvokeAsync(Method, arguments);
                await Task.Delay(Sleep);
                return true;
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd(Method, ClassName, ex);
                return false;
            }
        }
        private void ManuellStateChange(SonosEnums.EventingEnums t, DateTime lastchange)
        {
            try
            {
                if (Queue_Changed == null) return;
                LastChangeDates[t] = lastchange;
                Queue_Changed(t, pl);
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("DeviceProperties_Changed", ClassName, ex);
            }
        }
        #endregion private Methoden
    }
}
