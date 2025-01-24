using System;
using System.Collections.Generic;
using SonosUPnP;
using System.Text.RegularExpressions;
using System.Linq;
using MP3File;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SonosConst;
using Sonos.Classes.Interfaces;
using HomeLogging;
using SonosData.DataClasses;
using SonosUPNPCore.Classes;
using SonosData;
using SonosSQLiteWrapper.Interfaces;
using SonosUPNPCore.Interfaces;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sonos.Controllers
{
    [Route("/[controller]")]
    public class PlayerController(IMusicPictures musicPictures, ISonosHelper sonosHelper, ILogging logger, ISonosDiscovery sonosDiscovery) : Controller
    {
        private JsonSerializerOptions jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };


        #region Frontend GET Fertig
        /// <summary>
        /// Füllt den übergebenen Player mit Daten. 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        [HttpGet("FillPlayerPropertiesDefaults/{id}/{v}")]
        public async Task<IActionResult> FillPlayerPropertiesDefaults(string id, bool v)
        {
            try
            {
                SonosPlayer pl = sonosDiscovery.GetPlayerbyUuid(id);
                if (pl == null)
                {
                    return NotFound($"Spieler mit ID {id} wurde nicht gefunden.");
                }

                bool result = await pl.FillPlayerPropertiesDefaultsAsync(v);
                if (result)
                {
                    sonosHelper.CheckPlayerForHashImages(pl);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd($"FillPlayerPropertiesDefaults: {id}", ex, "PlayerController");
                return StatusCode(500, $"Ein interner Serverfehler ist aufgetreten: {ex.Message}");
            }
        }
        /// <summary>
        /// Setzen des Wiedergabemodus wie Schuffe und Repeat
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v">SHUFFLE,NORMAL,SHUFFLE_NOREPEAT,REPEAT_ALL, REPEAT_ONE,SHUFFLE_REPEAT_ONE</param>
        [HttpGet("SetPlaymode/{id}/{v}")]
        public async Task<IActionResult> SetPlaymode(string id, string v)
        {
            try
            {
                if (!Enum.TryParse(v, out SonosEnums.PlayModes playMode))
                {
                    return BadRequest($"Ungültiger Playmodus: {v}");
                }

                var player = sonosDiscovery.GetPlayerbyUuid(id);
                if (player == null)
                {
                    return NotFound($"Spieler mit ID {id} wurde nicht gefunden.");
                }

                bool result = await player.AVTransport.SetPlayMode(playMode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                AddServerErrors("SetPlaymode", ex);
                return StatusCode(500, $"Ein interner Serverfehler ist aufgetreten: {ex.Message}");
            }
        }

        /// <summary>
        /// Umsortierung eines Songs in der Playlist
        /// </summary>
        /// <param name="id">Player</param>
        /// <param name="v">alteposition</param>
        /// <param name="v2">neueposition</param>
        [HttpGet("ReorderTracksInQueue/{id}/{v}/{v2}")]
        public async Task<IActionResult> ReorderTracksInQueue(string id, string v, string v2)
        {
            try
            {
                if (string.IsNullOrEmpty(v) || string.IsNullOrEmpty(v2))
                {
                    return BadRequest("Ungültige Eingabeparameter: Positionen dürfen nicht leer sein.");
                }

                if (!int.TryParse(v, out int oldposition) || !int.TryParse(v2, out int newposition))
                {
                    return BadRequest("Ungültige Eingabeparameter: Positionen müssen ganze Zahlen sein.");
                }

                if (oldposition <= 0 || newposition <= 0)
                {
                    return BadRequest("Ungültige Eingabeparameter: Positionen müssen größer als 0 sein.");
                }

                if (oldposition == newposition)
                {
                    return Ok(true); // Keine Änderung notwendig
                }

                var pl = sonosDiscovery.GetPlayerbyUuid(id);
                if (pl == null)
                {
                    return NotFound($"Spieler mit ID {id} wurde nicht gefunden.");
                }

                if (newposition > oldposition)
                {
                    newposition++;
                }

                await pl.AVTransport.ReorderTracksInQueue(oldposition, newposition);
                return Ok(true);
            }
            catch (Exception ex)
            {
                AddServerErrors("ReorderTracksInQueue", ex);
                return StatusCode(500, $"Ein interner Serverfehler ist aufgetreten: {ex.Message}");
            }
        }

        /// <summary>
        /// Liefert die Baseulr für den Angegeben Player zurück.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("BaseURL/{id}")]
        public IActionResult BaseURL(string id)
        {
            try
            {
                SonosPlayer pl = sonosDiscovery.GetPlayerbyUuid(id);
                if (pl == null)
                {
                    return NotFound($"Spieler mit ID {id} wurde nicht gefunden.");
                }
                return Ok(pl.PlayerProperties.BaseUrl);
            }
            catch (Exception ex)
            {
                AddServerErrors("BaseURL", ex);
                return StatusCode(500, $"Ein interner Serverfehler ist aufgetreten: {ex.Message}");
            }
        }


        /// <summary>
        /// Player zum Absoielen bewegen
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Play/{id}")]
        public async Task<IActionResult> Play(string id)
        {
            try
            {
                SonosPlayer pl = sonosDiscovery.GetPlayerbyUuid(id);
                if (pl == null || pl.AVTransport == null)
                {
                    return NotFound($"Spieler mit ID {id} wurde nicht gefunden oder AVTransport ist nicht verfügbar.");
                }

                bool result = await pl.AVTransport.Play();
                return Ok(result);
            }
            catch (Exception ex)
            {
                AddServerErrors("Play", ex);
                return StatusCode(500, $"Ein interner Serverfehler ist aufgetreten: {ex.Message}");
            }
        }

        [HttpGet("Cover/{id}")]
        public String Cover(string id)
        {
            try
            {
                SonosPlayer pl = sonosDiscovery.GetPlayerbyUuid(id);
                if (pl == null) return null;
                if (pl.PlayerProperties.CurrentTrack.AlbumArtURI.StartsWith(SonosConstants.CoverHashPathForBrowser))
                {
                    return "http://" + Request.Host.Value + pl.PlayerProperties.CurrentTrack.AlbumArtURI;
                }
                else
                {
                    return "http://" + pl.PlayerProperties.BaseUrl + pl.PlayerProperties.CurrentTrack.AlbumArtURI;
                }
            }
            catch (Exception ex)
            {
                AddServerErrors("Play", ex);
                throw;
            }
        }


        [HttpGet("CheckPlayerPropertiesWithClient/{id}")]
        public IActionResult CheckPlayerPropertiesWithClient(string id, [FromBody] PlayerProperties v)
        {
            try
            {
                SonosPlayer sp = sonosDiscovery.GetPlayerbyUuid(id);
                if (sp == null)
                {
                    return NotFound($"Spieler mit ID {id} wurde nicht gefunden.");
                }

                sp.CheckPlayerPropertiesWithClient(v);
                return Ok(true);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Fehler beim Überprüfen der Spielereigenschaften: {ex.Message}";
                sonosDiscovery.GetPlayerbyUuid(id)?.ServerErrorsAdd("CheckPlayerPropertiesWithClient", "PlayerController", ex);
                return StatusCode(500, errorMessage);
            }
        }

        /// <summary>
        /// Setzen von Pause
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Pause/{id}")]
        public async Task<IActionResult> Pause(string id)
        {
            try
            {
                SonosPlayer pl = sonosDiscovery.GetPlayerbyUuid(id);
                if (pl == null)
                {
                    return NotFound($"Spieler mit ID {id} wurde nicht gefunden.");
                }

                if (pl.AVTransport == null)
                {
                    return BadRequest("AVTransport ist nicht verfügbar für diesen Spieler.");
                }

                var retv = await pl.AVTransport.Pause();
                if (retv)
                {
                    pl.PlayerProperties.TransportState = SonosEnums.TransportState.PAUSED_PLAYBACK;
                }

                return Ok(retv);
            }
            catch (Exception ex)
            {
                AddServerErrors("Pause", ex);
                return StatusCode(500, $"Ein interner Serverfehler ist aufgetreten: {ex.Message}");
            }
        }

        /// <summary>
        /// Player Stoppen
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Stop/{id}")]
        public async Task<IActionResult> Stop(string id)
        {
            try
            {
                SonosPlayer pl = sonosDiscovery.GetPlayerbyUuid(id);
                if (pl == null)
                {
                    return NotFound($"Spieler mit ID {id} wurde nicht gefunden.");
                }

                if (pl.AVTransport == null)
                {
                    return BadRequest("AVTransport ist nicht verfügbar für diesen Spieler.");
                }

                var retv = await pl.AVTransport.Stop();
                if (retv)
                {
                    pl.PlayerProperties.TransportState = SonosEnums.TransportState.STOPPED;
                }

                return Ok(retv);
            }
            catch (Exception ex)
            {
                AddServerErrors("Stop", ex);
                return StatusCode(500, $"Ein interner Serverfehler ist aufgetreten: {ex.Message}");
            }
        }

        /// <summary>
        /// Nächster Song
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Next/{id}")]
        public async Task<IActionResult> Next(string id)
        {
            try
            {
                SonosPlayer pl = sonosDiscovery.GetPlayerbyUuid(id);
                if (pl == null)
                {
                    return NotFound($"Spieler mit ID {id} wurde nicht gefunden.");
                }

                if (pl.AVTransport == null)
                {
                    return BadRequest("AVTransport ist nicht verfügbar für diesen Spieler.");
                }

                bool result = await pl.AVTransport.Next();
                return Ok(result);
            }
            catch (Exception ex)
            {
                AddServerErrors("Next", ex);
                return StatusCode(500, $"Ein interner Serverfehler ist aufgetreten: {ex.Message}");
            }
        }

        /// <summary>
        /// Vorheriger Song
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Previous/{id}")]
        public async Task<IActionResult> Previous(string id)
        {
            try
            {
                SonosPlayer pl = sonosDiscovery.GetPlayerbyUuid(id);
                if (pl == null)
                {
                    return NotFound($"Spieler mit ID {id} wurde nicht gefunden.");
                }

                if (pl.AVTransport == null)
                {
                    return BadRequest("AVTransport ist nicht verfügbar für diesen Spieler.");
                }

                bool result = await pl.AVTransport.Previous();
                return Ok(result);
            }
            catch (Exception ex)
            {
                AddServerErrors("Previous", ex);
                return StatusCode(500, $"Ein interner Serverfehler ist aufgetreten: {ex.Message}");
            }
        }

        /// <summary>
        /// Stummschalten eines Players
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("SetMute/{id}")]
        public async Task<IActionResult> SetMute(string id)
        {
            try
            {
                SonosPlayer sonosPlayer = sonosDiscovery.GetPlayerbyUuid(id);
                if (sonosPlayer == null)
                {
                    return NotFound($"Spieler mit ID {id} wurde nicht gefunden.");
                }

                if (sonosPlayer.PlayerProperties.GroupCoordinatorIsLocal)
                {
                    if (sonosPlayer.GroupRenderingControl == null)
                    {
                        return BadRequest("GroupRenderingControl ist nicht verfügbar für diesen Spieler.");
                    }
                    await sonosPlayer.GroupRenderingControl.SetGroupMute(!sonosPlayer.PlayerProperties.GroupRenderingControl_GroupMute);
                }
                else
                {
                    if (sonosPlayer.RenderingControl == null)
                    {
                        return BadRequest("RenderingControl ist nicht verfügbar für diesen Spieler.");
                    }
                    await sonosPlayer.RenderingControl.SetMute(!sonosPlayer.PlayerProperties.Mute);
                }

                return Ok(true);
            }
            catch (Exception ex)
            {
                AddServerErrors("SetMute", ex);
                return StatusCode(500, $"Ein interner Serverfehler ist aufgetreten: {ex.Message}");
            }
        }

        [HttpGet("GetMute/{id}")]
        public IActionResult GetMute(string id)
        {
            try
            {
                SonosPlayer sp = sonosDiscovery.GetPlayerbyUuid(id);
                if (sp == null)
                {
                    return NotFound($"Spieler mit ID {id} wurde nicht gefunden.");
                }

                bool muteStatus;
                if (sp.PlayerProperties.GroupCoordinatorIsLocal)
                {
                    muteStatus = sp.PlayerProperties.GroupRenderingControl_GroupMute;
                }
                else
                {
                    muteStatus = sp.PlayerProperties.Mute;
                }

                return Ok(muteStatus);
            }
            catch (Exception ex)
            {
                AddServerErrors("GetMute", ex);
                return StatusCode(500, $"Ein interner Serverfehler ist aufgetreten: {ex.Message}");
            }
        }

        /// <summary>
        /// Ermitteln des Sleeptimers.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetSleepTimer/{id}")]
        public async Task<IActionResult> GetSleepTimer(string id)
        {
            try
            {
                var pla = sonosDiscovery.GetPlayerbyUuid(id);
                if (pla == null)
                {
                    return NotFound($"Spieler mit ID {id} wurde nicht gefunden.");
                }

                if (pla.AVTransport == null)
                {
                    return BadRequest("AVTransport ist nicht verfügbar für diesen Spieler.");
                }

                string sleepTimerDuration = await pla.AVTransport.GetRemainingSleepTimerDuration();
                return Ok(sleepTimerDuration);
            }
            catch (Exception ex)
            {
                AddServerErrors("GetSleepTimer", ex);
                return StatusCode(500, $"Ein interner Serverfehler ist aufgetreten: {ex.Message}");
            }
        }

        /// <summary>
        /// Ermitteln der Lautstärke
        /// </summary>
        /// <param name="id">Rincon des Players</param>
        /// <returns>Wert zwischen 1 und 100</returns>
        [HttpGet("GetVolume/{id}")]
        public async Task<IActionResult> GetVolume(string id)
        {
            try
            {
                SonosPlayer pl;
                try
                {
                    pl = sonosDiscovery.GetPlayerbyUuid(id);
                }
                catch (Exception ex)
                {
                    AddServerErrors("GetVolume:GetPlayer", ex);
                    return StatusCode(500, "Fehler beim Abrufen des Players");
                }

                if (pl == null)
                {
                    return NotFound($"Spieler mit ID {id} wurde nicht gefunden.");
                }

                if (pl.RenderingControl == null)
                {
                    return BadRequest("RenderingControl ist nicht verfügbar für diesen Spieler.");
                }

                var vol = await pl.RenderingControl.GetVolume();
                pl.PlayerProperties.Volume = vol;
                return Ok(vol);
            }
            catch (Exception ex)
            {
                AddServerErrors("GetVolume", ex);
                return StatusCode(500, $"Ein interner Serverfehler ist aufgetreten: {ex.Message}");
            }
        }

        /// <summary>
        /// Ermitteln der Lautstärke
        /// </summary>
        /// <param name="id">Rincon des Players</param>
        /// <returns>Wert zwischen 1 und 100</returns>
        [HttpGet("GetGroupVolume/{id}")]
        public async Task<IActionResult> GetGroupVolume(string id)
        {
            try
            {
                SonosPlayer pl;
                try
                {
                    pl = sonosDiscovery.GetPlayerbyUuid(id);
                }
                catch (Exception ex)
                {
                    AddServerErrors("GetGroupVolume:GetPlayer", ex);
                    return StatusCode(500, "Fehler beim Abrufen des Players");
                }

                if (pl == null)
                {
                    return NotFound($"Spieler mit ID {id} wurde nicht gefunden.");
                }

                if (pl.GroupRenderingControl == null)
                {
                    return BadRequest("GroupRenderingControl ist nicht verfügbar für diesen Spieler.");
                }

                var vol = await pl.GroupRenderingControl.GetGroupVolume();
                pl.PlayerProperties.GroupRenderingControl_GroupVolume = vol;
                return Ok(vol);
            }
            catch (Exception ex)
            {
                AddServerErrors("GetGroupVolume", ex);
                return StatusCode(500, $"Ein interner Serverfehler ist aufgetreten: {ex.Message}");
            }
        }

        /// <summary>
        /// Ermittelt den Fade Mode
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetFadeMode/{id}")]
        public IActionResult GetFadeMode(string id)
        {
            try
            {
                SonosPlayer sp = sonosDiscovery.GetPlayerbyUuid(id);
                if (sp == null)
                {
                    return NotFound($"Spieler mit ID {id} wurde nicht gefunden.");
                }

                return Ok(sp.PlayerProperties.CurrentCrossFadeMode);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ein interner Serverfehler ist aufgetreten: {ex.Message}");
            }
        }


        /// <summary>
        ///Falls ein AudioIn Element vorhanden ist, dann kann dieses hier gesetzt werden.
        /// </summary>
        /// <param name="id"></param>
        [HttpGet("SetAudioIn/{id}")]
        public async Task<Boolean> SetAudioIn(string id)//todo: Später:Überarbeiten das ein Player auch einen anderen Eingang verwenden kann
        {
            try
            {
                SonosPlayer pl = sonosDiscovery.GetPlayerbyUuid(id);
                if (pl.PlayerProperties.CurrentTrack.Uri == null || pl.PlayerProperties.CurrentTrack.Uri.StartsWith(SonosConstants.xrinconstream) && (pl.PlayerProperties.CurrentTrack.StreamContent == SonosConstants.AudioEingang || pl.PlayerProperties.CurrentTrack.Title == "Heimkino"))
                {
                    //Normale Playlist laden
                    await pl.AVTransport.SetAVTransportURI(SonosConstants.xrinconqueue + pl.UUID + "#0");
                }
                else
                {
                    //Stream laden
                    await pl.AVTransport.SetAVTransportURI(SonosConstants.xrinconstream + pl.UUID);
                }
                //Neu Wegen Stream
                //pl.PlayerProperties.CurrentTrack = await SonosItemHelper.CheckItemForStreaming(pl.PlayerProperties.CurrentTrack, pl);
                return true;
            }
            catch (Exception ex)
            {
                AddServerErrors("SetAudioIn", ex);
                throw;
            }
        }
        /// <summary>
        /// Liefert die Liste von Ratingsfehlern
        /// </summary>
        /// <returns></returns>
        //[HttpGet("GetErrorListCount")]
        //public int GetErrorListCount()
        //{
        //    MP3ReadWrite.WriteNow();
        //    return MP3ReadWrite.listOfCurrentErrors.Count;
        //}
        ///// <summary>
        ///// Liefert die Namen der kaputten Songs
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("GetErrorList")]
        //public List<MP3File.MP3File> GetErrorList()
        //{
        //    try
        //    {
        //        foreach (MP3File.MP3File mp3 in MP3ReadWrite.listOfCurrentErrors)
        //        {
        //            if (String.IsNullOrEmpty(mp3.Titel))
        //            {
        //                mp3.Titel = MP3ReadWrite.ReadMetaData(mp3.Pfad).Titel;
        //            }
        //        }

        //        return MP3ReadWrite.listOfCurrentErrors;
        //    }
        //    catch (Exception ex)
        //    {
        //        AddServerErrors("GetErrorList", ex);
        //        return new List<MP3File.MP3File>();
        //    }
        //}
        /// <summary>
        /// Lautstärke setzen
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        [HttpGet("SetVolume/{id}/{v}")]
        public async Task<Boolean> SetVolume(string id, string v)
        {
            try
            {
                UInt16 value = Convert.ToUInt16(v);
                if (value > 100)
                {
                    value = 100;
                }
                if (value < 1)
                {
                    value = 1;
                }
                var pl = sonosDiscovery.GetPlayerbyUuid(id);
                return await pl.RenderingControl.SetVolume(value);
            }
            catch (Exception ex)
            {
                AddServerErrors("SetVolume", ex);
                throw;
            }
        }
        /// <summary>
        /// Setzen der Gruppenlautstärke
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        [HttpGet("SetGroupVolume/{id}/{v}")]
        public async Task<Boolean> SetGroupVolume(string id, int v)
        {
            try
            {
                int value = v;
                if (value > 100)
                {
                    value = 100;
                }
                if (value < 1)
                {
                    value = 1;
                }
                SonosPlayer sp = sonosDiscovery.GetPlayerbyUuid(id);
                if (sp == null || !sp.PlayerProperties.GroupCoordinatorIsLocal) return false;
                return await sp.GroupRenderingControl.SetGroupVolume(value);
            }
            catch (Exception ex)
            {
                AddServerErrors("SetVolume", ex);
                throw;
            }
        }
        /// <summary>
        /// Soll überbelendet werden
        /// </summary>
        /// <param name="id">Rincon</param>
        /// <param name="v">true/false</param>
        [HttpGet("SetFadeMode/{id}/{v}")]
        public async Task<Boolean> SetFadeMode(string id, Boolean v)
        {
            try
            {
                var pl = sonosDiscovery.GetPlayerbyUuid(id);
                return await pl.AVTransport.SetCrossfadeMode(v);
            }
            catch (Exception ex)
            {
                AddServerErrors("SetFadeMode", ex);
                throw;
            }
        }
        /// <summary>
        /// Songs aus der Wiedergabeliste entfernen
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        [HttpGet("RemoveSongInPlaylist/{id}/{v}")]
        public async Task<Boolean> RemoveSongInPlaylist(string id, int v)
        {
            try
            {
                var pl = sonosDiscovery.GetPlayerbyUuid(id);
                return await pl.AVTransport.RemoveTrackFromQueue(v); //Playlist wird im Player durch ein Event neu befüllt
            }
            catch (Exception ex)
            {
                AddServerErrors("SetPlaymode", ex);
                throw;
            }
        }
        /// <summary>
        /// Für den Übergebenen Player den Typ Playlist zurückgeben
        /// </summary>
        /// <param name="id">Rincon des Players</param>
        /// <returns>Playlist mit SonosItems und TotalMatches</returns>
        [HttpGet("GetPlayerPlaylist/{id}/{v}")]
        public async Task<Playlist> GetPlayerPlaylist(string id, Boolean v)
        {
            var pl = sonosDiscovery.GetPlayerbyUuid(id);
            if (pl == null) return new();
            await pl.GetPlayerPlaylist(v);
            try
            {
                if (!pl.PlayerProperties.Playlist.IsEmpty && !pl.PlayerProperties.Playlist.PlayListItemsHashChecked)
                {
                    lock (pl.PlayerProperties.Playlist.PlayListItems)
                    {
                        foreach (SonosItem item in pl.PlayerProperties.Playlist.PlayListItems)
                        {
                            try
                            {
                                if (item != null)
                                    musicPictures.UpdateItemToHashPath(item);
                                else
                                   break;
                            }
                            catch
                            {
                                continue;
                            }
                        }
                        pl.PlayerProperties.Playlist.PlayListItemsHashChecked = true;
                    }
                }
            }
            catch
            {
                //ignore; wird zu fehlern führen beim wechsel der Playlist.
            }
            return pl.PlayerProperties.Playlist;
        }
        /// <summary>
        /// Liefert den Playmode (Wiedergabeart) zurück
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetPlayMode/{id}")]
        public String GetPlayMode(string id)
        {
            try
            {
                var pl = sonosDiscovery.GetPlayerbyUuid(id);
                return pl.PlayerProperties.CurrentPlayModeString;
            }
            catch (Exception ex)
            {
                AddServerErrors("GetPlayMode", ex);
                throw;
            }
        }
        /// <summary>
        /// Liefert ob ein Player pausiert, gestoppt oder abspielend ist.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetTransportState/{id}")]
        public string GetTransportState(string id)
        {
            try
            {
                var pl = sonosDiscovery.GetPlayerbyUuid(id);
                return pl.PlayerProperties.TransportStateString;
            }
            catch (Exception ex)
            {
                AddServerErrors("GetPlayState", ex);
                return SonosEnums.TransportState.STOPPED.ToString();
            }
        }
        #endregion Frontend GET Fertig
        #region PrivateFunctions


        /// <summary>
        /// Fügt Exception zum _sonosHelper
        /// </summary>
        /// <param name="Func"></param>
        /// <param name="ex"></param>
        private void AddServerErrors(string Func, Exception ex)
        {
            logger.ServerErrorsAdd(Func, ex, "PlayerController");
        }

        #endregion PrivateFunctions
        #region Frontend POST
        /// <summary>
        /// Setzen des Schlummermodus
        /// </summary>
        /// <param name="id">Rincon des Players</param>
        /// <param name="v">Dauer in hh:mm:ss oder "aus"</param>
        [HttpPost("SetSleepTimer/{id}")]
        public async Task<Boolean> SetSleepTimer(string id, [FromBody] string v)
        {
            try
            {
                var k = new Regex(@"^(?:[01][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$");
                if (k.IsMatch(v) || v == SonosConstants.Off)
                {
                    if (v == SonosConstants.Off)
                    {
                        v = String.Empty;
                    }
                    var pl = sonosDiscovery.GetPlayerbyUuid(id);
                    return await pl.AVTransport.ConfigureSleepTimer(v);
                }
                return false;
            }
            catch (Exception ex)
            {
                AddServerErrors("SetSleepTimer", ex);
                throw;
            }
        }
        /// <summary>
        /// Entfernt ein übergebenes FAvoriten Item
        /// </summary>
        /// <param name="id">Player ID</param>
        /// <param name="v">Favorititem Muster: FV:2/XX</param>
        [HttpPost("RemoveFavItem")]
        public async Task<Boolean> RemoveFavItem([FromBody] string v)
        {
            if (v == null || !v.StartsWith(SonosConstants.FV2)) return false;
            try
            {
                var k = sonosDiscovery.ZoneProperties.ListOfFavorites.FirstOrDefault(x => x.ItemID == v);
                if (k == null && sonosDiscovery.ZoneProperties.ListOfFavorites.Count > 0) return false;
                var pl = sonosDiscovery.GetPlayerbySoftwareGenerationPlaylistentry(v);
                return await pl.ContentDirectory.DestroyObject(v);
            }
            catch (Exception ex)
            {
                AddServerErrors("RemoveFavItem", ex);
                throw;
            }
        }
        /// <summary>
        /// Fügt das beigefügte Item den Favoriten hinzu
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        [HttpPost("AddFavItem/{id}")]
        public async Task<Boolean> AddFavItem(string id, [FromBody] string v)
        {
            try
            {
                return await sonosDiscovery.ZoneMethods.CreateFavorite(sonosDiscovery.GetPlayerbyUuid(id), v);
            }
            catch (Exception ex)
            {
                AddServerErrors("AddFavItem", ex);
                throw;
            }
        }

        /// <summary>
        /// Liefert eine Liste mit Sonositems zurück. Bei Tracks wird das Rating mit ausgelesen und ausgegeben.
        /// </summary>
        /// <param name="id">Id des Players</param>
        /// <param name="v">Der Browsingparameter wie z.B. A: / S: / A:Artist</param>
        /// <returns></returns>
        [HttpPost("Browsing/{id}")]
        public async Task<IList<SonosItem>> Browsing(string id, [FromBody] string v)
        {
            var retval = await sonosDiscovery.ZoneMethods.Browsing(sonosDiscovery.GetPlayerbyUuid(id), v, true);
            if (retval == null) return null;
            musicPictures.UpdateItemListToHashPath(retval);
            return retval;
        }
        /// <summary>
        /// Wenn currentstate nicht funktioniert liefert die funktion dennoch Daten
        /// </summary>
        /// <param name="id">Playerid</param>
        /// <returns></returns>
        [HttpGet("GetAktSongInfo/{id}")]
        public async Task<SonosItem> GetAktSongInfo(string id)
        {
            try
            {

                SonosPlayer pla;
                PlayerInfo pl;
                SonosItem cur = new();
                try
                {
                    pla = sonosDiscovery.GetPlayerbyUuid(id);
                    if (pla.PlayerProperties.CurrentTrack != null)
                    {
                        cur = pla.PlayerProperties.CurrentTrack;
                        musicPictures.UpdateItemToHashPath(cur);
                    }
                    if (pla.AVTransport != null)
                    {
                        pl = await pla.AVTransport?.GetPositionInfo();
                    }
                    else
                    {
                        return cur;
                    }
                }
                catch
                {
                    return cur;
                }
                if (pla == null)
                {
                    return cur;
                }
                if (pl == null)
                {
                    return cur;
                }
                if (pl.TrackMetaData != SonosConstants.NotImplemented) //Kommt, wenn kein Song in Playlist
                {
                    try
                    {
                        if (pl.IsEmpty)
                        {
                            //MediaInfo lesen
                            var mi = await pla.AVTransport.GetMediaInfo();// pla.GetMediaInfoURIMeta();
                            if (mi != null && !string.IsNullOrEmpty(mi.URIMetaData))
                            {
                                cur = SonosItem.ParseSingleItem(mi.URIMetaData);
                                cur.Uri = mi.URI;
                            }
                        }
                        else
                        {
                            cur = SonosItem.ParseSingleItem(pl.TrackMetaData);
                        }
                    }
                    catch (Exception ex)
                    {
                        AddServerErrors("GetAktSongInfo:Block1", ex);
                        return pla.PlayerProperties.CurrentTrack ?? cur;
                    }
                    try
                    {
                        if (cur.Uri != pla.PlayerProperties.CurrentTrack.Uri)
                        {
                            var cump3 = pla.PlayerProperties.CurrentTrack.MP3;
                            pla.PlayerProperties.CurrentTrack = cur;
                            if (cump3 != null && cur.Artist == cump3.Artist && cur.Title == cump3.Titel &&
                                cur.Album == cump3.Album)
                            {
                                pla.PlayerProperties.CurrentTrack.MP3 = cump3; //MP3 wieder schreiben.
                            }
                            if (pla.PlayerProperties.CurrentTrack.Duration != pl.TrackDuration)
                                pla.PlayerProperties.CurrentTrack.Duration = pl.TrackDuration;
                        }
                        if (pla.PlayerProperties.CurrentTrackNumber != pl.TrackIndex)
                        {
                            pla.PlayerProperties.CurrentTrackNumber = pl.TrackIndex;
                        }
                        if (pla.PlayerProperties.CurrentTrack.RelTime != pl.RelTime)
                            pla.PlayerProperties.CurrentTrack.RelTime = pl.RelTime;
                        //Neu Wegen Stream
                        //pla.PlayerProperties.CurrentTrack = await SonosItemHelper.CheckItemForStreaming(pla.PlayerProperties.CurrentTrack, pla);
                        if (pla.PlayerProperties.CurrentTrack.Uri.Contains(".mp4") &&
                            pla.PlayerProperties.CurrentTrack.Uri.StartsWith(SonosConstants.xsonoshttp))
                        {
                            return musicPictures.UpdateItemToHashPath(pla.PlayerProperties.CurrentTrack);
                        }
                    }
                    catch (Exception ex)
                    {
                        AddServerErrors("GetAktSongInfo:Block2", ex);
                        return cur;
                    }
                    try
                    {
                        if (pla.PlayerProperties.CurrentTrack.Stream == false)
                        {
                            if (pla.PlayerProperties.CurrentTrack.Uri != pl.TrackURI || pla.PlayerProperties.CurrentTrack.MP3.IsEmpty())
                            {
                                //Bei Songwechsel Zugriff aufs Dateisystem außer es wird als Parameter übergeben.
                                string u = SonosConstants.URItoPath(cur.Uri);
                                pla.PlayerProperties.CurrentTrack.MP3 = MP3ReadWrite.ReadMetaData(u);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        AddServerErrors("GetAktSongInfo:Block3", ex);
                        return cur;
                    }
                }
                else
                {
                    try
                    {
                        if (pl.TrackURI.StartsWith(SonosConstants.xrinconstream))
                        {
                            pla.PlayerProperties.CurrentTrack.Uri = pl.TrackURI;
                        }
                    }
                    catch (Exception ex)
                    {
                        AddServerErrors("GetAktSongInfo:Block4", ex);
                        return cur;
                    }
                }
                return musicPictures.UpdateItemToHashPath(pla.PlayerProperties.CurrentTrack);
            }
            catch (Exception ex)
            {
                AddServerErrors("GetAktSongInfo", ex);
                throw;
            }
        }
        /// <summary>
        /// Läd die MetaDaten aus dem Übergebenen Parameter über die TagLib
        /// </summary>
        /// <param name="id">0</param>
        /// <param name="v">URI zum Track</param>
        /// <returns></returns>
        [HttpPost("GetSongMeta")]
        public MP3File.MP3File GetSongMeta([FromBody] string v)
        {
            //Prüfen, ob schon in RatingFehlerliste enthalten.
            try
            {
                v = SonosConstants.URItoPath(v);
                if (MP3ReadWrite.listOfCurrentErrors.Count != 0)
                {
                    var k = MP3ReadWrite.listOfCurrentErrors.Find(x => x.Pfad == v);
                    if (k != null) return k;
                }
                return MP3ReadWrite.ReadMetaData(v);

            }
            catch (Exception ex)
            {
                AddServerErrors("GetSongMeta", ex);
                return new MP3File.MP3File();
            }
        }

        [HttpPost("SetGroups/{id}")]
        public async Task<Boolean> SetGroups(string id, [FromBody] string[] v)
        {
            try
            {
                //Es wurde keiner gewählt was dazu führt, das alle Gruppen aufgelöst werden.
                if (v[0].Equals(SonosConstants.empty, StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach (var player in sonosDiscovery.Players)
                    {
                        await player.AVTransport.BecomeCoordinatorOfStandaloneGroup();
                    }
                    return true;
                }
                //SonosPlayer master = _sonosHelper.GetPlayerbyUuid(id);
                List<string> tocordinatedplayer = v.ToList();
                return await sonosHelper.GenerateZoneConstruct(id, tocordinatedplayer);
            }
            catch (Exception ex)
            {
                AddServerErrors("SetGroups", ex);
                throw;
            }
        }
        /// <summary>
        /// Setzt die übergebenen Parameter (Rating/PopM) inkl. Fehlerhandling, falls song gerade abgespielt wird.
        /// </summary>
        /// <param name="id">UUID Des Players</param>
        /// <param name="v">PFAD#Rating'Gelegenheit</param>
        [HttpPost("SetSongMeta/{id}")]
        public async Task<IActionResult> SetSongMeta(string id)
        {
            var requestReader = new StreamReader(HttpContext.Request.Body);
            var content = await requestReader.ReadToEndAsync();
            if (string.IsNullOrEmpty(content))
            {
                return BadRequest("Der Request-Body ist leer.");
            }

            MP3File.MP3File lied = new MP3File.MP3File();
            var pla = sonosDiscovery.GetPlayerbyUuid(id);

            try
            {
                lied = System.Text.Json.JsonSerializer.Deserialize<MP3File.MP3File>(content, jsonOptions);
            }
            catch (Exception ex) {
                logger.ServerErrorsAdd("SetSongMeta:" + id+ "Content:"+content, ex, "PlayerController");
                return BadRequest($"Fehler bei der JSON-Deserialisierung: {ex.Message}");
            }
            if (string.IsNullOrEmpty(lied.Pfad))
            {
                return BadRequest("Der Pfad des MP3-Files ist leer oder null.");
            }
            try
            {
                //Stream Rating
                Boolean streaming = false;
                if (pla.PlayerProperties.CurrentTrack.MP3.Pfad == lied.Pfad)
                {
                    //wenn aktueller song diesen hinterlegen
                    pla.PlayerProperties.CurrentTrack.MP3 = lied;
                }
                if (pla.PlayerProperties.Playlist.PlayListItems.Count != 0 && lied.Tracknumber != -1)
                {

                    try
                    {
                        var plid = pla.PlayerProperties.Playlist.PlayListItems[lied.Tracknumber];
                        if (plid != null)
                        {
                            plid.MP3 = lied;
                        }
                    }
                    catch
                    {

                    }

                }
                if (!streaming)
                {
                    if (!MP3ReadWrite.WriteMetaData(lied))
                    {
                        logger.ServerErrorsAdd("MP3 Schreiben SetSongMeta:" + id + "Content:" + content, MP3ReadWrite.LetzerFehler, "PlayerController");
                        MP3ReadWrite.Add(lied);
                    }
                }
                return Ok(true);
            }
            catch
            {
                //Kein Catch, da dies über Finally gemacht wird
                return Ok(true);
            }
            finally
            {
                //Falls mal Fehler vorhanden waren diese nun abarbeiten und hoffen, das dies geht
                if (MP3ReadWrite.listOfCurrentErrors.Count > 0)
                {
                    MP3ReadWrite.WriteNow();
                }
            }
        }
        /// <summary>
        /// Setzen des Filters für Songs aufgrund der Ratings
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        [HttpPost("SetRatingFilter/{id}")]
        public Boolean SetRatingFilter(string id, [FromBody] SonosRatingFilter v)
        {
            try
            {
                var pl = sonosDiscovery.GetPlayerbyUuid(id);
                if (pl != null && v.IsValid)
                {
                    if (pl.RatingFilter.CheckSonosRatingFilter(v)) return false;
                    pl.RatingFilter = v;
                }
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("SetRatingFilter:" + id, ex, "PlayerController");
                throw;
            }
            return true;
        }
        /// <summary>
        /// Im Song vorspulen
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        [HttpPost("Seek/{id}")]
        public async Task<Boolean> Seek(string id, [FromBody] double v)
        {
            if (v < 1) return true;
            var pl = sonosDiscovery.GetPlayerbyUuid(id);
            return await pl.AVTransport.Seek(TimeSpan.FromSeconds(v).ToString());
        }
        /// <summary>
        /// Etwas der Wiedergabeliste zufügen
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        [HttpPost("Enqueue/{id}")]
        public async Task<Boolean> Enqueue(string id, [FromBody] string v)
        {
            var pl = sonosDiscovery.GetPlayerbyUuid(id);
            return await sonosDiscovery.ZoneMethods.AddToQueue(v, pl);
        }
        /// <summary>
        /// Aktuelle wiedergabeliste Speichern.
        /// </summary>
        /// <param name="id">Rincon</param>
        /// <param name="v">Name der Wiedergabeliste. Falls schon vorhanden wird diese überschrieben</param>
        [HttpPost("SaveQueue/{id}")]
        public async Task<Boolean> SaveQueue(string id, [FromBody] string v)
        {
            try
            {
                SonosPlayer pla = sonosDiscovery.GetPlayerbyUuid(id);
                var br = await pla.ContentDirectory.Browse(BrowseObjects.SonosPlaylist);
                IList<SonosItem> sonosplaylists = br.Result;
                string sonosid = String.Empty;
                foreach (SonosItem pl in sonosplaylists)
                {
                    if (pl.Title == v)
                    {
                        sonosid = pl.ContainerID;
                    }
                }
                if (!String.IsNullOrEmpty(sonosid))
                {
                    return await pla.AVTransport.SaveQueue(v, sonosid);
                }
                else
                {
                    return await pla.AVTransport.SaveQueue(v);
                }
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("SaveQueue:" + id, ex, "PlayerController");
                throw;
            }


        }
        /// <summary>
        /// Exportieren der aktuellen Wiedergabeliste
        /// </summary>
        /// <param name="id">Rincon</param>
        /// <param name="v">Titel des exports</param>
        [HttpPost("ExportQueue/{id}")]
        public async Task<Boolean> ExportQueue(string id, [FromBody] string v)
        {
            try
            {
                SonosPlayer pla = sonosDiscovery.GetPlayerbyUuid(id);
                //Playlist ermitteln und in Datei schreiben
                var brpl = await pla.ContentDirectory.Browse(BrowseObjects.CurrentPlaylist);
                IList<SonosItem> pl = brpl.Result;
                var brshares = await pla.ContentDirectory.Browse(BrowseObjects.Shares);
                var br = brshares.Result;
                string nas = br[0].Title;//Bei der Suche nach Shares wird im Titel der Pfad hinterlegt.
                List<MP3File.MP3File> mpfiles = [];
                foreach (SonosItem song in pl)
                {
                    string pfad = SonosConstants.URItoPath(song.Uri);
                    mpfiles.Add(MP3ReadWrite.ReadMetaData(pfad));

                }
                return MP3ReadWrite.WritePlaylist(mpfiles, v, nas + "\\Playlistexport");
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("ExportQueue:" + id, ex, "PlayerController");
                throw;
            }
        }
        /// <summary>
        /// Aktuelle Wiedergabeliste ersetzen
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        [HttpPost("ReplacePlaylist/{id}")]
        public async Task<Boolean> ReplacePlaylist(string id, [FromBody] string v)
        {
            try
            {
                SonosPlayer pl = sonosDiscovery.GetPlayerbyUuid(id);
                await pl.AVTransport.RemoveAllTracksFromQueue();
                await Task.Delay(300);
                await sonosDiscovery.ZoneMethods.AddToQueue(v, pl);
                if (pl.PlayerProperties.AVTransportURI != SonosConstants.xrinconqueue + pl.UUID + "#0")
                {
                    await pl.AVTransport.SetAVTransportURI(SonosConstants.xrinconqueue + pl.UUID + "#0");
                }
                await Task.Delay(300);
                await pl.AVTransport.Play();
                return true;
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("ReplacePlaylist:" + id, ex, "PlayerController");
                throw;
            }
        }
        /// <summary>
        /// Song in der Wiedergabe liste setzen und abspielen
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        [HttpGet("SetSongInPlaylist/{id}/{v}")]
        public async Task<Boolean> SetSongInPlaylist(string id, string v)
        {
            try
            {
                SonosPlayer pl = sonosDiscovery.GetPlayerbyUuid(id);
                await pl.AVTransport.Seek(v, SonosEnums.SeekUnit.TRACK_NR);
                await Task.Delay(100);
                return await pl.AVTransport.Play();
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("SetSongInPlaylist:" + id, ex, "PlayerController");
                throw;
            }
        }
        #endregion Frontend POST Fertig
    }
}
