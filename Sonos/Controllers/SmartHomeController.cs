using System;
using Sonos.Classes;
using System.Threading.Tasks;
using SonosUPnP;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SonosConst;
using Sonos.Classes.Interfaces;
using HomeLogging;
using SonosData.DataClasses;
using SonosData;
using SonosData.Enums;
using SonosData.Props;
using SonosSQLiteWrapper.Interfaces;
using SonosUPNPCore.Interfaces;

namespace Sonos.Controllers
{
    [Route("/[controller]")]
    public class SmartHomeController(ISonosHelper sonosHelper, ILogging logger, ISonosDiscovery sonosDiscovery, IMusicPictures musicPictures) : Controller
    {
        #region Global
        [HttpGet("StoppAllPlayers")]
        public async Task<Boolean> StoppAllPlayers()
        {
            if (!sonosDiscovery.Players.Any())
            {
                logger.ServerErrorsAdd("StoppAllPlayers", new Exception("es gibt keinen Player"));
                throw new Exception("StoppAllPlayers No Player");
            }

            foreach (SonosPlayer sp in sonosDiscovery.Players)
            {
                try
                {
                    if (sp == null)
                    {
                        logger.ServerErrorsAdd("StoppAllPlayers", new Exception("Player ist null"));
                        continue;
                    }
                    if (sp.AVTransport == null)
                    {
                        logger.ServerErrorsAdd("StoppAllPlayers", new Exception("Avtransport ist null bei Player:" + sp.Name));
                        continue;
                    }
                    if (sp.PlayerProperties.GroupCoordinatorIsLocal)
                    {
                        await sp.AVTransport?.Pause();
                    }
                    else
                    {
                        await sp.AVTransport?.BecomeCoordinatorOfStandaloneGroup();
                    }
                }
                catch (Exception ex)
                {
                    logger.ServerErrorsAdd("StoppAllPlayers", ex);
                    continue;
                }
            }
            return true;
        }
        /// <summary>
        /// Set Room Volume relativ 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v">UP for true</param>
        /// <returns></returns>
        [HttpGet("RoomVolumeRelativ/{id}/{v}")]
        public async Task<String> RoomVolumeRelativ(string id, Boolean v)
        {
            if (string.IsNullOrEmpty(id)) return "Volume leer";
            SonosPlayer pp = sonosDiscovery.GetPlayerbyName(id);
            if (!pp.PlayerProperties.GroupCoordinatorIsLocal) return "NotLeader";
            int vol = await pp.GroupRenderingControl?.GetGroupVolume();
            if (v)
            {
                vol += 3;
            }
            else
            {
                vol -= 3;
            }
            if (vol > 0 && vol < 101)
                try
                {
                    //await pp.RenderingControl?.SetVolume(v) &&
                    return "retval:" + (await pp.GroupRenderingControl?.SetGroupVolume(vol)).ToString() + "Boolean insert:" + v;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            return "Volume out of Range";
        }
        #endregion Global
        #region Ground
        [HttpGet("GroundFloorOn/{id}")]
        public async Task<Boolean> GroundFloorOn(string id = SonosConstants.defaultPlaylist)
        {
            string playlistToPlay = id;
            try
            {
                //Alles ins Wohnzimmer legen.
                SonosPlayer primaryplayer = sonosDiscovery.GetPlayerbyName(SonosConstants.WohnzimmerName);
                if (primaryplayer == null)
                {
                    logger.ServerErrorsAdd("GroundFloorOn:Primary", new Exception("primaryplayer konnten nicht ermittelt werden"), "SmartHomeWrapper");
                    return false;
                }
                SonosPlayer secondaryplayer = sonosDiscovery.GetPlayerbyName(SonosConstants.EsszimmerName);
                if (secondaryplayer == null)
                {
                    logger.ServerErrorsAdd("GroundFloorOn:secondaryplayer", new Exception("secondaryplayer konnte nicht ermittelt werden"), "SmartHomeWrapper");
                    return false;
                }
                SonosPlayer thirdplayer = sonosDiscovery.GetPlayerbyName(SonosConstants.KücheName);
                if (thirdplayer == null)
                {
                    logger.ServerErrorsAdd("GroundFloorOn:thirdplayer", new Exception("thirdplayer konnte nicht ermittelt werden."), "SmartHomeWrapper");
                    return false;
                }
                if (sonosHelper.IsSonosTargetGroupExist(primaryplayer, [secondaryplayer.UUID, thirdplayer.UUID]))
                {
                    try
                    {
                        //Die Zielarchitektur existiert, daher nur Playlist
                        int oldcurrenttrack = primaryplayer.PlayerProperties.CurrentTrackNumber;
                        if (sonosDiscovery.ZoneProperties.ListOfAllPlaylist.Count == 0)
                            await sonosHelper.GetAllPlaylist();
                        var playlist = sonosDiscovery.ZoneProperties.ListOfAllPlaylist.FirstOrDefault(x => x.Title.Equals(playlistToPlay, StringComparison.CurrentCultureIgnoreCase));
                        //if (playlist == null)
                        //{
                        //    await _sonosHelper.GetAllPlaylist();//todo: prüfen ob das aukommentiert bleiben kann.
                        //}
                        playlist = sonosDiscovery.ZoneProperties.ListOfAllPlaylist.FirstOrDefault(x => x.Title.Equals(playlistToPlay, StringComparison.CurrentCultureIgnoreCase));
                        Boolean loadPlaylist = false;
                        if (playlist != null)
                        {
                            loadPlaylist = sonosHelper.CheckPlaylist(playlist, primaryplayer);
                        }
                        if (loadPlaylist)
                        {
                            if (!await sonosHelper.LoadPlaylist(playlist, primaryplayer))
                                return false;
                        }
                        else
                        {
                            //alten Song aus der Playlist laden, da immer wieder auf 1 reset passiert.
                            if (primaryplayer.PlayerProperties.CurrentTrackNumber != oldcurrenttrack)
                            {
                                await primaryplayer.AVTransport?.Seek(oldcurrenttrack.ToString(), SonosEnums.SeekUnit.TRACK_NR);
                                await Task.Delay(100);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.ServerErrorsAdd("GroundFloorOn:Block2", ex, "SmartHomeWrapper");
                        throw;
                    }
                }
                else
                {

                    try
                    {
                        //alles neu
                        await sonosHelper.GenerateZoneConstruct(primaryplayer, [secondaryplayer.UUID, thirdplayer.UUID]);
                        //Playlist verarbeiten
                        if (sonosDiscovery.ZoneProperties.ListOfAllPlaylist.Count == 0)
                            await sonosHelper.GetAllPlaylist();
                        var playlist = sonosDiscovery.ZoneProperties.ListOfAllPlaylist.FirstOrDefault(x => x.Title.Equals(playlistToPlay, StringComparison.CurrentCultureIgnoreCase));
                        if (!await sonosHelper.LoadPlaylist(playlist, primaryplayer))
                            return false;
                    }
                    catch (Exception ex)
                    {
                        logger.ServerErrorsAdd("GroundFloorOn:Block3", ex, "SmartHomeWrapper");
                        throw;
                    }
                }
                try
                {
                    ushort secondaryplayerVolume = SonosConstants.EsszimmerVolume;
                    ushort thirdplayerVolume = SonosConstants.KücheVolume;
                    if (secondaryplayer.PlayerProperties.Volume != secondaryplayerVolume && secondaryplayer.RenderingControl != null)
                    {
                        await secondaryplayer.RenderingControl.SetVolume(secondaryplayerVolume);
                    }
                    if (primaryplayer.PlayerProperties.Volume != SonosConstants.WohnzimmerVolume && primaryplayer.RenderingControl != null)
                    {
                        await primaryplayer.RenderingControl.SetVolume(SonosConstants.WohnzimmerVolume);
                    }
                    if (thirdplayer.PlayerProperties.Volume != thirdplayerVolume && thirdplayer.RenderingControl != null)
                    {
                        await thirdplayer.RenderingControl.SetVolume(thirdplayerVolume);
                    }
                }
                catch (Exception ex)
                {
                    logger.ServerErrorsAdd("GroundFloorOn:Block4", ex, "SmartHomeWrapper");
                    throw;
                }

                try
                {
                    await Task.Delay(500);
                    return await primaryplayer.AVTransport.Play();

                }
                catch (Exception ex)
                {
                    string mess = primaryplayer.Name + " AVTransport:" + (primaryplayer.AVTransport != null).ToString();
                    logger.ServerErrorsAdd("GroundFloorOn:Block5:" + mess, ex, "SmartHomeWrapper");
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("GroundFloorOn", ex, "SmartHomeWrapper");
                throw;
            }
        }

        [HttpGet("GroundFloorOn")]
        public async Task<Boolean> GroundFloorOn()
        {
            return await GroundFloorOn(SonosConstants.defaultPlaylist);
        }
        [HttpGet("GroundFloorOff")]
        public async Task<Boolean> GroundFloorOff()
        {
            try
            {
                //Alles ins Wohnzimmer legen.
                SonosPlayer primaryplayer = sonosDiscovery.GetPlayerbyName(SonosConstants.WohnzimmerName);
                if (primaryplayer == null)
                {
                    logger.ServerErrorsAdd("GroundFloorOn:Primary", new Exception("primaryplayer konnten nicht ermittelt werden"), "SmartHomeWrapper");
                }
                else
                {
                    await GenericStopOrMakeCoordinatorbySelf(primaryplayer);
                }
                SonosPlayer secondaryplayer = sonosDiscovery.GetPlayerbyName(SonosConstants.EsszimmerName);
                if (secondaryplayer == null)
                {
                    logger.ServerErrorsAdd("GroundFloorOn:secondaryplayer", new Exception("secondaryplayer konnte nicht ermittelt werden"), "SmartHomeWrapper");
                }
                else
                {
                    await GenericStopOrMakeCoordinatorbySelf(secondaryplayer);
                }
                SonosPlayer thirdplayer = sonosDiscovery.GetPlayerbyName(SonosConstants.KücheName);
                if (thirdplayer == null)
                {
                    logger.ServerErrorsAdd("GroundFloorOn:thirdplayer", new Exception("secondaryplayer konnte nicht ermittelt werden"), "SmartHomeWrapper");
                }
                else
                {
                    await GenericStopOrMakeCoordinatorbySelf(thirdplayer);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("GroundFloorOff:Block2", ex, "SmartHomeWrapper");
                return false;
            }
        }

        [HttpGet("FillTimeToChildGenreList")]
        public async Task<bool> FillTimeToChildGenreList()
        {
            try
            {
                var player = sonosDiscovery.GetPlayerbySoftWareGeneration(SoftwareGeneration.ZG1);
                List<SonosItem> genre = await sonosDiscovery.ZoneMethods.Browsing(player, SonosConstants.aGenre + "/Hörspiel", false);
                List<SonosItem> childmusic = await sonosDiscovery.ZoneMethods.Browsing(player, SonosConstants.aGenre + "/Children%e2%80%99s%20Music", false);
                genre = genre.Union(childmusic).ToList();
                int ge = genre.Count - 2;
                if (sonosHelper.ChildGenrelist.Count != ge)
                {
                    if (sonosHelper.ChildGenrelist.Count > ge)
                    {
                        sonosHelper.ChildGenrelist.Clear(); //hier ist ein fehler passiert daher neu.
                    }
                    foreach (SonosItem item in genre)
                    {
                        var title = item.Title;
                        var exist = sonosHelper.ChildGenrelist.FirstOrDefault(x => x.Artist == title);
                        if (exist != null) continue;//wenn schon vorhanden einfach weiter gehen. 

                        if (!string.IsNullOrEmpty(item.AlbumArtURI))
                        {
                            var titem = musicPictures.UpdateItemToHashPath(item);
                            item.AlbumArtURI = titem.AlbumArtURI;
                        }
                        if (title == SonosConstants.aALL) continue;
                        SonosBrowseList sbl = new()
                        {
                            Artist = title,
                            Source = item.ParentID,
                            Childs = await sonosDiscovery.ZoneMethods.Browsing(player, SonosConstants.aAlbumArtist + "/" + title, false)
                        };
                        if (sbl.Childs.Count > 0)
                        {
                            sbl.Childs.RemoveRange(0, 1);
                        }
                        foreach (SonosItem citem in sbl.Childs)
                        {
                            //hier die metadaten holen um die zeit zu bekommen? 
                            if (!string.IsNullOrEmpty(citem.AlbumArtURI))
                            {
                                var titem = musicPictures.UpdateItemToHashPath(citem);
                                citem.AlbumArtURI = titem.AlbumArtURI;
                            }
                        }
                        sonosHelper.ChildGenrelist.Add(sbl);
                    }
                }

                //Zeiten füllen, wenn nicht gefüllt
                foreach (var artistlist in sonosHelper.ChildGenrelist)
                {
                    if(artistlist.Source.EndsWith("Music")) continue;

                    foreach (SonosItem artistchildlist in artistlist.Childs)
                    {
                        TimeSpan tspan = new(0, 0, 0, 0);
                        if (!artistchildlist.Duration.IsZero)
                        {
                            continue;
                        }
                        musicPictures.UpdateItemToHashPath(artistchildlist);
                        var childvalues = await sonosDiscovery.ZoneMethods.Browsing(sonosDiscovery.GetPlayerbySoftWareGeneration(SoftwareGeneration.ZG1), artistchildlist.ContainerID);
                        foreach (var item in childvalues)
                        {
                            try
                            {
                                var meta = await sonosDiscovery.ZoneMethods.Browsing(sonosDiscovery.GetPlayerbySoftWareGeneration(SoftwareGeneration.ZG1), item.ItemID, false, SonosEnums.BrowseFlagData.BrowseMetadata);
                                SonosItem metaitem = meta.FirstOrDefault();
                                if (metaitem != null)
                                    tspan += metaitem.Duration.TimeSpan;
                            }
                            catch (Exception ex)
                            {
                                logger.ServerErrorsAdd("FillTimeToChildGenreList:348:" + item.ItemID, ex, "SmartHomeController");
                                continue;
                            }
                        }
                        artistchildlist.Duration = new SonosTimeSpan(tspan);
                    }
                }




                return true;
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("FillTimeToChildGenreList", ex, "SmartHomeController");
                return false;
            }

        }

        /// <summary>
        /// Wohnzimmer alleine machen und mit einer random Playlist versehen. 
        /// Auf Sonos schalten und alle Lampen ausschalten. 
        /// </summary>
        /// <returns></returns>
        [HttpGet("LivingRoomSpezial")]
        public async Task<Boolean> LivingRoomSpezial()
        {
            Boolean retval = true;
            try
            {
                String selectedplaylistname = String.Empty;
                int ix = -999;
                List<String> RandomPlaylistList = ["4 Sterne", "4.5 Sterne", "5 Sterne", "Amon Armath", "Five Finger Death Punch", "Disturbed", "Foo Fighters", "Harte Gruppen", "Harte Gruppen Genre", "Herbert Grönemeyer", "I Prevail"];
                try
                {
                    //Playlist Ermitteln
                    Random rdm = new();
                    ix = rdm.Next(0, RandomPlaylistList.Count - 1);
                    selectedplaylistname = RandomPlaylistList[ix];
                }
                catch (Exception ex)
                {
                    logger.ServerErrorsAdd("WohnzimmerSpezial:Random", ex, "SmartHomeWrapper");
                }
                //Player vorbereiten.            
                SonosPlayer primaryplayer = sonosDiscovery.GetPlayerbyName(SonosConstants.WohnzimmerName);
                if (primaryplayer == null)
                {
                    logger.ServerErrorsAdd("WohnzimmerSpezial:Primary", new Exception(), "SmartHomeWrapper");
                    return false;
                }
                if (primaryplayer.PlayerProperties.TransportState == SonosEnums.TransportState.PLAYING)
                {
                    await primaryplayer.AVTransport.Pause();
                    await Task.Delay(300);
                }
                try
                {
                    if (primaryplayer.PlayerProperties.ZoneGroupTopology_ZonePlayerUUIDsInGroup.Count > 0)
                    {
                        await primaryplayer.AVTransport.BecomeCoordinatorOfStandaloneGroup();
                    }
                }
                catch (Exception ex)
                {
                    logger.ServerErrorsAdd("WohnzimmerSpezial:ZoneGroupTopology_ZonePlayerUUIDsInGroup", ex, "SmartHomeWrapper");
                }
                if (primaryplayer.PlayerProperties.Volume != SonosConstants.WohnzimmerVolume && primaryplayer.RenderingControl != null)
                {
                    await primaryplayer.RenderingControl.SetVolume(SonosConstants.WohnzimmerVolume);
                }
                //Playlist befüllen
                try
                {
                    if (sonosDiscovery.ZoneProperties.ListOfAllPlaylist.Count == 0)
                        await sonosHelper.GetAllPlaylist();
                }
                catch (Exception ex)
                {
                    logger.ServerErrorsAdd("WohnzimmerSpezial:AllPlaylist", ex, "SmartHomeWrapper");
                }
                var playlist = sonosDiscovery.ZoneProperties.ListOfAllPlaylist.FirstOrDefault(x => x.Title.Equals(selectedplaylistname, StringComparison.CurrentCultureIgnoreCase));
                if (playlist == null)
                {
                    logger.ServerErrorsAdd("WohnzimmerSpezial:Playlist", new Exception("Playlist konnte nicht ermittelt werden.Ermittelter Name:" + selectedplaylistname + " Ermittelter Index:" + ix ?? "Null"), "SmartHomeWrapper");
                    throw new Exception("Playlist konnte nicht ermittelt werden.Ermittelter Name:" + selectedplaylistname + " Ermittelter Index:" + ix ?? "Null");
                }
                Boolean loadPlaylist = sonosHelper.CheckPlaylist(playlist, primaryplayer);
                if (loadPlaylist)
                {
                    if (!await sonosHelper.LoadPlaylist(playlist, primaryplayer))
                        throw new Exception("Playlist konnte nicht geladen werden.");
                }
                await Task.Delay(300);
                try
                {
                    await primaryplayer.AVTransport?.Play();
                }
                catch (Exception ex)
                {
                    logger.ServerErrorsAdd("WohnzimmerSpezial:Play", ex, "SmartHomeWrapper");
                    throw new Exception("Play konnte nicht geladen werden.");
                }

            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("WohnzimmerSpezial:Global", ex, "SmartHomeWrapper");
                throw;
            }
            return retval;
        }
        #endregion Ground
        #region Finn
        [HttpGet("FinnToggleRoom/{id}")]
        public async Task<String> FinnToggleRoom(string id)
        {
            return await GenericToggleRoom(SonosConstants.FinnzimmerName, id, SonosConstants.FinnzimmerVolume);
        }
        [HttpGet("FinnToggleRoom")]
        public async Task<String> FinnToggleRoom()
        {
            return await GenericToggleRoom(SonosConstants.FinnzimmerName, null, SonosConstants.FinnzimmerVolume);
        }
        [HttpGet("FinnVolume/{id}")]
        public async Task<Boolean> FinnVolume(int id)
        {
            return await GenericRoomVolume(SonosConstants.FinnzimmerName, id, 50);
        }
        #endregion Finn
        #region Ian
        [HttpGet("IanToggle")]
        public async Task<String> IanToggle()
        {
            return await GenericToggleRoom(SonosConstants.IanzimmerName, null, SonosConstants.IanzimmerVolume);
        }
        [HttpGet("IanVolume/{id}")]
        public async Task<Boolean> IanVolume(int id)
        {
            int max = 50;
            if (DateTime.Now.Hour > 18 || DateTime.Now.Hour < 7)
                max = 10;

            return await GenericRoomVolume(SonosConstants.IanzimmerName, id, max);
        }
        [HttpGet("IanRoom/{id}")]
        public async Task<Boolean> IanRoom(string id)
        {
            return await GenericRoomOn(id, SonosConstants.IanzimmerName, SonosConstants.IanzimmerVolume);
        }
        [HttpGet("IanRoomNoStart/{id}")]
        public async Task<Boolean> IanRoomNoStart(string id)
        {
            return await GenericRoomOn(id, SonosConstants.IanzimmerName, SonosConstants.IanzimmerVolume, false);
        }
        [HttpGet("IanRoom")]
        public async Task<Boolean> IanRoom()
        {
            return await GenericRoomOn(null, SonosConstants.IanzimmerName, SonosConstants.IanzimmerVolume);
        }
        [HttpGet("IanRoomOff")]
        public async Task<Boolean> IanRoomOff()
        {
            return await GenericRoomOff(SonosConstants.IanzimmerName);
        }
        #endregion Ian
        #region Workroom
        [HttpGet("WorkRoom")]
        public async Task<Boolean> WorkRoom()
        {
            return await GenericRoomOn(null, SonosConstants.ArbeitszimmerName, SonosConstants.ArbeitzimmerVolume);
        }
        [HttpGet("WorkRoomOff")]
        public async Task<Boolean> WorkRoomOff()
        {
            return await GenericRoomOff(SonosConstants.ArbeitszimmerName);
        }
        [HttpGet("WorkRoomVolume/{id}")]
        public async Task<Boolean> WorkRoomVolume(int id)
        {
            return await GenericRoomVolume(SonosConstants.ArbeitszimmerName, id);
        }
        [HttpGet("WorkRoomSongManage/{id}")]
        public async Task<Boolean> WorkRoomSongManage(int id)
        {
            
            SonosPlayer pl = sonosDiscovery.GetPlayerbyName(SonosConstants.ArbeitszimmerName);
            Boolean retval = true;
            switch (id)
            {
                case 1: //Next
                    await pl.AVTransport.Next();
                    break;
                case 2: //Prev
                    await pl.AVTransport.Previous();
                    break;
            }
            return retval;
        }
        [HttpGet("WorkRoomPlaylistManage/{id}")]
        public async Task<Boolean> WorkRoomPlaylistManage(string id)
        {
            
            SonosPlayer pl = sonosDiscovery.GetPlayerbyName(SonosConstants.ArbeitszimmerName);
            Boolean retval = true;
            String selectedplaylistname = id;
            switch (id)
            {
                case "Random": //Next

                    int ix;
                    List<String> RandomPlaylistList = ["Mine", "4.5 Sterne", "5 Sterne", "Amon Armath", "Five Finger Death Punch", "Disturbed", "Foo Fighters", "Harte Gruppen", "Harte Gruppen Genre", "Herbert Grönemeyer", "I Prevail"];
                    try
                    {
                        //Playlist Ermitteln
                        Random rdm = new();
                        ix = rdm.Next(0, RandomPlaylistList.Count - 1);
                        selectedplaylistname = RandomPlaylistList[ix];
                    }
                    catch (Exception ex)
                    {
                        logger.ServerErrorsAdd("WorkRoomPlaylistManage:Random", ex, "SmartHomeWrapper");
                        retval = false;
                    }
                    break;

            }
            try
            {
                await GenericRoomOn(selectedplaylistname, SonosConstants.ArbeitszimmerName, await pl.RenderingControl.GetVolume());
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("WorkRoomPlaylistManage:GenericRoomOn", ex, "SmartHomeWrapper");
                retval = false;
            }
            return retval;
        }
        #endregion Workroom
        #region Guest
        [HttpGet("PlayRoom/{id}/{v}")]
        public async Task<Boolean> PlayRoom(string id, int v)
        {
            
            if (string.IsNullOrEmpty(id)) return false;
            try
            {
                string playlistToPlay = id;
                SonosPlayer pp = sonosDiscovery.GetPlayerbyName(SonosConstants.SpielzimmerName);
                if (pp == null) return false;
                if ((string.IsNullOrEmpty(pp.PlayerProperties.AVTransportURI) || pp.PlayerProperties.AVTransportURI.StartsWith(SonosConstants.xrinconstream)) && pp.AVTransport != null)
                {
                    await pp.AVTransport?.SetAVTransportURI(SonosConstants.xrinconqueue + pp.UUID + "#0");
                }
                if (sonosDiscovery.ZoneProperties.ListOfAllPlaylist.Count == 0)
                {
                    //_logger.TraceLog("GuestRoom", "get allplaylist");
                    await sonosHelper.GetAllPlaylist();
                }
                var playlist = sonosDiscovery.ZoneProperties.ListOfAllPlaylist.FirstOrDefault(x => x.Title.Equals(playlistToPlay, StringComparison.CurrentCultureIgnoreCase));
                if (playlist == null)
                {
                    await sonosHelper.GetAllPlaylist();
                }
                playlist = sonosDiscovery.ZoneProperties.ListOfAllPlaylist.FirstOrDefault(x => x.Title.Equals(playlistToPlay, StringComparison.CurrentCultureIgnoreCase));
                Boolean loadPlaylist = false;
                if (playlist != null)
                {
                    loadPlaylist = sonosHelper.CheckPlaylist(playlist, pp);
                }
                else
                {
                    // _logger.TraceLog("GuestRoom", "playlist:null");
                }
                if (loadPlaylist)
                {
                    if (!await sonosHelper.LoadPlaylist(playlist, pp))
                        return false;
                }
                else
                {
                    if (pp != null && pp.AVTransport != null)
                    {
                        await pp.AVTransport?.Seek("1", SonosEnums.SeekUnit.TRACK_NR);
                        await Task.Delay(100);
                    }
                }
                if (pp.PlayerProperties.Volume != v)
                {
                    try
                    {
                        await pp.GroupRenderingControl?.SetGroupVolume(v);
                    }
                    catch
                    {
                        //ignore
                    }
                    try
                    {
                        await pp.RenderingControl?.SetVolume(v);
                    }
                    catch
                    {
                        //ignore
                    }
                }
                await Task.Delay(700);
                return await pp?.AVTransport?.Play();
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("PlayRoom:" + id, ex, "SmarthomeController");
                throw;
            }
        }
        [HttpGet("PlayRoom/{id}")]
        public async Task<Boolean> PlayRoom(string id)
        {
            
            //_logger.TraceLog("GuestRoom", "Start");
            if (string.IsNullOrEmpty(id)) return false;
            return await PlayRoom(id, SonosConstants.SpielzimmerVolume);

        }
        [HttpGet("PlayRoomOff")]
        public async Task<Boolean> PlayRoomOff()
        {
            return await GenericRoomOff(SonosConstants.SpielzimmerName);
        }
        [HttpGet("PlayRoomAudioInOn")]
        public async Task<Boolean> PlayRoomAudioInOn()
        {
            
            try
            {
                var pl = sonosDiscovery.GetPlayerbyName(SonosConstants.SpielzimmerName);
                var retval = await pl.AVTransport.SetAVTransportURI(SonosConstants.xrinconstream + pl.UUID);
                if (retval)
                {
                    //Neu Wegen Stream
                    //pl.PlayerProperties.CurrentTrack = await SonosItemHelper.CheckItemForStreaming(pl.PlayerProperties.CurrentTrack, pl);
                    return await pl.AVTransport.Play();
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("PlayRoomAudioInOn", ex, "SmarthomeController");
                throw;
            }
        }
        [HttpGet("PlayRoomAudioInOff")]
        public async Task<Boolean> PlayRoomAudioInOff()
        {
            
            try
            {
                var pl = sonosDiscovery.GetPlayerbyName(SonosConstants.SpielzimmerName);
                await pl.AVTransport.Stop();
                var retval = await pl.AVTransport.SetAVTransportURI(SonosConstants.xrinconqueue + pl.UUID + "#0");
                if (retval)
                {
                    //Neu Wegen Stream
                    //pl.PlayerProperties.CurrentTrack = await SonosItemHelper.CheckItemForStreaming(pl.PlayerProperties.CurrentTrack, pl);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("GuestRoomAudioInOff", ex, "SmarthomeController");
                throw;
            }
        }
        #endregion Guest
        [HttpGet("GenericRoomOn/{Playlist}/{Playername}/{Volume}/{startPlaying?}")]
        public async Task<Boolean> GenericRoomOn(string Playlist, string Playername, int Volume, Boolean startPlaying = true)
        {
            try
            {
                if (string.IsNullOrEmpty(Playername)) return false;
                
                SonosPlayer pp = sonosDiscovery.GetPlayerbyName(Playername);
                if (pp == null) return false;
                if (!string.IsNullOrEmpty(Playlist))
                {
                    if (sonosDiscovery.ZoneProperties.ListOfAllPlaylist.Count == 0)
                        await sonosHelper.GetAllPlaylist();
                    var playlist = sonosDiscovery.ZoneProperties.ListOfAllPlaylist.FirstOrDefault(x => x.Title.Equals(Playlist, StringComparison.CurrentCultureIgnoreCase));
                    if (playlist == null)
                    {
                        await sonosHelper.GetAllPlaylist();
                    }
                    playlist = sonosDiscovery.ZoneProperties.ListOfAllPlaylist.FirstOrDefault(x => x.Title.Equals(Playlist, StringComparison.CurrentCultureIgnoreCase));
                    Boolean loadPlaylist = false;
                    if(playlist == null)
                    {
                        return false;
                    }
                    if (playlist != null)
                    {
                        loadPlaylist = sonosHelper.CheckPlaylist(playlist, pp);
                    }
                    if (loadPlaylist)
                    {
                        if (!await sonosHelper.LoadPlaylist(playlist, pp))
                            return false;
                    }
                }
                await pp.GroupRenderingControl?.GetGroupVolume();
                if (pp.PlayerProperties.GroupRenderingControl_GroupVolume != Volume)
                {
                    try
                    {
                        await pp.GroupRenderingControl?.SetGroupVolume(Volume);
                    }
                    catch
                    {
                        //ignore
                    }
                }
                await Task.Delay(300);
                if (!startPlaying) return true;

                return await pp?.AVTransport?.Play();
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("GenericRoomOn", ex, "SmarthomeController");
                throw;
            }
        }

        [HttpGet("GenericRoomOff/{playername}")]
        public async Task<Boolean> GenericRoomOff(string playername)
        {
            
            try
            {
                var pl = sonosDiscovery.GetPlayerbyName(playername);
                if (pl == null) return false;
                if(pl.AVTransport == null)
                {
                    //pl.ServiceCheck();
                    logger.ServerErrorsAdd("GenericRoomOff", new Exception("AvTransport is Null. ServiceInit is:"+pl.ServiceInit), "SmarthomeController");
                    return false;
                }

                return await pl.AVTransport?.Pause();
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("GenericRoomOff", ex, "SmarthomeController");
                throw;
            }
        }
        #region Private Methods
        private async Task<Boolean> GenericStopOrMakeCoordinatorbySelf(SonosPlayer pl)
        {
            try
            {
                if (pl.PlayerProperties.GroupCoordinatorIsLocal)
                {
                    return await pl.AVTransport?.Pause();
                }
                else
                {
                    return await pl.AVTransport?.BecomeCoordinatorOfStandaloneGroup();
                }
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("GenericStopOrMakeCoordinatorbySelf", ex, "SmarthomeController");
                throw;
            }

        }
        /// <summary>
        /// Toggle Playstate for Player
        /// </summary>
        /// <param name="playername">Playername</param>
        /// <param name="playlist">Playlist name (Optional)</param>
        /// <returns></returns>
        private async Task<String> GenericToggleRoom(string playername, string playlist = null, int volume = 20)
        {
            try
            {
                SonosPlayer pp = null;
                try
                {
                    pp = sonosDiscovery.GetPlayerbyName(playername);
                    if (pp == null || pp.AVTransport == null) return "Player null.";
                    await pp.AVTransport.GetTransportInfo();
                    await sonosHelper.WaitForTransitioning(pp);
                }
                catch (Exception ex)
                {
                    logger.ServerErrorsAdd("GenericToggleRoom1", ex, "SmarthomeController");
                    return ex.Message;
                }
                if (pp.PlayerProperties.TransportState == SonosEnums.TransportState.PLAYING)
                {
                    return (await GenericRoomOff(playername)).ToString();
                }
                else
                {
                    return (await GenericRoomOn(playlist, playername, volume)).ToString();
                }
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("GenericToggleRoomLast", ex, "SmarthomeController");
                return ex.Message;
            }
        }

        private async Task<Boolean> GenericRoomVolume(string playername, int volume, int max = 100)
        {
            try
            {
                
                SonosPlayer pl = sonosDiscovery.GetPlayerbyName(playername);
                await pl.GroupRenderingControl?.GetGroupVolume();

                int vol = pl.PlayerProperties.GroupRenderingControl_GroupVolume;
                vol += volume;
                if (vol > max)
                    vol = max;
                if (vol < 1)
                    vol = 1;
                return await pl.GroupRenderingControl?.SetGroupVolume(vol);
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("GenericRoomVolume:Volume:" + volume, ex, "SmarthomeController");
                return false;
            }
        }
        #endregion Private Methods
    }
}
