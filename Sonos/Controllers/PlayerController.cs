using System;
using System.Collections.Generic;
using SonosUPnP;
using System.Text.RegularExpressions;
using System.Linq;
using MP3File;
using SonosUPnP.DataClasses;
using SonosUPnP.Props;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SonosConst;
using Sonos.Classes.Interfaces;
using HomeLogging;

namespace Sonos.Controllers
{
    [Route("/[controller]")]
    public class PlayerController : Controller
    {
        private readonly IMusicPictures musicPictures;
        private readonly ILogging _logger;
        private readonly ISonosHelper _sonosHelper;
        private readonly ISonosDiscovery _sonos;
        public PlayerController(IMusicPictures imu, ISonosHelper sonosHelper, ILogging log, ISonosDiscovery sonos)
        {
            musicPictures = imu;
            _logger = log;
            _sonosHelper = sonosHelper;
            _sonos = sonos;
        }

        #region Frontend GET Fertig
        /// <summary>
        /// Füllt den übergebenen Player mit Daten. 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        [HttpGet("FillPlayerPropertiesDefaults/{id}/{v}")]
        public async Task<Boolean> FillPlayerPropertiesDefaults(string id, Boolean v)
        {
            try
            {
                SonosPlayer pl = _sonos.GetPlayerbyUuid(id);
                if (pl == null) return false;
                await pl.FillPlayerPropertiesDefaultsAsync(v);
                return true;
            }
            catch (Exception ex)
            {
                _logger.ServerErrorsAdd("FillPlayerPropertiesDefaults:" + id, ex, "PlayerController");
                throw;
            }
        }
        /// <summary>
        /// Setzen des Wiedergabemodus wie Schuffe und Repeat
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v">SHUFFLE,NORMAL,SHUFFLE_NOREPEAT,REPEAT_ALL, REPEAT_ONE,SHUFFLE_REPEAT_ONE</param>
        [HttpGet("SetPlaymode/{id}/{v}")]
        public async Task<Boolean> SetPlaymode(string id, string v)
        {
            try
            {
                if (!Enum.TryParse(v, out SonosEnums.PlayModes k))
                {
                    return false;
                }
                var pl = _sonos.GetPlayerbyUuid(id);
                return await pl.AVTransport.SetPlayMode(k);
            }
            catch (Exception ex)
            {
                AddServerErrors("SetPlaymode", ex);
                throw;
            }

        }
        /// <summary>
        /// Umsortierung eines Songs in der Playlist
        /// </summary>
        /// <param name="id">Player</param>
        /// <param name="v">alteposition</param>
        /// <param name="v2">neueposition</param>
        [HttpGet("ReorderTracksInQueue/{id}/{v}/{v2}")]
        public async Task<Boolean> ReorderTracksInQueue(string id, string v, string v2)
        {
            try
            {
                if (string.IsNullOrEmpty(v) || string.IsNullOrEmpty(v2)) return false;
                if (int.TryParse(v, out int oldposition) && int.TryParse(v2, out int newposition))
                {
                    if (newposition > 0 && oldposition < newposition)
                        newposition++;
                    if (oldposition != newposition && oldposition > 0 && newposition > 0)
                    {
                        var pl = _sonos.GetPlayerbyUuid(id);
                        await pl.AVTransport.ReorderTracksInQueue(oldposition, newposition);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                AddServerErrors("ReorderTracksInQueue", ex);
                throw;
            }

        }
        /// <summary>
        /// Liefert die Baseulr für den Angegeben Player zurück.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("BaseURL/{id}")]
        public string BaseURL(string id)
        {
            try
            {

                SonosPlayer pl = _sonos.GetPlayerbyUuid(id);
                if (pl == null) return String.Empty;
                return pl.PlayerProperties.BaseUrl;
            }
            catch (Exception ex)
            {
                AddServerErrors("BaseURL", ex);
                throw;
            }
        }

        /// <summary>
        /// Player zum Absoielen bewegen
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Play/{id}")]
        public async Task<Boolean> Play(string id)
        {
            try
            {
                SonosPlayer pl = _sonos.GetPlayerbyUuid(id);
                if (pl == null) return false;
                return await pl.AVTransport?.Play();
            }
            catch (Exception ex)
            {
                AddServerErrors("Play", ex);
                throw;
            }
        }
        [HttpGet("Cover/{id}")]
        public String Cover(string id)
        {
            try
            {
                SonosPlayer pl = _sonos.GetPlayerbyUuid(id);
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
        public Boolean CheckPlayerPropertiesWithClient(string id, [FromForm] PlayerProperties v)
        {
            SonosPlayer sp = _sonos.GetPlayerbyUuid(id);
            if (sp == null) return false;
            try
            {
                sp.CheckPlayerPropertiesWithClient(v);
                return true;
            }
            catch (Exception ex)
            {
                sp.ServerErrorsAdd("CheckPlayerPropertiesWithClient", "PlayerController", ex);
                throw;
            }
        }
        /// <summary>
        /// Setzen von Pause
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Pause/{id}")]
        public async Task<Boolean> Pause(string id)
        {
            try
            {
                SonosPlayer pl = _sonos.GetPlayerbyUuid(id);
                if (pl == null) return false;
                var retv = await pl.AVTransport?.Pause();
                pl.PlayerProperties.TransportState = SonosEnums.TransportState.PAUSED_PLAYBACK;
                return retv;
            }
            catch (Exception ex)
            {
                AddServerErrors("Pause", ex);
                throw;
            }
        }
        /// <summary>
        /// Player Stoppen
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Stop/{id}")]
        public async Task<Boolean> Stop(string id)
        {
            try
            {
                SonosPlayer pl = _sonos.GetPlayerbyUuid(id);
                if (pl == null) return false;
                var retv = await pl.AVTransport.Stop();
                pl.PlayerProperties.TransportState = SonosEnums.TransportState.STOPPED;
                return retv;
            }
            catch (Exception ex)
            {
                AddServerErrors("Stop", ex);
                throw;
            }
        }
        /// <summary>
        /// Nächster Song
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Next/{id}")]
        public async Task<Boolean> Next(string id)
        {
            try
            {
                SonosPlayer pl = _sonos.GetPlayerbyUuid(id);
                if (pl == null) return false;
                return await pl.AVTransport.Next();
            }
            catch (Exception ex)
            {
                AddServerErrors("Next", ex);
                throw;
            }
        }
        /// <summary>
        /// Vorheriger Song
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Previous/{id}")]
        public async Task<Boolean> Previous(string id)
        {
            try
            {
                SonosPlayer pl = _sonos.GetPlayerbyUuid(id);
                if (pl == null) return false;
                return await pl.AVTransport.Previous();
            }
            catch (Exception ex)
            {
                AddServerErrors("Previous", ex);
                throw;
            }
        }
        /// <summary>
        /// Stummschalten eines Players
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("SetMute/{id}")]
        public async Task<Boolean> SetMute(string id)
        {
            try
            {
                SonosPlayer sonosPlayer = _sonos.GetPlayerbyUuid(id);
                if (sonosPlayer.PlayerProperties.GroupCoordinatorIsLocal)
                {
                    await sonosPlayer.GroupRenderingControl.SetGroupMute(!sonosPlayer.PlayerProperties.GroupRenderingControl_GroupMute);
                }
                else
                {
                    await sonosPlayer.RenderingControl.SetMute(!sonosPlayer.PlayerProperties.Mute);
                }
                return true;
            }
            catch (Exception ex)
            {
                AddServerErrors("SetMute", ex);
                throw;
            }
        }
        [HttpGet("GetMute/{id}")]
        public Boolean GetMute(string id)
        {
            try
            {
                SonosPlayer sp = _sonos.GetPlayerbyUuid(id);
                if (sp == null) return false;
                if (sp.PlayerProperties.GroupCoordinatorIsLocal)
                {
                    return sp.PlayerProperties.GroupRenderingControl_GroupMute;
                }
                return sp.PlayerProperties.Mute;
            }
            catch (Exception ex)
            {
                AddServerErrors("GetMute", ex);
                throw;
            }
        }
        /// <summary>
        /// Ermitteln des Sleeptimers.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetSleepTimer/{id}")]
        public async Task<String> GetSleepTimer(string id)
        {
            try
            {
                var pla = _sonos.GetPlayerbyUuid(id);
                if (pla == null)
                {
                    return SonosConstants.Off;
                }
                return await pla.AVTransport.GetRemainingSleepTimerDuration();
            }
            catch (Exception ex)
            {
                AddServerErrors("SetPlaymode", ex);
                return SonosConstants.Off;
            }
        }
        /// <summary>
        /// Ermitteln der Lautstärke
        /// </summary>
        /// <param name="id">Rincon des Players</param>
        /// <returns>Wert zwischen 1 und 100</returns>
        [HttpGet("GetVolume/{id}")]
        public async Task<int> GetVolume(string id)
        {
            try
            {
                SonosPlayer pl;
                try
                {
                    pl = _sonos.GetPlayerbyUuid(id);
                }
                catch (Exception ex)
                {
                    AddServerErrors("GetVolume:GetPlayer", ex);
                    return 1;
                }
                if (pl == null) return 0;
                var vol = await pl.RenderingControl.GetVolume();
                pl.PlayerProperties.Volume = vol;
                return vol;
            }
            catch (Exception ex)
            {
                AddServerErrors("GetVolume", ex);
                return 1;
            }
        }
        /// <summary>
        /// Ermitteln der Lautstärke
        /// </summary>
        /// <param name="id">Rincon des Players</param>
        /// <returns>Wert zwischen 1 und 100</returns>
        [HttpGet("GetGroupVolume/{id}")]
        public async Task<int> GetGroupVolume(string id)
        {
            try
            {
                SonosPlayer pl;
                try
                {
                    pl = _sonos.GetPlayerbyUuid(id);
                }
                catch (Exception ex)
                {
                    AddServerErrors("GetGroupVolume:GetPlayer", ex);
                    return 1;
                }
                if (pl == null || pl.GroupRenderingControl == null) return 0;
                var vol = await pl.GroupRenderingControl.GetGroupVolume();
                pl.PlayerProperties.GroupRenderingControl_GroupVolume = vol;
                return vol;
            }
            catch (Exception ex)
            {
                AddServerErrors("GetGroupVolume", ex);
                return 1;
            }
        }
        /// <summary>
        /// Ermittelt den Fade Mode
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetFadeMode/{id}")]
        public Boolean? GetFadeMode(string id)
        {

            SonosPlayer sp = _sonos.GetPlayerbyUuid(id);
            if (sp == null) return false;
            return sp.PlayerProperties.CurrentCrossFadeMode;
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
                SonosPlayer pl = _sonos.GetPlayerbyUuid(id);
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
                var pl = _sonos.GetPlayerbyUuid(id);
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
                SonosPlayer sp = _sonos.GetPlayerbyUuid(id);
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
                var pl = _sonos.GetPlayerbyUuid(id);
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
                var pl = _sonos.GetPlayerbyUuid(id);
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
            var pl = _sonos.GetPlayerbyUuid(id);
            if (pl == null) return new();
            await pl.GetPlayerPlaylist(v);
            try
            {
                if (!pl.PlayerProperties.Playlist.IsEmpty && !pl.PlayerProperties.Playlist.PlayListItemsHashChecked)
                {
                    foreach (SonosItem item in pl.PlayerProperties.Playlist.PlayListItems)
                    {
                        try
                        {
                            SonosItemHelper.UpdateItemToHashPath(item);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    pl.PlayerProperties.Playlist.PlayListItemsHashChecked = true;
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
                var pl = _sonos.GetPlayerbyUuid(id);
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
                var pl = _sonos.GetPlayerbyUuid(id);
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
            _logger.ServerErrorsAdd(Func, ex, "PlayerController");
        }

        #endregion PrivateFunctions
        #region Frontend POST
        /// <summary>
        /// Setzen des Schlummermodus
        /// </summary>
        /// <param name="id">Rincon des Players</param>
        /// <param name="v">Dauer in hh:mm:ss oder "aus"</param>
        [HttpPost("SetSleepTimer/{id}")]
        public async Task<Boolean> SetSleepTimer(string id, [FromForm] string v)
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
                    var pl = _sonos.GetPlayerbyUuid(id);
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
        public async Task<Boolean> RemoveFavItem([FromForm] string v)
        {
            if (v == null || !v.StartsWith(SonosConstants.FV2)) return false;
            try
            {
                var k = _sonos.ZoneProperties.ListOfFavorites.FirstOrDefault(x => x.ItemID == v);
                if (k == null && _sonos.ZoneProperties.ListOfFavorites.Count > 0) return false;
                var pl = _sonos.GetPlayerbySoftwareGenerationPlaylistentry(v);
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
        public async Task<Boolean> AddFavItem(string id, [FromForm] string v)
        {
            try
            {
                return await _sonos.ZoneMethods.CreateFavorite(_sonos.GetPlayerbyUuid(id), v);
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
        public async Task<IList<SonosItem>> Browsing(string id, [FromForm] string v)
        {
            var retval = await _sonos.ZoneMethods.Browsing(_sonos.GetPlayerbyUuid(id), v, true);
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
                    pla = _sonos.GetPlayerbyUuid(id);
                    if (pla.PlayerProperties.CurrentTrack != null)
                    {
                        cur = pla.PlayerProperties.CurrentTrack;
                        SonosItemHelper.UpdateItemToHashPath(cur);
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
                            return SonosItemHelper.UpdateItemToHashPath(pla.PlayerProperties.CurrentTrack);
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
                                string u = SonosItemHelper.URItoPath(cur.Uri);
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
                return SonosItemHelper.UpdateItemToHashPath(pla.PlayerProperties.CurrentTrack);
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
        public MP3File.MP3File GetSongMeta([FromForm] string v)
        {
            //Prüfen, ob schon in RatingFehlerliste enthalten.
            try
            {
                v = SonosItemHelper.URItoPath(v);
                if (MP3ReadWrite.listOfCurrentErrors.Any())
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
        public async Task<Boolean> SetGroups(string id, [FromForm] string[] v)
        {
            try
            {
                //Es wurde keiner gewählt was dazu führt, das alle Gruppen aufgelöst werden.
                if (v[0].ToLower() == SonosConstants.empty)
                {
                    foreach (var player in _sonos.Players)
                    {
                        await player.AVTransport.BecomeCoordinatorOfStandaloneGroup();
                    }
                    return true;
                }
                //SonosPlayer master = _sonosHelper.GetPlayerbyUuid(id);
                List<string> tocordinatedplayer = v.ToList();
                return await _sonosHelper.GenerateZoneConstruct(id, tocordinatedplayer);
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
        public Boolean SetSongMeta(string id, [FromForm] MP3File.MP3File v)
        {
            var pla = _sonos.GetPlayerbyUuid(id);
            MP3File.MP3File lied = v;
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
                        MP3ReadWrite.Add(lied);
                    }
                }
                return true;
            }
            catch
            {
                //Kein Catch, da dies über Finally gemacht wird
                return true;
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
        public Boolean SetRatingFilter(string id, [FromForm] SonosRatingFilter v)
        {
            try
            {
                var pl = _sonos.GetPlayerbyUuid(id);
                if (pl != null && v.IsValid)
                {
                    if (pl.RatingFilter.CheckSonosRatingFilter(v)) return false;
                    pl.RatingFilter = v;
                }
            }
            catch (Exception ex)
            {
                _logger.ServerErrorsAdd("SetRatingFilter:" + id, ex, "PlayerController");
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
        public async Task<Boolean> Seek(string id, [FromForm] double v)
        {
            if (v < 1) return true;
            var pl = _sonos.GetPlayerbyUuid(id);
            return await pl.AVTransport.Seek(TimeSpan.FromSeconds(v).ToString());
        }
        /// <summary>
        /// Etwas der Wiedergabeliste zufügen
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        [HttpPost("Enqueue/{id}")]
        public async Task<Boolean> Enqueue(string id, [FromForm] string v)
        {
            var pl = _sonos.GetPlayerbyUuid(id);
            return await _sonos.ZoneMethods.AddToQueue(v, pl);
        }
        /// <summary>
        /// Aktuelle wiedergabeliste Speichern.
        /// </summary>
        /// <param name="id">Rincon</param>
        /// <param name="v">Name der Wiedergabeliste. Falls schon vorhanden wird diese überschrieben</param>
        [HttpPost("SaveQueue/{id}")]
        public async Task<Boolean> SaveQueue(string id, [FromForm] string v)
        {
            try
            {
                SonosPlayer pla = _sonos.GetPlayerbyUuid(id);
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
                _logger.ServerErrorsAdd("SaveQueue:" + id, ex, "PlayerController");
                throw;
            }


        }
        /// <summary>
        /// Exportieren der aktuellen Wiedergabeliste
        /// </summary>
        /// <param name="id">Rincon</param>
        /// <param name="v">Titel des exports</param>
        [HttpPost("ExportQueue/{id}")]
        public async Task<Boolean> ExportQueue(string id, [FromForm] string v)
        {
            try
            {
                SonosPlayer pla = _sonos.GetPlayerbyUuid(id);
                //Playlist ermitteln und in Datei schreiben
                var brpl = await pla.ContentDirectory.Browse(BrowseObjects.CurrentPlaylist);
                IList<SonosItem> pl = brpl.Result;
                var brshares = await pla.ContentDirectory.Browse(BrowseObjects.Shares);
                var br = brshares.Result;
                string nas = br[0].Title;//Bei der Suche nach Shares wird im Titel der Pfad hinterlegt.
                List<MP3File.MP3File> mpfiles = new();
                foreach (SonosItem song in pl)
                {
                    string pfad = SonosItemHelper.URItoPath(song.Uri);
                    mpfiles.Add(MP3ReadWrite.ReadMetaData(pfad));

                }
                return MP3ReadWrite.WritePlaylist(mpfiles, v, nas + "\\Playlistexport");
            }
            catch (Exception ex)
            {
                _logger.ServerErrorsAdd("ExportQueue:" + id, ex, "PlayerController");
                throw;
            }
        }
        /// <summary>
        /// Aktuelle Wiedergabeliste ersetzen
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        [HttpPost("ReplacePlaylist/{id}")]
        public async Task<Boolean> ReplacePlaylist(string id, [FromForm] string v)
        {
            try
            {
                SonosPlayer pl = _sonos.GetPlayerbyUuid(id);
                await pl.AVTransport.RemoveAllTracksFromQueue();
                await Task.Delay(300);
                await _sonos.ZoneMethods.AddToQueue(v, pl);
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
                _logger.ServerErrorsAdd("ReplacePlaylist:" + id, ex, "PlayerController");
                throw;
            }
        }
        /// <summary>
        /// Song in der Wiedergabe liste setzen und abspielen
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        [HttpPost("SetSongInPlaylist/{id}")]
        public async Task<Boolean> SetSongInPlaylist(string id, [FromForm] string v)
        {
            try
            {
                SonosPlayer pl = _sonos.GetPlayerbyUuid(id);
                await pl.AVTransport.Seek(v, SonosEnums.SeekUnit.TRACK_NR);
                await Task.Delay(100);
                return await pl.AVTransport.Play();
            }
            catch (Exception ex)
            {
                _logger.ServerErrorsAdd("SetSongInPlaylist:" + id, ex, "PlayerController");
                throw;
            }
        }
        #endregion Frontend POST Fertig
    }
}
