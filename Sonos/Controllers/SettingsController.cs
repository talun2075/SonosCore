using System;
using SonosUPnP;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sonos.Classes.Interfaces;
using HomeLogging;
using SonosData.DataClasses;
using SonosUPNPCore.Classes;
using SonosData;
using SonosConst;
using SonosSQLiteWrapper.Interfaces;
using SonosUPNPCore.Interfaces;

namespace Sonos.Controllers
{
    [Route("/[controller]")]
    public class SettingsController : Controller
    {
        #region Klassenvariablen
        private const int newAlarmID = 99999;
        private readonly IMusicPictures musicPictures;
        private readonly ILogging _logger;
        private readonly ISonosHelper _sonosHelper;
        private readonly ISonosDiscovery _sonos;
        #endregion Klassenvariablen
        public SettingsController(IMusicPictures imu, ISonosHelper sonosHelper, ILogging log, ISonosDiscovery sonos)
        {
            musicPictures = imu;
            _logger = log;
            _sonosHelper = sonosHelper;
            _sonos = sonos;
        }
        #region Alarm
        /// <summary>
        /// Liefert alle Wecker als Liste
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetAlarms")]
        public async Task<IList<Alarm>> GetAlarms()
        {
            try
            {
                await _sonos.GetSonosTimeStuff();
                return _sonos.Zone.Properties.ListOfAlarms;
            }
            catch (Exception ex)
            {
                _logger.ServerErrorsAdd("GetAlarms", ex);
                throw;
            }
        }
        /// <summary>
        /// Editiert einen Alarm oder setzt diesen neu.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        [HttpPost("SetAlarm")]
        public async Task<Boolean> SetAlarm([FromBody] Alarm v)
        {
            try
            {
                Boolean metaChange = false;
                SonosPlayer pl = _sonos.GetPlayerbyUuid(v.RoomUUID);
                if (pl == null) return false;
                Alarm alarm = new();
                if (v.ID != newAlarmID)
                {
                    //Vorhandener Alarm
                    await _sonos.GetSonosTimeStuff();
                    Alarm knowedalarm = _sonos.Zone.Properties.ListOfAlarms.FirstOrDefault(x => x.ID == v.ID);
                    if (knowedalarm.ContainerID != v.ContainerID) metaChange = true;
                    alarm = v;
                    if (knowedalarm == null)
                    {
                        alarm.ID = newAlarmID;
                        metaChange = true;
                    }
                }
                else
                {
                    alarm = v;
                    metaChange = true;
                }
                //Hier nun die MetaDaten lesen, wenn Änderung.
                if (metaChange)
                {
                    alarm.ProgramMetaData = "<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns:r=\"urn:schemas-rinconnetworks-com:metadata-1-0/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\"><item id=\"" +
                    alarm.ContainerID + "\" parentID=\"A:PLAYLISTS\" restricted=\"true\"><dc:title>" + alarm.ProgramURIShort +
                    "</dc:title><upnp:class>object.container.playlistContainer</upnp:class><desc id=\"cdudn\" nameSpace=\"urn:schemas-rinconnetworks-com:metadata-1-0/\">RINCON_AssociatedZPUDN</desc></item></DIDL-Lite>";
                }
                if (alarm.ID == newAlarmID)
                {
                    await pl.AlarmClock.CreateAlarm(alarm);
                }
                else
                {
                    await pl.AlarmClock.UpdateAlarm(alarm);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.ServerErrorsAdd("SetAlarm", ex);
                throw;
            }
        }
        /// <summary>
        /// Löscht einen Alarm
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        [HttpGet("DestroyAlarm/{id}")]
        public async Task<Boolean> DestroyAlarm(string id)
        {
            try
            {
                Alarm al;
                if (!int.TryParse(id, out int alarmid)) return false;
                 await _sonos.GetSonosTimeStuff();

                al = _sonos.Zone.Properties.ListOfAlarms.FirstOrDefault(alarm => alarm.ID == alarmid);//Alarm ermitteln und löschen.
                if (al == null) return false;
                SonosPlayer pl = _sonos.Players.FirstOrDefault(x => x.UUID == al.RoomUUID);
                if (await pl?.AlarmClock?.DestroyAlarm(al))
                {
                    _sonos.Zone.Properties.ListOfAlarms.Remove(al);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.ServerErrorsAdd("DestroyAlarms", ex);
                throw;
            }
        }

        /// <summary>
        /// Ändert den Aktivierungsstatus eines Weckers
        /// </summary>
        /// <param name="v">ID des Weckers</param>
        /// <param name="v2">Bool an/aus</param>
        /// <returns></returns>
        [HttpGet("AlarmEnable/{id}/{v}")]
        public async Task<Boolean?> AlarmEnable(string id, string v)
        {
            try
            {
                if (string.IsNullOrEmpty(v) || !Boolean.TryParse(v, out bool ena) || !int.TryParse(id, out int alarmind))
                {
                    return false;
                }
                await _sonos.GetSonosTimeStuff();

                Alarm al = _sonos.Zone.Properties.ListOfAlarms.FirstOrDefault(x => x.ID == alarmind && x.Enabled != ena);
                if (al != null)
                {
                    //Es wurde die ID gefunden und diese ist anders als aktuell benötigt
                    al.Enabled = ena;
                    SonosPlayer pl = _sonos.Players.FirstOrDefault(x => x.UUID == al.RoomUUID);
                    return await pl?.AlarmClock?.UpdateAlarm(al);
                }
                //ID nicht gefunden oder Zustand nicht geändert daher false
                return false;
            }
            catch (Exception ex)
            {
                _logger.ServerErrorsAdd("AlarmEnable", ex);
                throw;
            }
        }
        #endregion Alarm
        /// <summary>
        /// Liefert die globalen Eigenschaften des SonosSystems zurück
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetSonosSettings")]
        public async Task<Zone> GetSonosSettings()
        {
            await _sonosHelper.FillSonosTimeSettingStuff();
            return _sonos.Zone;
        }
        [HttpGet("SetSettings")]
        public async Task<String> SetSettings()
        {
            string retval = "ok";
            try
            {
                await _sonos.SetSettings();
            }
            catch (Exception ex)
            {
                _logger.ServerErrorsAdd("SetSettings", ex, "ZoneController");
                throw;
            }
            return retval;
        }
        [HttpGet("FillSonosTimeSettingStuff")]
        public async Task<String> FillSonosTimeSettingStuff()
        {
            string retval = "ok";
            try
            {
                await _sonosHelper.FillSonosTimeSettingStuff();
            }
            catch (Exception ex)
            {
                _logger.ServerErrorsAdd("FillSonosTimeSettingStuff", ex, "SettingsController");
                throw;
            }
            return retval;
        }

        /// <summary>
        /// Aktualisiert den Musik index
        /// </summary>
        /// <param name="id"></param>
        [HttpGet("SetUpdateMusicIndex")]
        public async Task<Boolean> SetUpdateMusicIndex()
        {
            try
            {
                if (_sonos.Zone.Properties.ShareIndexInProgress) return true;
                await _sonos.UpdateMusicIndex();
                List<SonosItem> tracks = await _sonos.ZoneMethods.Browsing(_sonos.Players.First(), SonosConstants.aTracks, false);
                musicPictures.GenerateDBContent(tracks);
                _sonosHelper.ChildGenrelist.Clear();//Liste der Hörspiele leeren; Generieren erfolgt über Timer.
                return _sonos.Zone.Properties.ShareIndexInProgress;
            }
            catch (Exception ex)
            {
                _logger.ServerErrorsAdd("SetUpdateMusicIndex", ex);
                throw;
            }
        }
        /// <summary>
        /// Wird der Musikindex gerade AKtualisiert?
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetUpdateIndexInProgress")]
        public Boolean? GetUpdateIndexInProgress()
        {
            return _sonos.Zone?.Properties.ShareIndexInProgress;
        }
        [HttpGet("GetLoggingServerErrors")]
        public Dictionary<String, String> GetLoggingServerErrors()
        {
            return _logger.ServerErrors;
        }
    }
}
