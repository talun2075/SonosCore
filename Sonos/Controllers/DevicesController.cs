using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Sonos.Classes;
using SonosUPnP;

namespace Sonos.Controllers
{
    [Route("/[controller]")]
    public class DevicesController : Controller
    {
        #region Klassenvariablen

        /// <summary>
        /// Alles was von den XFile Informationen gelöscht werden muß;
        /// </summary>
        public static string RemoveFromUri { get; set; } = SonosConstants.xfilecifs;
        private readonly Dictionary<string, DateTime> _playersLastChange = new();
        #endregion KlassenVariablen
        public DevicesController(IConfiguration iConfig)
        {
            SonosHelper.Configuration = iConfig;
        }
        #region Public Methoden
        /// <summary>
        /// Initialisierung des ganzen
        /// </summary>
        /// <returns></returns>
        [HttpGet("Get")]
        public async Task<string> Get()
        {
            try
            {
                await SonosHelper.Initialisierung();
                return "Ready";
            }
            catch (Exception ex)
            {
                SonosHelper.Logger.ServerErrorsAdd("DeviceGetError", ex);
                throw;
            }
        }
        /// <summary>
        /// Liefert eine Liste Aller Player
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetPlayers")]
        public async Task<IList<SonosPlayer>> GetPlayers()
        {
            if (SonosHelper.Sonos == null || SonosHelper.Sonos.Players.Count == 0)
            {
                await SonosHelper.Initialisierung();
            }
            if (SonosHelper.Sonos == null) return null;
            return SonosHelper.Sonos.Players;
        }
        [HttpGet("test")]
        public async Task<Boolean> Test()
        {
            string t = "<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns:r=\"urn:schemas-rinconnetworks-com:metadata-1-0/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\"><container id=\"SQ:90\" parentID=\"SQ:\" restricted=\"true\"><dc:title>zzzIanButton3</dc:title><res protocolInfo=\"file:*:audio/mpegurl:*\">file:///jffs/settings/savedqueues.rsq#90</res><upnp:class>object.container.playlistContainer</upnp:class><upnp:albumArtURI>/getaa?u=x-file-cifs%3a%2f%2fNAS%2fMusik%2fHoerspiele%2fYakari%2f01.03%2520Kleiner%2520Donner%2520rei%25c3%259ft%2520aus%2fKapitel%252001%2520001%2520Kleiner%2520Donner%2520rei%25c3%259ft%2520aus.mp3&amp;v=4877</upnp:albumArtURI><upnp:albumArtURI>/getaa?u=x-file-cifs%3a%2f%2fNAS%2fMusik%2fHoerspiele%2fYakari%2f01.03%2520Kleiner%2520Donner%2520rei%25c3%259ft%2520aus%2fKapitel%252003%2520001%2520Kleiner%2520Donner%2520rei%25c3%259ft%2520aus.mp3&amp;v=4877</upnp:albumArtURI><upnp:albumArtURI>/getaa?u=x-file-cifs%3a%2f%2fNAS%2fMusik%2fHoerspiele%2fYakari%2f01.04%2520Im%2520Land%2520der%2520W%25c3%25b6lfe%2fKapitel%252001%2520001%2520Im%2520Land%2520der%2520W%25c3%25b6lfe.mp3&amp;v=4877</upnp:albumArtURI><upnp:albumArtURI>/getaa?u=x-file-cifs%3a%2f%2fNAS%2fMusik%2fHoerspiele%2fYakari%2f01.04%2520Im%2520Land%2520der%2520W%25c3%25b6lfe%2fKapitel%252003%2520001%2520Im%2520Land%2520der%2520W%25c3%25b6lfe.mp3&amp;v=4877</upnp:albumArtURI></container></DIDL-Lite>";
            SonosItem.ParseSingleItem(t);
            return true;
        }
        /// <summary>
        /// Liefert einen definierten Player
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetPlayer/{id}")]
        public async Task<SonosPlayer> GetPlayer(string id)
        {
            if (SonosHelper.Sonos == null || SonosHelper.Sonos.Players.Count == 0)
            {
                await SonosHelper.Initialisierung();
            }
            return await SonosHelper.GetPlayerbyUuid(id);
        }

        [HttpGet("GetLongPlayer/{id}")]
        public async Task<SonosPlayer> GetLongPlayer(string id)
        {
            if (SonosHelper.Sonos == null || SonosHelper.Sonos.Players.Count == 0)
            {
                await SonosHelper.Initialisierung();
            }
            if(SonosHelper.CheckAllPlayerReachable())
                return await SonosHelper.GetPlayerbyUuid(id);
            throw new Exception("System Reset, weil Player nicht erreichbar");
        }

        /// <summary>
        /// Liefert eine Liste dem Zeitpunkt der letzten Änderung eines jeden Players
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetLastChangesDateTimes")]
        public async Task<Dictionary<string, DateTime>> GetLastChangesDateTimes()
        {
            if(!await SonosHelper.CheckSonosLiving()) return null;
            _playersLastChange.Clear();
            foreach (SonosPlayer item in SonosHelper.Sonos.Players)
            {
                    _playersLastChange.Add(item.UUID, item.LastChange);
            }
            //Das erst hier, weil dann das Web schon initialisiert wurde. 
            Debug.WriteLine("GetLastChangesDateTimes wurde aufgerufen.");
            SonosHelper.Sonos.CheckDevicesToPlayer();
            return _playersLastChange;
        }
        #endregion Public Methoden
        [HttpGet("GetPlayerProperties/{id}")]
        public async Task<PlayerDeviceProperties> GetPlayerProperties(string id)
        {
            if (!await SonosHelper.CheckSonosLiving())
            {
                throw new Exception("CheckSonosLiving Error");
            }
            return await new PlayerDeviceProperties().FilledData(await SonosHelper.GetPlayerbyUuid(id));
        }
        [HttpGet("GetPlayerNamesAndUUID")]
        public async Task<Dictionary<String, String>> GetPlayerNamesAndUUID()
        {
            Dictionary<String, String> result = new();
            if (!await SonosHelper.CheckSonosLiving())
            {
                throw new Exception("CheckSonosLiving Error");
            }
            foreach (SonosPlayer sp in SonosHelper.Sonos.Players)
            {
                if (!result.ContainsKey(sp.Name))
                    result.Add(sp.Name, sp.UUID);
            }
            return result;
        }

        [HttpGet("CheckPlayerReachable")]
        public async Task<Boolean> CheckPlayerReachable()
        {
            if (! await SonosHelper.CheckSonosLiving()) return false;

            return SonosHelper.CheckAllPlayerReachable();
        }


        [HttpPost("SetPlayerProperties")]
        public async Task<ActionResult> SetPlayerProperties(PlayerPropertiesRequest playerPropertiesRequest)
        {
            try
            {
                

                if (playerPropertiesRequest == null) return BadRequest("Übergabe Null");
                SonosPlayer player = await SonosHelper.GetPlayerbyUuid(playerPropertiesRequest.uuid);
                if (player == null) return BadRequest("Kein Player Gefunden");
                Boolean retval = false;
                switch (playerPropertiesRequest.type)
                {
                    case PlayerDevicePropertiesTypes.ButtonLockState:
                        if (playerPropertiesRequest.value == "true")
                        {
                            retval = await player.DeviceProperties.SetButtonLockState(SonosUPnP.DataClasses.SonosEnums.OnOff.On);
                        }
                        else
                        {
                            retval = await player.DeviceProperties.SetButtonLockState(SonosUPnP.DataClasses.SonosEnums.OnOff.Off);
                        }
                        break;
                    case PlayerDevicePropertiesTypes.LEDState:
                        if (playerPropertiesRequest.value == "true")
                        {
                            retval = await player.DeviceProperties.SetLEDState(SonosUPnP.DataClasses.SonosEnums.OnOff.On);
                        }
                        else
                        {
                            retval = await player.DeviceProperties.SetLEDState(SonosUPnP.DataClasses.SonosEnums.OnOff.Off);
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
