using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Sonos.Classes;
using SonosUPnP;
using SonosUPnP.DataClasses;
using SonosUPnP.Props;
using SonosConst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Sonos.Controllers
{
    [Route("/[controller]")]
    public class ZoneController : Controller
    {
        private Boolean getzonesRunning = false;
        public ZoneController(IConfiguration iConfig)
        {
            SonosConstants.Configuration = iConfig;
        }
        /// <summary>
        /// Ermittelt alle Playlists, die es gibt. 
        /// Importierte und Sonos
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetPlaylists")]
        public async Task<IList<SonosItem>> GetPlaylists()
        {
            try
            {
                if (!await SonosHelper.CheckSonosLiving()) return null;
                if (SonosHelper.Sonos.CheckPlaylists())
                {
                    await SonosHelper.Sonos.SetPlaylists(true);
                }
                return SonosHelper.Sonos.ZoneProperties.ListOfAllPlaylist;
            }
            catch (Exception x)
            {
                SonosHelper.Logger.ServerErrorsAdd("GetPlaylists", x);
                return new List<SonosItem> { new SonosItem { Title = "Exception" }, new SonosItem { Title = x.Message } };
            }
        }
        /// <summary>
        /// Liefert alle gespeicherten Favoriten.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
       [HttpGet("GetFavorites")]
        public async Task<IList<SonosItem>> GetFavorites()
        {
            if (!await SonosHelper.CheckSonosLiving() || SonosHelper.Sonos.Players.Count == 0) return new List<SonosItem>();
            if (SonosHelper.Sonos.CheckPlaylists())
                await SonosHelper.Sonos.SetPlaylists();
            return SonosHelper.Sonos.ZoneProperties.ListOfFavorites;
        }
        /// <summary>
        /// Liefert die ServerErrorliste
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
       [HttpGet("GetServerErrorList")]
        public Dictionary<String, String> GetServerErrorList()
        {
            return SonosHelper.serverErrors;
        }
        /// <summary>
        /// Liefert die Globalen Einstellungen zurück. 
        /// </summary>
        /// <returns></returns>
       [HttpGet("GetZoneProp")]
        public async Task<DiscoveryZoneProperties> GetZoneProp()
        {
            if (!await SonosHelper.CheckSonosLiving()) return null;
            return SonosHelper.Sonos.ZoneProperties;
        }
        /// <summary>
        /// Gibt alle Zonen zurück
        /// </summary>
        /// <returns></returns>
       [HttpGet("GetZones")]
        public async Task<ZoneGroupStateList> GetZones()
        {
            try
            {
                if (!await SonosHelper.CheckSonosLiving()) return null;
                if (SonosHelper.Sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates.Count != SonosHelper.Sonos.Players.Count && !getzonesRunning)
                {
                    getzonesRunning = true;
                    //Hier komme ich rein, wenn es weniger Zonen als Player gibt. Was ok ist, wenn es gruppen gibt. 
                    int calculatedzones = 0;
                    foreach (SonosPlayer pl in SonosHelper.Sonos.Players)
                    {
                        if (pl.PlayerProperties.GroupCoordinatorIsLocal)
                        {
                            calculatedzones += 1;
                            continue;
                        }
                    }
                    if (calculatedzones != SonosHelper.Sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates.Count)
                    {
                        //Hier komme ich rein, wenn es eine unstimmigkeit gibt
                        if (calculatedzones < SonosHelper.Sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates.Count)
                            SonosHelper.Sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates.Clear();

                        //Need Update
                        var s1 = SonosHelper.Sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates.FirstOrDefault(x => x.SoftwareGeneration ==  SonosUPNPCore.Enums.SoftwareGeneration.ZG1);
                        if (s1 == null)
                        {
                            //Update for 1
                            SonosPlayer pl = await SonosHelper.GetPlayerbySoftWareGeneration(SonosUPNPCore.Enums.SoftwareGeneration.ZG1);
                            var k = await pl.ZoneGroupTopology.GetZoneGroupState();
                            SonosHelper.Sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates.AddRange(k.ZoneGroupStates);
                        }
                        var s2 = SonosHelper.Sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates.FirstOrDefault(x => x.SoftwareGeneration ==  SonosUPNPCore.Enums.SoftwareGeneration.ZG2);
                        if (s2 == null)
                        {
                            //Update for 2
                            SonosPlayer pl = await SonosHelper.GetPlayerbySoftWareGeneration(SonosUPNPCore.Enums.SoftwareGeneration.ZG2);
                            var k = await pl.ZoneGroupTopology.GetZoneGroupState();
                            lock (SonosHelper.Sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates)
                            {
                                SonosHelper.Sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates.AddRange(k.ZoneGroupStates);
                            }
                        }
                        await SonosHelper.Sonos.SetPlaylists(true);
                    }
                    getzonesRunning = false;
                }
                //Sort Test
                ZoneGroupStateList t = new();
                t.ZoneGroupStates = SonosHelper.Sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates.OrderBy(x => x.ZoneGroupMember.First().ZoneName).ToList();
                SonosHelper.Sonos.ZoneProperties.ZoneGroupState = t;
                return SonosHelper.Sonos.ZoneProperties.ZoneGroupState;
            }
            catch(Exception ex)
            {
                SonosHelper.Logger.ServerErrorsAdd("GetZones", ex, "ZoneController");
                return SonosHelper.Sonos.ZoneProperties.ZoneGroupState;
            }
            //return SonosHelper.Sonos.ZoneProperties.ZoneGroupState;
        }
       [HttpGet("SetPlaylists")]
        public async Task<String> SetPlaylists()
        {
            string retval = "ok";
            try
            {
                if (!await SonosHelper.CheckSonosLiving()) return "Checkliving Error";
                await SonosHelper.Sonos?.SetPlaylists();
            }
            catch (Exception ex)
            {
                SonosHelper.Logger.ServerErrorsAdd("SetPlaylists", ex, "ZoneController");
                throw;
            }
            return retval;
        }
        [HttpGet("CheckPlayersForHashImages")]
        public async Task<String> CheckPlayersForHashImages()
        {
            string retval = "ok";
            try
            {
                if (!await SonosHelper.CheckSonosLiving()) return "Checkliving Error";
                await SonosHelper.CheckPlayerForHashImages(SonosHelper.Sonos.Players);
            }
            catch (Exception ex)
            {
                SonosHelper.Logger.ServerErrorsAdd("CheckPlayersForHashImages", ex, "ZoneController");
                throw;
            }
            return retval;
        }

        [HttpGet("FillAllPlayerProperties")]
        public async Task<String> FillAllPlayerProperties()
        {
            string retval = "ok";
            try
            {
                if (!await SonosHelper.CheckSonosLiving()) return "Checkliving Error";
                await SonosHelper.FillAllPlayerProperties();
            }
            catch (Exception ex)
            {
                SonosHelper.Logger.ServerErrorsAdd("FillAllPlayerProperties", ex, "ZoneController");
                throw;
            }
            return retval;
        }
        [HttpGet("CheckDevicesToPlayer")]
        public async Task<String> CheckDevicesToPlayer()
        {
            string retval = "ok";
            try
            {
                if (!await SonosHelper.CheckSonosLiving()) return "Checkliving Error";
                SonosHelper.Sonos.CheckDevicesToPlayer();
            }
            catch (Exception ex)
            {
                SonosHelper.Logger.ServerErrorsAdd("CheckDevicesToPlayer", ex, "ZoneController");
                throw;
            }
            return retval;
        }
    }
}
