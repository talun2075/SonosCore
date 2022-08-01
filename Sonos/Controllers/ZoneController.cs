using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Sonos.Classes;
using SonosUPnP;
using SonosConst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeLogging;
using Sonos.Classes.Interfaces;
using SonosData.DataClasses;
using SonosUPNPCore.Classes;
using SonosData;
using SonosData.Enums;
using SonosUPNPCore.Interfaces;

namespace Sonos.Controllers
{
    [Route("/[controller]")]
    public class ZoneController : Controller
    {
        private Boolean getzonesRunning = false;
        private readonly ILogging _logger;
        private readonly ISonosHelper _sonosHelper;
        private readonly ISonosDiscovery _sonos;
        public ZoneController(ISonosHelper sonosHelper, ILogging log, ISonosDiscovery sonos)
        {
            _logger = log;
            _sonosHelper = sonosHelper;
            _sonos = sonos;
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
                if (_sonos.CheckPlaylists())
                {
                    await _sonos.SetPlaylists(true);
                }
                return _sonos.ZoneProperties.ListOfAllPlaylist;
            }
            catch (Exception x)
            {
                _logger.ServerErrorsAdd("GetPlaylists", x);
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
            if (!_sonos.Players.Any()) return new List<SonosItem>();
            if (_sonos.CheckPlaylists())
                await _sonos.SetPlaylists();
            return _sonos.ZoneProperties.ListOfFavorites;
        }
        /// <summary>
        /// Liefert die Globalen Einstellungen zurück. 
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetZoneProp")]
        public DiscoveryZoneProperties GetZoneProp()
        {
            return _sonos.ZoneProperties;
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
                if (_sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates.Count != _sonos.Players.Count && !getzonesRunning)
                {
                    try
                    {
                        getzonesRunning = true;
                        //Hier komme ich rein, wenn es weniger Zonen als Player gibt. Was ok ist, wenn es gruppen gibt. 
                        int calculatedzones = 0;
                        lock (_sonos.Players)
                        {
                            foreach (SonosPlayer pl in _sonos.Players)
                            {
                                if (pl.PlayerProperties.GroupCoordinatorIsLocal)
                                {
                                    calculatedzones += 1;
                                    continue;
                                }
                            }
                        }
                        if (calculatedzones != _sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates.Count)
                        {
                            //Hier komme ich rein, wenn es eine unstimmigkeit gibt
                            if (calculatedzones < _sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates.Count)
                                _sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates.Clear();

                            //Need Update
                            var s1 = _sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates.FirstOrDefault(x => x.SoftwareGeneration == SoftwareGeneration.ZG1);
                            if (s1 == null)
                            {
                                //Update for 1
                                SonosPlayer pl = _sonos.GetPlayerbySoftWareGeneration(SoftwareGeneration.ZG1);
                                var k = await pl.ZoneGroupTopology.GetZoneGroupState();
                                _sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates.AddRange(k.ZoneGroupStates);
                            }
                            var s2 = _sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates.FirstOrDefault(x => x.SoftwareGeneration == SoftwareGeneration.ZG2);
                            if (s2 == null)
                            {
                                //Update for 2
                                SonosPlayer pl = _sonos.GetPlayerbySoftWareGeneration(SoftwareGeneration.ZG2);
                                var k = await pl.ZoneGroupTopology.GetZoneGroupState();
                                lock (_sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates)
                                {
                                    _sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates.AddRange(k.ZoneGroupStates);
                                }
                            }
                            await _sonos.SetPlaylists(true);
                        }
                        getzonesRunning = false;
                    }
                    catch (Exception ex)
                    {
                        _logger.ServerErrorsAdd("GetZones:Block1", ex, "ZoneController");
                        return _sonos.ZoneProperties.ZoneGroupState;
                    }
                }
                try
                {
                    //Sort Test
                    ZoneGroupStateList t = new();
                    t.ZoneGroupStates = _sonos.ZoneProperties.ZoneGroupState.ZoneGroupStates.OrderBy(x => x.ZoneGroupMember.First().ZoneName).ToList();
                    _sonos.ZoneProperties.ZoneGroupState = t;
                }
                catch (Exception ex)
                {
                    _logger.ServerErrorsAdd("GetZones:Sort", ex, "ZoneController");
                    return _sonos.ZoneProperties.ZoneGroupState;
                }
                return _sonos.ZoneProperties.ZoneGroupState;
            }
            catch (Exception ex)
            {
                _logger.ServerErrorsAdd("GetZones", ex, "ZoneController");
                return _sonos.ZoneProperties.ZoneGroupState;
            }
        }
        [HttpGet("SetPlaylists")]
        public async Task<String> SetPlaylists()
        {
            string retval = "ok";
            try
            {
                await _sonos.SetPlaylists();
            }
            catch (Exception ex)
            {
                _logger.ServerErrorsAdd("SetPlaylists", ex, "ZoneController");
                throw;
            }
            return retval;
        }
        [HttpGet("CheckPlayersForHashImages")]
        public String CheckPlayersForHashImages()
        {
            Boolean retval = false;
            try
            {
                retval = _sonosHelper.CheckPlayerForHashImages(_sonos.Players);
            }
            catch (Exception ex)
            {
                _logger.ServerErrorsAdd("CheckPlayersForHashImages", ex, "ZoneController");
                throw;
            }
            return retval.ToString();
        }

        [HttpGet("FillAllPlayerProperties")]
        public async Task<String> FillAllPlayerProperties()
        {
            string retval = "ok";
            try
            {
                await _sonosHelper.FillAllPlayerProperties();
            }
            catch (Exception ex)
            {
                _logger.ServerErrorsAdd("FillAllPlayerProperties", ex, "ZoneController");
                throw;
            }
            return retval;
        }
        [HttpGet("CheckDevicesToPlayer")]
        public String CheckDevicesToPlayer()
        {
            string retval = "ok";
            try
            {
                _sonos.CheckDevicesToPlayer();
            }
            catch (Exception ex)
            {
                _logger.ServerErrorsAdd("CheckDevicesToPlayer", ex, "ZoneController");
                throw;
            }
            return retval;
        }
    }
}
