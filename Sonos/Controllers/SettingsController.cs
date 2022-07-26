using Sonos.Classes;
using System;
using SonosUPnP;
using System.Linq;
using SonosUPnP.DataClasses;
using System.Collections.Generic;
using SonosUPnP.Props;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SonosConst;

namespace Sonos.Controllers
{
    [Route("/[controller]")]
    public class SettingsController : Controller
    {
        #region Klassenvariablen
        private const int newAlarmID = 99999;
        #endregion Klassenvariablen
        public SettingsController(IConfiguration iConfig)
        {
            SonosConstants.Configuration = iConfig;
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
                if (SonosHelper.Sonos.ZoneProperties.ListOfAlarms.Count == 0)
                {
                    await SonosHelper.Sonos.GetSonosTimeStuff();
                }
                return SonosHelper.Sonos.ZoneProperties.ListOfAlarms;
            }
            catch (Exception ex)
            {
                SonosHelper.Logger.ServerErrorsAdd("GetAlarms", ex);
                throw;
            }
        }
        /// <summary>
        /// Editiert einen Alarm oder setzt diesen neu.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        [HttpPost("SetAlarm")]
        public async Task<Boolean> SetAlarm([FromForm] Alarm v)
        {
            try
            {
                Boolean metaChange = false;
                SonosPlayer pl = await SonosHelper.GetPlayerbyUuid(v.RoomUUID);
                if (pl == null) return false;
                Alarm alarm = new();
                if (v.ID != newAlarmID)
                {
                    //Vorhandener Alarm
                    if (SonosHelper.Sonos.ZoneProperties.ListOfAlarms.Count == 0)
                    {
                        await SonosHelper.Sonos.GetSonosTimeStuff();
                    }
                    Alarm knowedalarm = SonosHelper.Sonos.ZoneProperties.ListOfAlarms.FirstOrDefault(x => x.ID == v.ID);
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
                await SonosHelper.Sonos.GetSonosTimeStuff();
                return true;
            }
            catch (Exception ex)
            {
                SonosHelper.Logger.ServerErrorsAdd("SetAlarm", ex);
                throw;
            }
        }
        /// <summary>
        /// Löscht einen Alarm
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        [HttpPost("DestroyAlarm")]
        public async Task<Boolean> DestroyAlarm([FromForm] string v)
        {
            try
            {
                Alarm al;
                if (!int.TryParse(v, out int alarmid)) return false;
                if (SonosHelper.Sonos.ZoneProperties.ListOfAlarms.Count == 0)
                {
                    await SonosHelper.Sonos.GetSonosTimeStuff();
                }
                al = SonosHelper.Sonos.ZoneProperties.ListOfAlarms.FirstOrDefault(alarm => alarm.ID == alarmid);//Alarm ermitteln und löschen.
                if (al == null) return false;
                SonosPlayer pl = SonosHelper.Sonos.Players.FirstOrDefault(x => x.UUID == al.RoomUUID);
                if (await pl?.AlarmClock?.DestroyAlarm(al))
                {
                    SonosHelper.Sonos.ZoneProperties.ListOfAlarms.Remove(al);
                    //SonosHelper.Sonos.GetSonosTimeStuff();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                SonosHelper.Logger.ServerErrorsAdd("DestroyAlarms", ex);
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
                if (!await SonosHelper.CheckSonosLiving()) return null;
                if (string.IsNullOrEmpty(v) || !Boolean.TryParse(v, out bool ena) || !int.TryParse(id, out int alarmind))
                {
                    return false;
                }
                if (SonosHelper.Sonos.ZoneProperties.ListOfAlarms.Count == 0)
                {
                    await SonosHelper.Sonos.GetSonosTimeStuff();
                }
                Alarm al = SonosHelper.Sonos.ZoneProperties.ListOfAlarms.FirstOrDefault(x => x.ID == alarmind && x.Enabled != ena);
                if (al != null)
                {
                    //Es wurde die ID gefunden und diese ist anders als aktuell benötigt
                    al.Enabled = ena;
                    SonosPlayer pl = SonosHelper.Sonos.Players.FirstOrDefault(x => x.UUID == al.RoomUUID);
                    return await pl?.AlarmClock?.UpdateAlarm(al);
                }
                //ID nicht gefunden oder Zustand nicht geändert daher false
                return false;
            }
            catch (Exception ex)
            {
                SonosHelper.Logger.ServerErrorsAdd("AlarmEnable", ex);
                throw;
            }
        }
        #endregion Alarm
        /// <summary>
        /// Liefert die globalen Eigenschaften des SonosSystems zurück
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetSonosSettings")]
        public async Task<DiscoveryZoneProperties> GetSonosSettings()
        {
            if (!await SonosHelper.CheckSonosLiving()) throw new Exception("CheckSonosLiving Error");
            await SonosHelper.FillSonosTimeSettingStuff();
            return SonosHelper.Sonos?.ZoneProperties;
        }

        [HttpGet("FillSonosTimeSettingStuff")]
        public async Task<String> FillSonosTimeSettingStuff()
        {
            string retval = "ok";
            try
            {
                if (!await SonosHelper.CheckSonosLiving()) return "Checkliving Error";
                await SonosHelper.FillSonosTimeSettingStuff();
            }
            catch (Exception ex)
            {
                SonosHelper.Logger.ServerErrorsAdd("FillSonosTimeSettingStuff", ex, "SetrtingsController");
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
                if (!await SonosHelper.CheckSonosLiving()) return false;
                if (SonosHelper.Sonos.ZoneProperties.ShareIndexInProgress) return true;
                await SonosHelper.Sonos.UpdateMusicIndex();
                await Task.Delay(2000);
                _ = MusicPictures.GenerateDBContent();
                SonosHelper.ChildGenrelist.Clear();//Liste der Hörspiele leeren; Generieren erfolgt über Timer.
                return SonosHelper.Sonos.ZoneProperties.ShareIndexInProgress;
            }
            catch (Exception ex)
            {
                SonosHelper.Logger.ServerErrorsAdd("SetUpdateMusicIndex", ex);
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
            return SonosHelper.Sonos?.ZoneProperties?.ShareIndexInProgress;
        }
        [HttpGet("GetLoggingServerErrors")]
        public Dictionary<String, String> GetLoggingServerErrors()
        {
            return SonosHelper.Logger.ServerErrors;
        }
    }
}
