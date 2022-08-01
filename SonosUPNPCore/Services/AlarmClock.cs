using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OSTL.UPnP;
using SonosData.DataClasses;
using SonosUPnP.Classes;

namespace SonosUPnP.Services
{
    /// <summary>
    /// Liefert für alle Player Informationen, die mit Zeit zu tun haben. Wecker, Uhrzeit und Aktualisierungszeitpunkt der Bibliothekt
    /// </summary>
    public class AlarmClock
    {
        #region Klassenvariablen
        private const string ClassName = "AlarmClock";
        private readonly SonosPlayer pl;
        private UPnPService alarmclock;
        public UPnPStateVariable AlarmListVersion { get; set; }
        public UPnPStateVariable DailyIndexRefreshTime { get; set; }
        public UPnPStateVariable DateFormat { get; set; }
        public UPnPStateVariable TimeFormat { get; set; }
        public UPnPStateVariable TimeGeneration { get; set; }
        public UPnPStateVariable TimeServer { get; set; }
        public UPnPStateVariable TimeZone { get; set; }

        public event EventHandler<SonosPlayer> AlarmClock_Changed = delegate { };
        public DateTime LastChangeByEvent { get; private set; }
        private readonly Dictionary<SonosEnums.EventingEnums, DateTime> LastChangeDates = new();

        #endregion Klassenvariablen
        #region ctor Service
        /// <summary>
        /// Konstruktor mit übergebenen SonosPlayer
        /// </summary>
        /// <param name="sp"></param>
        public AlarmClock(SonosPlayer sp)
        {
            pl = sp;
            LastChangeDates.Add(SonosEnums.EventingEnums.TimeZone, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.TimeServer, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.TimeFormat, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.TimeGeneration, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.DateFormat, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.DailyIndexRefreshTime, new DateTime());
            LastChangeDates.Add(SonosEnums.EventingEnums.AlarmListVersion, new DateTime());
        }
        /// <summary>
        /// Der eigentliche Kommunikationsservice
        /// </summary>
        public UPnPService AlarmClockService
        {
            get
            {
                if (alarmclock != null)
                    return alarmclock;
                if (pl.Device == null)
                {
                    pl.LoadDevice();
                    if (pl.Device == null)
                        return null;
                }
                alarmclock = pl.Device.GetService("urn:upnp-org:serviceId:AlarmClock");
                return alarmclock;
            }
        }
        #endregion ctor Service
        #region Eventing
        public void SubscripeToEvents()
        {
            AlarmClockService.Subscribe(600, (service, subscribeok) =>
            {
                if (!subscribeok)
                    return;

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
        }

        private void EventFired_TimeZone(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.TimeZone != nv)
            {
                pl.PlayerProperties.TimeZone = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.TimeZone].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.TimeZone] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.TimeZone, DateTime.Now);
            }
        }
        private void EventFired_TimeServer(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.TimeServer != nv)
            {
                pl.PlayerProperties.TimeServer = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.TimeServer].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.TimeServer] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.TimeServer, DateTime.Now);
            }
        }

        private void EventFired_TimeGeneration(UPnPStateVariable sender, object NewValue)
        {
            if (int.TryParse(NewValue.ToString(), out int nv))

                if (pl.PlayerProperties.TimeGeneration != nv)
                {
                    pl.PlayerProperties.TimeGeneration = nv;
                    if (LastChangeDates[SonosEnums.EventingEnums.TimeGeneration].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.TimeGeneration] = DateTime.Now;
                        LastChangeByEvent = DateTime.Now;
                        return;
                    }
                    ManuellStateChange(SonosEnums.EventingEnums.TimeGeneration, DateTime.Now);
                }
        }

        private void EventFired_TimeFormat(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.TimeFormat != nv)
            {
                pl.PlayerProperties.TimeFormat = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.TimeFormat].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.TimeFormat] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.TimeFormat, DateTime.Now);
            }
        }

        private void EventFired_DateFormat(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.DateFormat != nv)
            {
                pl.PlayerProperties.DateFormat = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.DateFormat].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.DateFormat] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.DateFormat, DateTime.Now);
            }
        }

        private void EventFired_DailyIndexRefreshTime(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (pl.PlayerProperties.DailyIndexRefreshTime != nv)
            {
                pl.PlayerProperties.DailyIndexRefreshTime = nv;
                if (LastChangeDates[SonosEnums.EventingEnums.DailyIndexRefreshTime].Ticks == 0)
                {
                    LastChangeDates[SonosEnums.EventingEnums.DailyIndexRefreshTime] = DateTime.Now;
                    LastChangeByEvent = DateTime.Now;
                    return;
                }
                ManuellStateChange(SonosEnums.EventingEnums.DailyIndexRefreshTime, DateTime.Now);
            }
        }

        private void EventFired_AlarmListVersion(UPnPStateVariable sender, object NewValue)
        {
            var nv = NewValue.ToString();
            if (nv.Contains(':'))
            {
                var x = nv.Split(':');

                if (int.TryParse(x[1], out int nvint) && pl.PlayerProperties.AlarmListVersion != nvint)
                {
                    pl.PlayerProperties.AlarmListVersion = nvint;
                    if (LastChangeDates[SonosEnums.EventingEnums.AlarmListVersion].Ticks == 0)
                    {
                        LastChangeDates[SonosEnums.EventingEnums.AlarmListVersion] = DateTime.Now;
                        LastChangeByEvent = DateTime.Now;
                        return;
                    }
                    ManuellStateChange(SonosEnums.EventingEnums.AlarmListVersion, DateTime.Now);
                }
            }
        }

        #endregion Eventing
        #region Alarm
        /// <summary>
        /// ERstellt einen neuen Wecker
        /// </summary>
        /// <param name="al">Alarm</param>
        /// <returns>Bool ob es geklappt hat</returns>
        public async Task<Boolean> CreateAlarm(Alarm al)
        {
            var arguments = new UPnPArgument[11];
            arguments[0] = new UPnPArgument("AssignedID", null);
            arguments[1] = new UPnPArgument("StartLocalTime", al.StartTime);
            arguments[2] = new UPnPArgument("Duration", al.Duration);
            arguments[3] = new UPnPArgument("Recurrence", al.Recurrence);
            arguments[4] = new UPnPArgument("Enabled", al.Enabled);
            arguments[5] = new UPnPArgument("RoomUUID", al.RoomUUID);
            arguments[6] = new UPnPArgument("ProgramURI", al.ProgramURI);
            arguments[7] = new UPnPArgument("ProgramMetaData", al.ProgramMetaData);
            arguments[8] = new UPnPArgument("PlayMode", al.PlayMode);
            arguments[9] = new UPnPArgument("Volume", al.Volume);
            arguments[10] = new UPnPArgument("IncludeLinkedZones", al.IncludeLinkedZones);
            var retval = await Invoke("CreateAlarm", arguments);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            return retval;
        }
        /// <summary>
        /// Zerstört einen Wecker
        /// </summary>
        /// <param name="al">Alarm</param>
        /// <returns>Bool ob es geklappt hat</returns>
        public async Task<Boolean> DestroyAlarm(Alarm al)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("ID", al.ID);
            return await Invoke("DestroyAlarm", arguments);
        }
        /// <summary>
        /// Aktualisiert einen Wecker
        /// </summary>
        /// <param name="al">Alarm</param>
        /// <returns>Bool ob es geklappt hat</returns>
        public async Task<Boolean> UpdateAlarm(Alarm al)
        {
            var arguments = new UPnPArgument[11];
            arguments[0] = new UPnPArgument("ID", al.ID);
            arguments[1] = new UPnPArgument("StartLocalTime", al.StartTime);
            arguments[2] = new UPnPArgument("Duration", al.Duration);
            arguments[3] = new UPnPArgument("Recurrence", al.Recurrence);
            arguments[4] = new UPnPArgument("Enabled", al.Enabled);
            arguments[5] = new UPnPArgument("RoomUUID", al.RoomUUID);
            arguments[6] = new UPnPArgument("ProgramURI", al.ProgramURI);
            arguments[7] = new UPnPArgument("ProgramMetaData", al.ProgramMetaData);
            arguments[8] = new UPnPArgument("PlayMode", al.PlayMode);
            arguments[9] = new UPnPArgument("Volume", al.Volume);
            arguments[10] = new UPnPArgument("IncludeLinkedZones", al.IncludeLinkedZones);
            return await Invoke("UpdateAlarm", arguments);
        }
        /// <summary>
        /// Liefert die Liste der Wecker zurück
        /// </summary>
        ///<param name = "sleep" >Interner Wert um auf die Argumente zu warten für Rekursion</ param >
        /// <param name="count">Interner Counter für Rekursion</param>
        /// <returns>Liste mit allen Weckern</returns>/// 
        public async Task<List<Alarm>> ListAlarms(int sleep = 150, int count = 0)
        {
            try
            {
                if (count == 5) return new List<Alarm>();
                var arguments = new UPnPArgument[2];
                arguments[0] = new UPnPArgument("CurrentAlarmList", null);
                arguments[1] = new UPnPArgument("CurrentAlarmListVersion", null);
                await Invoke("ListAlarms", arguments, sleep);
                await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
                if (arguments[0].DataValue == null) return await ListAlarms(sleep + 150, count + 1);
                return Alarm.Parse(arguments[0].DataValue.ToString()).OrderBy(o => o.RoomUUID).ToList();
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("GetAlarms", ClassName, ex);
                return null;
            }
        }
        #endregion Alarm
        #region Clock
        #region RefreshTime
        /// <summary>
        /// Liefert den Zeitpunkt, wann der Index Aktualisiert wird
        /// </summary>
        /// <returns>Format HH:MM:SS als Timespan</returns>
        public async Task<TimeSpan> GetDailyIndexRefreshTime()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("CurrentDailyIndexRefreshTime", null);
            await Invoke("GetDailyIndexRefreshTime", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            return TimeSpan.Parse(arguments[0].DataValue.ToString());
        }
        /// <summary>
        /// Setzen der Zeit, wann die lokale Musikbiliothek aktualisiert werden soll
        /// </summary>
        /// <param name="rt">Format: HH:MM:SS</param>
        public async Task<Boolean> SetDailyIndexRefreshTime(string rt)
        {
            if (string.IsNullOrEmpty(rt)) return false;
            if (TimeSpan.TryParse(rt, out TimeSpan t))
            {
                return await SetInternalDailyIndexRefreshTime(t.ToString());
            }
            return false;
        }
        /// <summary>
        /// Setzen der Zeit, wann die lokale Musikbiliothek aktualisiert werden soll
        /// </summary>
        /// <param name="rt">TimeSpan</param>
        public async Task<Boolean> SetDailyIndexRefreshTime(TimeSpan rt)
        {
            return await SetInternalDailyIndexRefreshTime(rt.ToString());
        }
        /// <summary>
        /// Interne verarbeitung
        /// </summary>
        /// <param name="rt"></param>
        /// <returns></returns>
        private async Task<Boolean> SetInternalDailyIndexRefreshTime(string rt)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("DesiredDailyIndexRefreshTime", rt);
            return await Invoke("SetDailyIndexRefreshTime", arguments);
        }

        #endregion RefreshTime
        /// <summary>
        /// Liefert das aktuelle Zeitformat in Timeformat Klasse
        /// </summary>
        /// <returns>TimeFormat</returns>
        public async Task<DateFormat> GetFormat()
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("CurrentTimeFormat", null);
            arguments[1] = new UPnPArgument("CurrentDateFormat", null);
            await Invoke("GetFormat", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            return new DateFormat() { Time = arguments[0].DataValue.ToString(), Date = arguments[1].DataValue.ToString() };
        }
        /// <summary>
        /// Setzen des Zeitformats
        /// </summary>
        /// <param name="tf">New Timeformart</param>
        /// <param name="pl">Any SonosPlayer</param>
        public async Task<Boolean> SetFormat(DateFormat tf)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("DesiredTimeFormat", tf.Time);
            arguments[1] = new UPnPArgument("DesiredDateFormat", tf.Date);
            return await Invoke("SetFormat", arguments);
        }

        public async Task<String> GetHouseholdTimeAtStamp(string TimeStamp)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("TimeStamp", TimeStamp);
            arguments[1] = new UPnPArgument("HouseholdUTCTime", null);
            await Invoke("GetHouseholdTimeAtStamp", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            return arguments[1].DataValue.ToString();
        }

        /// <summary>
        /// Liefert die Zeit in verschiedenen Zeiteinheiten.
        /// </summary>
        public async Task<CurrentTime> GetTimeNow(int sleep = 300)
        {
            var arguments = new UPnPArgument[4];
            arguments[0] = new UPnPArgument("CurrentUTCTime", null);
            arguments[1] = new UPnPArgument("CurrentLocalTime", null);
            arguments[2] = new UPnPArgument("CurrentTimeZone", null);
            arguments[3] = new UPnPArgument("CurrentTimeGeneration", null);
            await Invoke("GetTimeNow", arguments, sleep);
            await ServiceWaiter.WaitWhileAsync(arguments, 3, 100, 3, WaiterTypes.String);
            CurrentTime ct = new() { CurrentUTCTime = arguments[0].DataValue.ToString(), CurrentLocalTime = arguments[1].DataValue.ToString(), CurrentTimeZone = arguments[2].DataValue.ToString() };
            int ctg = 0;
            try
            {
                _ = int.TryParse(arguments[3].DataValue.ToString(), out ctg);
            }
            catch
            {
                //ignore
            }
            ct.CurrentTimeGeneration = ctg;
            ct.TimeZoneData = SonosTimeZone.FillSonosTimeZoneData(new SonosTimeZoneData { InternalString = ct.CurrentTimeZone });
            return ct;
        }
        /// <summary>
        /// Liefert den Aktuellen Zeit Server
        /// </summary>
        /// <returns></returns>
        public async Task<String> GetTimeServer()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("CurrentTimeServer", null);
            await Invoke("GetTimeServer", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
            return arguments[0].DataValue.ToString();
        }
        /// <summary>
        /// Liefert eine ID der Zeitzone und ob die Zeit automatisch an die Lokale Sommer-/Winterzeit angepasst werden soll.
        /// </summary>
        public async Task<SonosTimeZoneData> GetTimeZone()
        {
            try
            {
                var arguments = new UPnPArgument[2];
                arguments[0] = new UPnPArgument("Index", null);
                arguments[1] = new UPnPArgument("AutoAdjustDst", null);
                await Invoke("GetTimeZone", arguments, 100);
                await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
                SonosTimeZoneData stzd = new();
                if (!int.TryParse(arguments[0].DataValue.ToString(), out int id))
                {
                    id = -1; //Bei Fehler für die Darstellung weil de RIdnex bei 0 anfängt.
                }
                if (Boolean.TryParse(arguments[1].DataValue.ToString(), out bool autoadjust))
                    stzd.AutoAdjustDst = autoadjust;
                stzd.ID = id;

                SonosTimeZone.FillSonosTimeZoneData(stzd);
                return stzd;
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("GetTimeZone", ClassName, ex);
                return new SonosTimeZoneData();
            }
        }
        /// <summary>
        /// Liefert die TimeZone ID und eine Rule
        /// </summary>
        public async Task<SonosTimeZoneData> GetTimeZoneAndRule()
        {
            try
            {
                var arguments = new UPnPArgument[3];
                arguments[0] = new UPnPArgument("Index", null);
                arguments[1] = new UPnPArgument("AutoAdjustDst", null);
                arguments[2] = new UPnPArgument("CurrentTimeZone", null);
                await Invoke("GetTimeZoneAndRule", arguments, 100);
                await ServiceWaiter.WaitWhileAsync(arguments, 0, 100, 10, WaiterTypes.String);
                SonosTimeZoneData stzd = new();
                if (!int.TryParse(arguments[0].DataValue.ToString(), out int id))
                {
                    id = -1; //Bei Fehler für die Darstellung weil de RIdnex bei 0 anfängt.
                }
                if (Boolean.TryParse(arguments[1].DataValue.ToString(), out bool autoadjust))
                    stzd.AutoAdjustDst = autoadjust;
                stzd.ID = id;
                stzd.ExternalString = arguments[2].DataValue.ToString();
                SonosTimeZone.FillSonosTimeZoneData(stzd);
                return stzd;
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("GetTimeZoneAndRule", ClassName, ex);
                return new SonosTimeZoneData();
            }
        }
        /// <summary>
        /// Liefert zu einer TimeZone ID die entsprechende Rule
        /// </summary>
        public async Task<String> GetTimeZoneRule(uint index)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("Index", index);
            arguments[1] = new UPnPArgument("TimeZone", null);
            await Invoke("GetTimeZoneRule", arguments, 100);
            await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
            return arguments[0].DataValue.ToString();
        }
        /// <summary>
        /// Setzt die aktuelle Zeit.
        /// </summary>
        /// <param name="time">Zeit im ISO8601 Format (YYYY-MM-DD HH:MM:SS)</param>
        /// <param name="zone">Zone ExternalString von SonosTimeZoneData. 
        /// Nutze für die Liste SonosTimeZone.GetListOfTimeZones</param>
        /// <returns></returns>
        public async Task<Boolean> SetTimeNow(string time, string zone)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("DesiredTime", time);
            arguments[1] = new UPnPArgument("TimeZoneForDesiredTime", zone);
            return await Invoke("SetTimeNow", arguments);
        }
        /// <summary>
        /// Setzt den Zeitserver
        /// </summary>
        /// <param name="timeserver">z.B. fritz.box</param>
        /// <returns></returns>
        public async Task<Boolean> SetTimeServer(string timeserver)
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("DesiredTimeServer", timeserver);
            return await Invoke("SetTimeServer", arguments);
        }
        /// <summary>
        /// Setzen der Zeitzone inkl. Angabe für Sommerzeit
        /// </summary>
        /// <param name="pl">SonosPlayer</param>
        /// <param name="index">Index</param>
        /// <param name="sommerzeit">Soll Sommerzeit gesetzt werden?</param>
        /// <returns></returns>
        public async Task<Boolean> SetTimeZone(uint index, Boolean sommerzeit)
        {
            var arguments = new UPnPArgument[2];
            arguments[0] = new UPnPArgument("Index", index);
            arguments[1] = new UPnPArgument("AutoAdjustDst", sommerzeit);
            return await Invoke("SetTimeZone", arguments);
        }
        #endregion Clock
        #region private Methoden
        /// <summary>
        /// Macht den eigentlichen Invoke in den Service.
        /// </summary>
        /// <param name="Method"></param>
        /// <param name="arguments"></param>
        /// <param name="Sleep"></param>
        /// <returns></returns>
        private async Task<Boolean> Invoke(String Method, UPnPArgument[] arguments, int Sleep = 0)
        {
            try
            {
                if (AlarmClockService == null)
                {
                    pl.ServerErrorsAdd(Method, ClassName, new Exception(Method + " " + ClassName + " ist null"));
                    return false;
                }
                AlarmClockService.InvokeAsync(Method, arguments);
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
                if (AlarmClock_Changed == null) return;
                LastChangeByEvent = _lastchange;
                LastChangeDates[t] = _lastchange;
                AlarmClock_Changed(this, pl);
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("AlarmClock_Changed", ClassName, ex);
            }
        }
        #endregion private Methoden
    }
}





