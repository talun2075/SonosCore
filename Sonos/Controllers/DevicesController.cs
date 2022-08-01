using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sonos.Classes;
using SonosUPnP;
using Sonos.Classes.Interfaces;
using HomeLogging;
using Sonos.Classes.Enums;
using SonosData.DataClasses;
using SonosUPNPCore.Interfaces;

namespace Sonos.Controllers
{
    [Route("/[controller]")]
    public class DevicesController : Controller
    {
        #region Klassenvariablen

        private readonly Dictionary<string, DateTime> _playersLastChange = new();
        private readonly ILogging _logger;
        private readonly ISonosHelper _sonosHelper;
        private readonly ISonosDiscovery _sonos;
        #endregion KlassenVariablen
        public DevicesController(ISonosHelper sonosHelper, ILogging log, ISonosDiscovery sonos)
        {
            _sonosHelper = sonosHelper;
            _logger = log;
            _sonos = sonos;
        }
        #region Public Methoden
        /// <summary>
        /// Initialisierung des ganzen
        /// </summary>
        /// <returns></returns>
        [HttpGet("Get")]
        public string Get()
        {
            try
            {
                return "Ready";
            }
            catch (Exception ex)
            {
                _logger.ServerErrorsAdd("DeviceGetError", ex);
                throw;
            }
        }
        /// <summary>
        /// Liefert eine Liste Aller Player
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetPlayers")]
        public IList<SonosPlayer> GetPlayers()
        {
            return _sonos.Players;
        }
        /// <summary>
        /// Liefert einen definierten Player
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetPlayer/{id}")]
        public SonosPlayer GetPlayer(string id)
        {
            return _sonos.GetPlayerbyUuid(id);
        }

        [HttpGet("GetLongPlayer/{id}")]
        public SonosPlayer GetLongPlayer(string id)
        {
            if(_sonosHelper.CheckAllPlayerReachable())
                return _sonos.GetPlayerbyUuid(id);
            throw new Exception("System Reset, weil Player nicht erreichbar");
        }

        /// <summary>
        /// Liefert eine Liste dem Zeitpunkt der letzten Änderung eines jeden Players
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetLastChangesDateTimes")]
        public Dictionary<string, DateTime> GetLastChangesDateTimes()
        {
            _playersLastChange.Clear();
            foreach (SonosPlayer item in _sonos.Players)
            {
                    _playersLastChange.Add(item.UUID, item.LastChange);
            }
            //Das erst hier, weil dann das Web schon initialisiert wurde. 
            Debug.WriteLine("GetLastChangesDateTimes wurde aufgerufen.");
            _sonos.CheckDevicesToPlayer();
            return _playersLastChange;
        }
        #endregion Public Methoden
        [HttpGet("GetPlayerNamesAndUUID")]
        public Dictionary<String, String> GetPlayerNamesAndUUID()
        {
            Dictionary<String, String> result = new();
            foreach (SonosPlayer sp in _sonos.Players)
            {
                if (!result.ContainsKey(sp.Name))
                    result.Add(sp.Name, sp.UUID);
            }
            return result;
        }

        [HttpGet("CheckPlayerReachable")]
        public Boolean CheckPlayerReachable()
        {
            return _sonosHelper.CheckAllPlayerReachable();
        }
        [HttpGet("GetPlayerProperties/{id}")]
        public async Task<PlayerDeviceProperties> GetPlayerProperties(string id)
        {
            return await new PlayerDeviceProperties().FilledData(_sonos.GetPlayerbyUuid(id));
        }

        [HttpPost("SetPlayerProperties")]
        public async Task<ActionResult> SetPlayerProperties(PlayerPropertiesRequest playerPropertiesRequest)
        {
            try
            {
                if (playerPropertiesRequest == null) return BadRequest("Übergabe Null");
                SonosPlayer player = _sonos.GetPlayerbyUuid(playerPropertiesRequest.uuid);
                if (player == null) return BadRequest("Kein Player Gefunden");
                Boolean retval = false;
                switch (playerPropertiesRequest.type)
                {
                    case PlayerDevicePropertiesTypes.ButtonLockState:
                        if (playerPropertiesRequest.value == "true")
                        {
                            retval = await player.DeviceProperties.SetButtonLockState(SonosEnums.OnOff.On);
                        }
                        else
                        {
                            retval = await player.DeviceProperties.SetButtonLockState(SonosEnums.OnOff.Off);
                        }
                        break;
                    case PlayerDevicePropertiesTypes.LEDState:
                        if (playerPropertiesRequest.value == "true")
                        {
                            retval = await player.DeviceProperties.SetLEDState(SonosEnums.OnOff.On);
                        }
                        else
                        {
                            retval = await player.DeviceProperties.SetLEDState(SonosEnums.OnOff.Off);
                        }
                        break;
                    case PlayerDevicePropertiesTypes.Loudness:
                        if (playerPropertiesRequest.value == "true")
                        {
                            retval = await player.RenderingControl.SetLoudness();
                        }
                        else
                        {
                            retval = await player.RenderingControl.SetLoudness(false);
                        }
                        break;
                    case PlayerDevicePropertiesTypes.OutputFixed:
                        if (!player.PlayerProperties.SupportOutputFixed) return BadRequest("Player Supporte kein OutputFixed");
                        if (playerPropertiesRequest.value == "true")
                        {
                            retval = await player.RenderingControl.SetOutputFixed();
                        }
                        else
                        {
                            retval = await player.RenderingControl.SetOutputFixed(false);
                        }
                        break;
                    case PlayerDevicePropertiesTypes.Bass:
                        if (Int16.TryParse(playerPropertiesRequest.value, out Int16 bass))
                            retval = await player.RenderingControl.SetBass(bass);
                        break;
                    case PlayerDevicePropertiesTypes.Treble:
                        if (Int16.TryParse(playerPropertiesRequest.value, out Int16 treble))
                            retval = await player.RenderingControl.SetTreble(treble);
                        break;
                }

                return Ok(retval);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }
        #region PrivateMethoden

        #endregion PrivateMethoden
    }
}
