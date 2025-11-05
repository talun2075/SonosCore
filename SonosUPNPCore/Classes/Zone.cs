using HomeLogging;
using OSTL.UPnP;
using SonosData.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonosUPNPCore.Classes
{
    public class Zone
    {
        #region Klassenvariablen
        private UPnPStateVariable AlarmListVersion { get; set; }
        private UPnPStateVariable DailyIndexRefreshTime { get; set; }
        private UPnPStateVariable DateFormat { get; set; }
        private UPnPStateVariable TimeFormat { get; set; }
        private UPnPStateVariable TimeGeneration { get; set; }
        private UPnPStateVariable TimeServer { get; set; }
        private UPnPStateVariable TimeZone { get; set; }
        /// <summary>
        /// Wird aktualisiert wenn der Index neu eingelesen wurde
        /// </summary>
        private UPnPStateVariable ShareListUpdateID { get; set; }
        /// <summary>
        /// Wird gefeuert, wenn eine Sonos Playlist aktualisiert wird.
        /// </summary>
        private UPnPStateVariable SavedQueuesUpdateID { get; set; }
        /// <summary>
        /// Wird gefeuert, wenn bei den Favoriten was geändert wird
        /// </summary>
        private UPnPStateVariable FavoritesUpdateID { get; set; }
        /// <summary>
        /// Wird gefeuert, wenn der Zustand des indizierens sich ändert
        /// </summary>
        private UPnPStateVariable ShareIndexInProgress { get; set; }
        /// <summary>
        /// Wird wohl bei Indizierungsfehlern gefeuert
        /// </summary>
        private UPnPStateVariable ShareIndexLastError { get; set; }
        public event EventHandler<Zone> GlobalSonosChange = delegate { };
        private readonly Dictionary<SonosEnums.EventingEnums, DateTime> LastChangeDates = [];
        private readonly ILogging Logger;
        #endregion Klassenvariablen
        public Zone(ILogging log = null)
        {
            if (log == null)
            {
                Logger = new Logging();
            }
            else
            {
                Logger = log;
            }
            LastChangeDates.Add(SonosEnums.EventingEnums.ShareListUpdateID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.FavoritesUpdateID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.ShareIndexLastError, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.ShareIndexInProgress, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.SavedQueuesUpdateID, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.TimeZone, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.TimeServer, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.TimeGeneration, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.TimeFormat, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.DateFormat, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.DailyIndexRefreshTime, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.AlarmListVersion, new DateTime());
        }
        #region Eventing
        public void StartSubscription(UPnPDevice device)
        {
            var alarmClockService = device.GetService("urn:upnp-org:serviceId:AlarmClock");
            alarmClockService.Subscribe(600, (service, subscribeok) =>
            {
                if (!subscribeok) return;
                AlarmListVersion = service.GetStateVariableObject("AlarmListVersion");
                AlarmListVersion.OnModified += EventFired_AlarmListVersion;
                DailyIndexRefreshTime = service.GetStateVariableObject("DailyIndexRefreshTime");
                DailyIndexRefreshTime.OnModified += EventFired_DailyIndexRefreshTime;
                DateFormat = service.GetStateVariableObject("DateFormat");
                DateFormat.OnModified += EventFired_DateFormat;
                TimeFormat = service.GetStateVariableObject("TimeFormat");
                TimeFormat.OnModified += EventFired_TimeFormat;
                TimeGeneration = service.GetStateVariableObject("TimeGeneration");
                TimeGeneration.OnModified += EventFired_TimeGeneration;
                TimeServer = service.GetStateVariableObject("TimeServer");
                TimeServer.OnModified += EventFired_TimeServer;
                TimeZone = service.GetStateVariableObject("TimeZone");
                TimeZone.OnModified += EventFired_TimeZone;
            });
            var mediaServer = device.EmbeddedDevices.FirstOrDefault(d => d.DeviceURN == "urn:schemas-upnp-org:device:MediaServer:1");
            if (mediaServer != null)
            {
                var contentDirectoryService = mediaServer.GetService("urn:upnp-org:serviceId:ContentDirectory");
                contentDirectoryService.Subscribe(600, (service, subscribeok) =>
                {
                    if (!subscribeok)
                        return;

                    FavoritesUpdateID = service.GetStateVariableObject("FavoritesUpdateID");
                    FavoritesUpdateID.OnModified += EventFired_FavoritesUpdateID;
                    SavedQueuesUpdateID = service.GetStateVariableObject("SavedQueuesUpdateID");
                    SavedQueuesUpdateID.OnModified += EventFired_SavedQueuesUpdateID;
                    ShareIndexInProgress = service.GetStateVariableObject("ShareIndexInProgress");
                    ShareIndexInProgress.OnModified += EventFired_ShareIndexInProgress;
                    ShareIndexLastError = service.GetStateVariableObject("ShareIndexLastError");
                    ShareIndexLastError.OnModified += EventFired_ShareIndexLastError;
                    ShareListUpdateID = service.GetStateVariableObject("ShareListUpdateID");
                    ShareListUpdateID.OnModified += EventFired_ShareListUpdateID;
                });
            }
        }
        private void EventFired_ShareListUpdateID(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (nv.Contains(","))
                nv = nv.Split(',')[1];

            if (int.TryParse(nv, out int nvint) && Properties.ShareListUpdateID != nvint)
            {
                Properties.ShareListUpdateID = nvint;
                if (LastChangeDates[SonosEnums.EventingEnums.ShareListUpdateID].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.ShareListUpdateID] = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.ShareListUpdateID, DateTime.Now);
            }
        }
        private void EventFired_FavoritesUpdateID(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (nv.Contains(","))
                nv = nv.Split(',')[1];

            if (int.TryParse(nv, out int nvint) && Properties.FavoritesUpdateID != nvint)
            {
                Properties.FavoritesUpdateID = nvint;
                if (LastChangeDates[SonosEnums.EventingEnums.FavoritesUpdateID].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.FavoritesUpdateID] = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.FavoritesUpdateID, DateTime.Now);
            }
        }
        private void EventFired_ShareIndexLastError(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (Properties.ShareIndexLastError != nv)
            {
                Properties.ShareIndexLastError = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.ShareIndexLastError].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.ShareIndexLastError] = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.ShareIndexLastError, DateTime.Now);
            }

        }
        private void EventFired_ShareIndexInProgress(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();

            if (bool.TryParse(nv, out bool nvint) && Properties.ShareIndexInProgress != nvint)
            {
                Properties.ShareIndexInProgress = nvint;
                if (LastChangeDates[SonosEnums.EventingEnums.ShareIndexInProgress].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.ShareIndexInProgress] = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.ShareIndexInProgress, DateTime.Now);
            }
        }
        private void EventFired_SavedQueuesUpdateID(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (nv.Contains(","))
                nv = nv.Split(',')[1];

            if (int.TryParse(nv, out int nvint) && Properties.SavedQueuesUpdateID != nvint)
            {
                Properties.SavedQueuesUpdateID = nvint;
                if (LastChangeDates[SonosEnums.EventingEnums.SavedQueuesUpdateID].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.SavedQueuesUpdateID] = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.SavedQueuesUpdateID, DateTime.Now);
            }
        }
        private void EventFired_TimeZone(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (Properties.TimeZone != nv)
            {
                Properties.TimeZone = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.TimeZone].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.TimeZone] = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.TimeZone, DateTime.Now);
            }
        }
        private void EventFired_TimeServer(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (Properties.TimeServer != nv)
            {
                Properties.TimeServer = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.TimeServer].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.TimeServer] = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.TimeServer, DateTime.Now);
            }
        }
        private void EventFired_TimeGeneration(UPnPStateVariable sender, object NewValue)
        {
            if (int.TryParse(NewValue.ToString(), out int tg) && Properties.TimeGeneration != tg)
            {
                Properties.TimeGeneration = tg;
                if (LastChangeDates[SonosEnums.EventingEnums.TimeGeneration].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.TimeGeneration] = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.TimeGeneration, DateTime.Now);
            }
        }
        private void EventFired_TimeFormat(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (Properties.TimeFormat != nv)
            {
                Properties.TimeFormat = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.TimeFormat].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.TimeFormat] = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.TimeFormat, DateTime.Now);
            }
        }
        private void EventFired_DateFormat(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (Properties.DateFormat != nv)
            {
                Properties.DateFormat = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.DateFormat].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.DateFormat] = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.DateFormat, DateTime.Now);
            }
        }
        private void EventFired_DailyIndexRefreshTime(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (Properties.DailyIndexRefreshTime != nv)
            {
                Properties.DailyIndexRefreshTime = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.DailyIndexRefreshTime].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.DailyIndexRefreshTime] = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.DailyIndexRefreshTime, DateTime.Now);
            }
        }
        /// <summary>
        /// Eventing wird verwendet um zu prüfen ob die Version sich geändert hat. 
        /// Falls ja, werden die Alarme in eine entsprechende Liste gelegt und somit aktualisiert.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="NewValue"></param>
        private void EventFired_AlarmListVersion(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (nv.Contains(":"))
            {
                var x = nv.Split(':');

                if (int.TryParse(x[1], out int nvint) && Properties.AlarmListVersion != nvint)
                {
                    Properties.AlarmListVersion = nvint;
                    if (LastChangeDates[SonosEnums.EventingEnums.AlarmListVersion].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.AlarmListVersion] = DateTime.Now;
                        return;
                    }
                    ManuellStateChange(SonosEnums.EventingEnums.AlarmListVersion, DateTime.Now);
                }
            }

        }
        #endregion Eventing
        #region Properties
        public ZoneProperties Properties { get; set; } = new ZoneProperties();
        #endregion Properties
        #region Private Methoden
        /// <summary>
        /// Dient dazu manuelle Änderungen als Event zu feuern und den LastChange entsprechend zu setzen.
        /// </summary>
        /// <param name="_lastchange"></param>
        private void ManuellStateChange(SonosEnums.EventingEnums t, DateTime _lastchange)
        {
            try
            {
                if (GlobalSonosChange == null) return;
                LastChangeDates[t] = _lastchange;
                GlobalSonosChange(t, this);
            }
            catch (Exception ex)
            {
                Logger.ServerErrorsAdd("ManuellStateChange", ex, "Discovery");
            }
        }
        #endregion Private Methoden
    }
}
