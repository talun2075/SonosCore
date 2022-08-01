using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OSTL.UPnP;
using SonosData;
using SonosData.DataClasses;
using SonosUPnP.Classes;

namespace SonosUPnP.Services.MediaServerServices
{
    public class ContentDirectory
    {
        #region Klassenvariablen
        private const string ClassName = "ContentDirectory";
        public UPnPStateVariable Browseable { get; set; }
        public UPnPStateVariable ContainerUpdateIDs { get; set; }
        public UPnPStateVariable FavoritePresetsUpdateID { get; set; }
        /// <summary>
        /// Wird gefeuert, wenn bei den Favoriten was geändert wird
        /// </summary>
        public UPnPStateVariable FavoritesUpdateID { get; set; }
        public UPnPStateVariable RadioFavoritesUpdateID { get; set; }
        public UPnPStateVariable RadioLocationUpdateID { get; set; }

        public UPnPStateVariable RecentlyPlayedUpdateID { get; set; }
        /// <summary>
        /// Wird gefeuert, wenn eine Sonos Playlist aktualisiert wird.
        /// </summary>
        public UPnPStateVariable SavedQueuesUpdateID { get; set; }
        /// <summary>
        /// Wird gefeuert, wenn der Zustand des indizierens sich ändert
        /// </summary>
        public UPnPStateVariable ShareIndexInProgress { get; set; }
        /// <summary>
        /// Wird wohl bei Indizierungsfehlern gefeuert
        /// </summary>
        public UPnPStateVariable ShareIndexLastError { get; set; }
        public UPnPStateVariable ShareListUpdateID { get; set; }
        public UPnPStateVariable SystemUpdateID { get; set; }
        public UPnPStateVariable UserRadioUpdateID { get; set; }

        private UPnPService contentDirectory;
        private UPnPDevice mediaServer;
        private readonly SonosPlayer pl;
        public event EventHandler<SonosPlayer> ContentDirectory_Changed = delegate { };
        public DateTime LastChangeByEvent { get; private set; }
        private readonly Dictionary<SonosEnums.EventingEnums, DateTime> LastChangeDates = new();
        #endregion Klassenvariablen
        #region ctor und Service
        public ContentDirectory(SonosPlayer sp)
        {
            pl = sp;
            LastChangeDates.Add(SonosEnums.EventingEnums.Browseable, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.UserRadioUpdateID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.SystemUpdateID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.ShareListUpdateID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.ShareIndexLastError, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.ShareIndexInProgress, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.SavedQueuesUpdateID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.RecentlyPlayedUpdateID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.RadioLocationUpdateID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.RadioFavoritesUpdateID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.FavoritesUpdateID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.FavoritePresetsUpdateID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.ContainerUpdateIDs, new DateTime());

        }

        /// <summary>
        /// Wird genutzt um die Inhalte zu durchsuchen 
        /// </summary>
        public UPnPService ContentDirectoryService
        {
            get
            {
                if (contentDirectory != null)
                    return contentDirectory;
                if (mediaServer == null)
                    if (pl.Device == null)
                    {
                        pl.LoadDevice();
                        if (pl.Device == null)
                            return null;
                    }
                mediaServer = pl.Device.EmbeddedDevices.FirstOrDefault(d => d.DeviceURN == "urn:schemas-upnp-org:device:MediaServer:1");
                if (mediaServer == null)
                    return null;
                contentDirectory = mediaServer.GetService("urn:upnp-org:serviceId:ContentDirectory");
                return contentDirectory;
            }
        }
        #endregion ctor und Service
        #region Eventing
        public void SubscripeToEvents()
        {
            if (ContentDirectoryService == null) return;
            ContentDirectoryService.Subscribe(600, (service, subscribeok) =>
            {
                if (!subscribeok)
                    return;

                Browseable = service.GetStateVariableObject("Browseable");
                Browseable.OnModified += EventFired_Browseable;
                ContainerUpdateIDs = service.GetStateVariableObject("ContainerUpdateIDs");
                ContainerUpdateIDs.OnModified += EventFired_ContainerUpdateIDs;
                FavoritePresetsUpdateID = service.GetStateVariableObject("FavoritePresetsUpdateID");
                FavoritePresetsUpdateID.OnModified += EventFired_FavoritePresetsUpdateID;
                FavoritesUpdateID = service.GetStateVariableObject("FavoritesUpdateID");
                FavoritesUpdateID.OnModified += EventFired_FavoritesUpdateID;
                RadioFavoritesUpdateID = service.GetStateVariableObject("RadioFavoritesUpdateID");
                RadioFavoritesUpdateID.OnModified += EventFired_RadioFavoritesUpdateID;
                RadioLocationUpdateID = service.GetStateVariableObject("RadioLocationUpdateID");
                RadioLocationUpdateID.OnModified += EventFired_RadioLocationUpdateID;
                RecentlyPlayedUpdateID = service.GetStateVariableObject("RecentlyPlayedUpdateID");
                RecentlyPlayedUpdateID.OnModified += EventFired_RecentlyPlayedUpdateID;
                SavedQueuesUpdateID = service.GetStateVariableObject("SavedQueuesUpdateID");
                SavedQueuesUpdateID.OnModified += EventFired_SavedQueuesUpdateID;
                ShareIndexInProgress = service.GetStateVariableObject("ShareIndexInProgress");
                ShareIndexInProgress.OnModified += EventFired_ShareIndexInProgress;
                ShareIndexLastError = service.GetStateVariableObject("ShareIndexLastError");
                ShareIndexLastError.OnModified += EventFired_ShareIndexLastError;
                ShareListUpdateID = service.GetStateVariableObject("ShareListUpdateID");
                ShareListUpdateID.OnModified += EventFired_ShareListUpdateID;
                SystemUpdateID = service.GetStateVariableObject("SystemUpdateID");
                SystemUpdateID.OnModified += EventFired_SystemUpdateID;
                UserRadioUpdateID = service.GetStateVariableObject("UserRadioUpdateID");
                UserRadioUpdateID.OnModified += EventFired_UserRadioUpdateID;
            });
        }

        private void EventFired_UserRadioUpdateID(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.ContentDirectory_UserRadioUpdateID != nv)
            {
                pl.PlayerProperties.ContentDirectory_UserRadioUpdateID = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.UserRadioUpdateID].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.UserRadioUpdateID] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.UserRadioUpdateID, DateTime.Now);
            }
        }

        private void EventFired_SystemUpdateID(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.ContentDirectory_SystemUpdateID != nv)
            {
                pl.PlayerProperties.ContentDirectory_SystemUpdateID = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.SystemUpdateID].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.SystemUpdateID] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.SystemUpdateID, DateTime.Now);
            }
        }

        private void EventFired_ShareListUpdateID(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.ContentDirectory_ShareListUpdateID != nv)
            {
                pl.PlayerProperties.ContentDirectory_ShareListUpdateID = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.ShareListUpdateID].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.ShareListUpdateID] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.ShareListUpdateID, DateTime.Now);
            }
        }

        private void EventFired_ShareIndexLastError(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.ContentDirectory_ShareIndexLastError != nv)
            {
                pl.PlayerProperties.ContentDirectory_ShareIndexLastError = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.ShareIndexLastError].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.ShareIndexLastError] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.ShareIndexLastError, DateTime.Now);
            }
        }

        private void EventFired_ShareIndexInProgress(UPnPStateVariable sender, object NewValue)
        {
            
            if (Boolean.TryParse(NewValue.ToString(), out bool nv) && pl.PlayerProperties.ContentDirectory_ShareIndexInProgress != nv)
            {
                pl.PlayerProperties.ContentDirectory_ShareIndexInProgress = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.ShareIndexInProgress].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.ShareIndexInProgress] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.ShareIndexInProgress, DateTime.Now);
            }
        }

        private void EventFired_SavedQueuesUpdateID(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (nv.Contains(','))
                nv = nv.Split(',')[1];
            
            if (int.TryParse(nv, out int nvint) && pl.PlayerProperties.ContentDirectory_SavedQueuesUpdateID != nvint)
            {
                pl.PlayerProperties.ContentDirectory_SavedQueuesUpdateID = nvint;
                if (LastChangeDates[SonosEnums.EventingEnums.SavedQueuesUpdateID].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.SavedQueuesUpdateID] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.SavedQueuesUpdateID, DateTime.Now);
            }
        }

        private void EventFired_RecentlyPlayedUpdateID(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.ContentDirectory_RecentlyPlayedUpdateID != nv)
            {
                pl.PlayerProperties.ContentDirectory_RecentlyPlayedUpdateID = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.RecentlyPlayedUpdateID].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.RecentlyPlayedUpdateID] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.RecentlyPlayedUpdateID, DateTime.Now);
            }
        }

        private void EventFired_RadioLocationUpdateID(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (nv.Contains(','))
                nv = nv.Split(',')[1];
            
            if (int.TryParse(nv, out int nvint) && pl.PlayerProperties.ContentDirectory_RadioLocationUpdateID != nvint)
            {
                pl.PlayerProperties.ContentDirectory_RadioLocationUpdateID = nvint;
                if (LastChangeDates[SonosEnums.EventingEnums.RadioLocationUpdateID].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.RadioLocationUpdateID] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.RadioLocationUpdateID, DateTime.Now);
            }
        }

        private void EventFired_RadioFavoritesUpdateID(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.ContentDirectory_RadioFavoritesUpdateID != nv)
            {
                pl.PlayerProperties.ContentDirectory_RadioFavoritesUpdateID = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.RadioFavoritesUpdateID].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.RadioFavoritesUpdateID] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.RadioFavoritesUpdateID, DateTime.Now);
            }
        }

        private void EventFired_FavoritesUpdateID(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.ContentDirectory_FavoritesUpdateID != nv)
            {
                pl.PlayerProperties.ContentDirectory_FavoritesUpdateID = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.FavoritesUpdateID].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.FavoritesUpdateID] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.FavoritesUpdateID, DateTime.Now);
            }
        }

        private void EventFired_FavoritePresetsUpdateID(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.ContentDirectory_FavoritePresetsUpdateID != nv)
            {
                pl.PlayerProperties.ContentDirectory_FavoritePresetsUpdateID = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.FavoritePresetsUpdateID].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.FavoritePresetsUpdateID] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.FavoritePresetsUpdateID, DateTime.Now);
            }
        }

        private void EventFired_ContainerUpdateIDs(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            pl.PlayerProperties.ContentDirectory_ContainerUpdateIDs = nv;
            if (pl.PlayerProperties.ContentDirectory_ContainerUpdateIDs != nv)
            {
                pl.PlayerProperties.ContentDirectory_ContainerUpdateIDs = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.ContainerUpdateIDs].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.ContainerUpdateIDs] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.ContainerUpdateIDs, DateTime.Now);
            }
        }

        private void EventFired_Browseable(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            pl.PlayerProperties.ContentDirectory_Browseable = nv;
            if (pl.PlayerProperties.ContentDirectory_Browseable != nv)
            {
                pl.PlayerProperties.ContentDirectory_Browseable = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.Browseable].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.Browseable] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.Browseable, DateTime.Now);
            }
        }
        #endregion Eventing
        #region public Methoden
        /// <summary>
        /// Durchsucht Sonos nach der übergebenen Query
        /// </summary>
        /// <param name="pl">SonosPlayer</param>
        /// <param name="query">String mit der Suche. In BrowseObjects sind Anhaltspunkte hinterlegt.</param>
        /// <param name="startingIndex">Startindex falls nicht alle Ergebnisses benötigt werden</param>
        /// <param name="requestedCount">Anzahl der Ergebnisses vom Startindex ausgehend.</param>
        /// <param name="bfd">BrowseFlag entweder die Meta Daten oder die Kindselemente</param>
        /// <param name="sleep">Wie lange soll der Thread schlafen um eine Antwort zu erhalten. </param>
        /// <returns>Browseresults. Maximal 1000. Totalnumbers und Numberreturns sind zu berücksichtigen.</returns>
        public async Task<BrowseResults> Browse(string query, int startingIndex = 0, int requestedCount = 0, SonosEnums.BrowseFlagData bfd = SonosEnums.BrowseFlagData.BrowseDirectChildren, int sleep = 50, string filter = "")
        {
            //todo: Direkte Aufrufe auf den Wrapper in ZoneMethods legen
            var arguments = new UPnPArgument[10];
            arguments[0] = new UPnPArgument("ObjectID", query);
            arguments[1] = new UPnPArgument("BrowseFlag", bfd.ToString());
            arguments[2] = new UPnPArgument("Filter", filter);
            arguments[3] = new UPnPArgument("StartingIndex", Convert.ToUInt32(startingIndex));
            arguments[4] = new UPnPArgument("RequestedCount", Convert.ToUInt32(requestedCount));
            arguments[5] = new UPnPArgument("SortCriteria", "");
            arguments[6] = new UPnPArgument("Result", "");
            arguments[7] = new UPnPArgument("NumberReturned", 0u);
            arguments[8] = new UPnPArgument("TotalMatches", 0u);
            arguments[9] = new UPnPArgument("UpdateID", 0u);
            await Invoke("Browse", arguments, sleep);
            BrowseResults br = new();
            await ServiceWaiter.WaitWhileAsync(arguments, 6, 200, 40, WaiterTypes.String);
            if (arguments[7].DataValue != null && arguments[8].DataValue != null && arguments[9].DataValue != null && arguments[6].DataValue != null)
            {
                if (int.TryParse(arguments[7].DataValue.ToString(), out int nr))
                    br.NumberReturned = nr;
                if (int.TryParse(arguments[8].DataValue.ToString(), out int tm))
                    br.TotalMatches = tm;
                if (ushort.TryParse(arguments[9].DataValue.ToString(), out ushort uid))
                    br.UpdateID = uid;
                try
                {
                    if (!string.IsNullOrEmpty(arguments[6].DataValue as String))
                    {
                        br.Result = SonosItem.Parse(arguments[6].DataValue as String);
                    }
                    else
                    {
                        br.Result = new List<SonosItem>();
                    }
                }
                catch
                {
                    br.Result = new List<SonosItem>();
                }
            }
            return br;
        }
        /// <summary>
        /// Erzeugt ein Objekt, welches über den container definiert wird
        /// </summary>
        /// <param name="pl">Sonosplayer</param>
        /// <param name="metadata">Sonos MetaDaten</param>
        /// <param name="container">Container in dem das Objekt erstellt werden soll. Default wird ein Favorit erzeugt.</param>
        public async Task<Boolean> CreateObject(string metadata, string container = "FV:2")
        {
            var arguments = new UPnPArgument[4];
            arguments[0] = new UPnPArgument("ContainerID", container);
            arguments[1] = new UPnPArgument("Elements", metadata);
            arguments[2] = new UPnPArgument("ObjectID", null);
            arguments[3] = new UPnPArgument("Result", null);
            var retval = await Invoke("CreateObject", arguments);
            await ServiceWaiter.WaitWhileAsync(arguments, 3, 100, 10, WaiterTypes.String);
            return retval;
        }
        /// <summary>
        /// Zerstört ein übergebenes Element wie z.B. ein Eintrag aus der Favoriten Liste
        /// <param name="pl">Sonosplayer</param>
        /// <param name="item">Objekt zum vernichten</param>
        /// </summary>
        public async Task<Boolean> DestroyObject(string item)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("ObjectID", item);
            return await Invoke("DestroyObject", arguments);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="ObjectID"></param>
        /// <param name="Prefix"></param>
        /// <returns>StartingIndex</returns>
        public async Task<int> FindPrefix(string ObjectID, string Prefix)
        {
            var arguments = new UPnPArgument[4];
            arguments[0] = new UPnPArgument("ObjectID", ObjectID);
            arguments[1] = new UPnPArgument("Prefix", Prefix);
            arguments[2] = new UPnPArgument("StartingIndex", null);
            arguments[3] = new UPnPArgument("UpdateID", null);
            await Invoke("FindPrefix", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 3, 100, 10, WaiterTypes.String);
            int.TryParse(arguments[2].DataValue.ToString(), out int startind);
            return startind;
        }
        /// <summary>
        /// Bisher war WMP was auch immer das bedeutet.
        /// </summary>
        /// <param name="pl"></param>
        /// <returns>AlbumArtistDisplayOption</returns>
        public async Task<String> GetAlbumArtistDisplayOption()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("AlbumArtistDisplayOption", null);
            await Invoke("GetAlbumArtistDisplayOption", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            return arguments[0].DataValue.ToString();
        }
        public async Task<PrefixLocations> GetAllPrefixLocations(string ObjectID)
        {
            var arguments = new UPnPArgument[4];
            arguments[0] = new UPnPArgument("ObjectID", ObjectID);
            arguments[1] = new UPnPArgument("TotalPrefixes", null);
            arguments[2] = new UPnPArgument("PrefixAndIndexCSV", null);
            arguments[3] = new UPnPArgument("UpdateID", null);
            await Invoke("GetAllPrefixLocations", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 3, 100, 10, WaiterTypes.String);
            PrefixLocations pls = new();
            if(int.TryParse(arguments[3].DataValue.ToString(), out int upid))
                pls.UpdateID = upid;
            pls.TotalPrefixes = Convert.ToInt16(arguments[1].DataValue);
            pls.PrefixAndIndexCSV = arguments[2].DataValue.ToString();
            
            return pls;
        }
        public async Task<Boolean> GetBrowseable()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("IsBrowseable", null);
            await Invoke("GetBrowseable", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            Boolean.TryParse(arguments[0].DataValue.ToString(), out bool browseable);
            return browseable;
        }
        public async Task<DateTime> GetLastIndexChange()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("LastIndexChange", null);
            await Invoke("GetLastIndexChange", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            string retval = String.Empty;
            DateTime date = new();
            if (arguments[0].DataValue != null)
                retval = arguments[0].DataValue.ToString();
            if (!string.IsNullOrEmpty(retval))
            {
                retval = retval.Substring(1);
                date = Convert.ToDateTime(retval);
            }
            return date;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pl"></param>
        /// <returns>SearchCaps</returns>
        public async Task<String> GetSearchCapabilities()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("SearchCaps", null);
            await Invoke("GetSearchCapabilities", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 3, WaiterTypes.String);
            return arguments[0].DataValue.ToString();
        }
        /// <summary>
        /// Prüft, ob der Musikindex gerade aktualisiert wird.
        /// </summary>
        /// <returns></returns>
        public async Task<Boolean> GetShareIndexInProgress()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("IsIndexing", null);
            await Invoke("GetShareIndexInProgress", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            if (Boolean.TryParse(arguments[0].DataValue.ToString(), out bool inpro) && pl.PlayerProperties.ContentDirectory_ShareIndexInProgress != inpro)
            {
                pl.PlayerProperties.ContentDirectory_ShareIndexInProgress = inpro;
                ManuellStateChange(SonosEnums.EventingEnums.ShareIndexInProgress, DateTime.Now);
            }
            return inpro;
        }
        public async Task<String> GetSortCapabilities()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("SortCaps", null);
            await Invoke("GetSortCapabilities", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 3, WaiterTypes.String);
            return arguments[0].DataValue.ToString();
        }
        public async Task<int> GetSystemUpdateID()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("Id", null);
            await Invoke("GetSystemUpdateID", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 3, WaiterTypes.String);
            int.TryParse(arguments[0].DataValue.ToString(), out int upid);
            return upid;
        }
        /// <summary>
        /// Aktualisiert den Musixindex
        /// </summary>
        public async Task<Boolean> RefreshShareIndex(string AlbumArtistDisplayOption = "")
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("AlbumArtistDisplayOption", AlbumArtistDisplayOption);
            pl.PlayerProperties.ContentDirectory_ShareIndexInProgress = true;
            ManuellStateChange(SonosEnums.EventingEnums.ShareIndexInProgress, DateTime.Now);
            return await Invoke("RefreshShareIndex", arguments);

        }
        public async Task<Boolean> RequestResort(string SortOrder)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("SortOrder", SortOrder);
            return await Invoke("RequestResort", arguments);
        }
        public async Task<Boolean> SetBrowseable(Boolean Browseable = true)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("Browseable", Browseable);
            return await Invoke("SetBrowseable", arguments);
        }
        public async Task<Boolean> UpdateObject(string ObjectID, string CurrentTagValue, string NewTagValue)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("ObjectID", ObjectID);
            arguments[1] = new UPnPArgument("CurrentTagValue", CurrentTagValue);
            arguments[2] = new UPnPArgument("NewTagValue", NewTagValue);
            return await Invoke("UpdateObject", arguments);
        }
        #endregion public Methoden
        #region private Methoden
        private async Task<Boolean> Invoke(String Method, UPnPArgument[] arguments, int Sleep = 0)
        {
            try
            {
                if (ContentDirectoryService == null)
                {
                    pl.ServerErrorsAdd(Method, ClassName, new Exception(Method + " " + ClassName + " ist null"));
                    return false;
                }
                ContentDirectoryService.InvokeAsync(Method, arguments);
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
                if (ContentDirectory_Changed == null) return;
                LastChangeByEvent = _lastchange;
                LastChangeDates[t] = _lastchange;
                ContentDirectory_Changed(t, pl);
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("DeviceProperties_Changed", ClassName, ex);
            }
        }
        #endregion private Methoden
    }
}
