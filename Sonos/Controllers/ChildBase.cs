using Microsoft.AspNetCore.Mvc;
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
using SonosData;
using SonosSQLiteWrapper.Interfaces;
using SonosUPNPCore.Interfaces;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Sonos.Controllers
{
    public class ChildBase(ILogging logger, ISonosHelper sonosHelper, ISonosDiscovery sonosDiscovery, IMusicPictures musicPictures) : Controller
    {

        #region Props
        public static String ChildName { get; set; }
        public static List<SonosItem> ChildPlaylistButtonsIDs { get; set; } = [];
        public static Boolean IsRunning { get; set; } = false;
        public static Dictionary<String, Dictionary<String, List<String>>> RandomPlaylistItems { get; set; } = [];
        #endregion Props

        #region Methods
        
        public async Task<IList<ISonosBrowseList>> Start()
        {
            if (sonosHelper.ChildGenrelist.Count != 0) return sonosHelper.ChildGenrelist;
            List<SonosItem> genre = await sonosDiscovery.ZoneMethods.Browsing(GetChild(), SonosConstants.aGenre + "/Hörspiel", false);
            List<SonosItem> childmusic = await sonosDiscovery.ZoneMethods.Browsing(GetChild(), SonosConstants.aGenre + "/Kids", false);
            genre = genre.Union(childmusic).ToList();
            int ge = genre.Count - 2;
            if (ge == sonosHelper.ChildGenrelist.Count)
            {
                return sonosHelper.ChildGenrelist;//wenn ich hier hin komme, return weil fertig
            }
            if (sonosHelper.ChildGenrelist.Count > ge)
            {
                ResetList(); //hier ist ein fehler passiert daher neu.
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
                var sbl = new SonosBrowseList
                {
                    Artist = title,
                    Source = item.ParentID,
                    Childs = await sonosDiscovery.ZoneMethods.Browsing(GetChild(), SonosConstants.aAlbumArtist + "/" + title, false)
                };
                if (sbl.Childs.Count > 0)
                {
                    sbl.Childs.RemoveRange(0, 1);
                }
                foreach (SonosItem citem in sbl.Childs)
                {
                    if (!string.IsNullOrEmpty(citem.AlbumArtURI))
                    {
                        var titem = musicPictures.UpdateItemToHashPath(citem);
                        citem.AlbumArtURI = titem.AlbumArtURI;
                    }
                }
                sonosHelper.ChildGenrelist.Add(sbl);
            }
            return sonosHelper.ChildGenrelist;
        }
        public async void ReadConfiguration()
        {
            //neue Idee
            SonosPlayer player = GetChild();
            var playlistsToUse = await player.ContentDirectory.Browse(SonosConstants.SQ);
            lock (ChildPlaylistButtonsIDs)
            {
                foreach (SonosItem sonosItem in playlistsToUse.Result)
                {
                    var temptitle = sonosItem.Title.ToLower();
                    if (temptitle.StartsWith("zzz" + ChildName.ToLower()))
                    {
                        var sonosfromlist = ChildPlaylistButtonsIDs.FirstOrDefault(x => x.Title.Equals(temptitle, StringComparison.CurrentCultureIgnoreCase));
                        if (sonosfromlist == null)
                        {
                            ChildPlaylistButtonsIDs.Add(sonosItem);
                        }
                    }
                }
            }
        }
        public SonosPlayer GetChild()
        {
            return sonosDiscovery.GetPlayerbyName(ChildName);
        }
        public void ResetList()
        {
            sonosHelper.ChildGenrelist.Clear();
        }
        public string BaseURL()
        {
            var bu = GetChild();
            return bu.PlayerProperties.BaseUrl;
        }
        public async Task<int> Transport()
        {
            var bu = GetChild();
            if (bu == null || bu.AVTransport == null) return 0;
            var t = await bu.AVTransport?.GetTransportInfo();
            if (t == SonosEnums.TransportState.PLAYING)
                return 1;

            return 0;
        }
        public async Task<QueueData> DefineButton(string id, Boolean RemoveOld, [FromBody] string containerid)
        {
            var player = GetChild();
            SonosItem playlist = ChildPlaylistButtonsIDs.FirstOrDefault(x => x.Title.Equals(id, StringComparison.CurrentCultureIgnoreCase));

            if (playlist == null) return null;
                
            RandomPlaylistItems.Clear();
            //Browse
            var browsedplaylist = await player.ContentDirectory.Browse(playlist.ContainerID);
            var browseduritoadd = await player.ContentDirectory.Browse(containerid, 0, 1, SonosEnums.BrowseFlagData.BrowseMetadata);
            var UpdateID = browsedplaylist.UpdateID;
            var uritoadd = browseduritoadd.Result.FirstOrDefault();
            //wenn nichts gefunden wurde:
            if (uritoadd == null) return null;
            //delete
            if (browsedplaylist.Result.Count > 0 && RemoveOld)
            {
                //delete old stuff
                string toremove = "0";
                if (browsedplaylist.Result.Count > 1)
                {
                    toremove += "-" + (browsedplaylist.Result.Count - 1).ToString();
                }
                var quedata = await player.AVTransport.ReorderTracksInSavedQueue(playlist.ContainerID, UpdateID, toremove, "");
                UpdateID = quedata.NewUpdateID;
            }
            //add
            return await player.AVTransport.AddURIToSavedQueue(playlist.ContainerID, UpdateID, uritoadd.Uri, browseduritoadd.Result[0].MetaData);
        }
        public async Task<Boolean> Pause()
        {
            var bu = GetChild();
            return await bu.AVTransport.Pause();
        }
        public async Task<Boolean> Play()
        {
            var bu = GetChild();
            return await bu.AVTransport.Play();
        }
        public async Task<Boolean> Volume(string id)
        {
            try
            {
                UInt16 volume = 6;
                switch (id)
                {
                    case "2":
                        volume = 10;
                        break;
                    case "3":
                        volume = 15;
                        break;
                }
                var pl = GetChild();
                return await pl.RenderingControl.SetVolume(volume);
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("SetVolume", ex, ChildName+"Controller");
                throw;
            }

        }
        public async Task<Boolean> ReplacePlaylistGet(string pl, int volume)
        {
            try
            {
                SonosPlayer pp = GetChild();
                if (pp == null) return false;
                if (!string.IsNullOrEmpty(pl))
                {
                    if (sonosDiscovery.ZoneProperties.ListOfAllPlaylist.Count == 0)
                        await sonosHelper.GetAllPlaylist();
                    var playlist = sonosDiscovery.ZoneProperties.ListOfAllPlaylist.FirstOrDefault(x => x.Title.Equals(pl, StringComparison.CurrentCultureIgnoreCase));
                    if (playlist == null)
                    {
                        await sonosHelper.GetAllPlaylist();
                    }
                    playlist = sonosDiscovery.ZoneProperties.ListOfAllPlaylist.FirstOrDefault(x => x.Title.Equals(pl, StringComparison.CurrentCultureIgnoreCase));
                    Boolean loadPlaylist = false;
                    if (playlist == null)
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
                if (pp.PlayerProperties.GroupRenderingControl_GroupVolume != volume)
                {
                    try
                    {
                        await pp.GroupRenderingControl?.SetGroupVolume(volume);
                    }
                    catch
                    {
                        //ignore
                    }
                }
                await Task.Delay(300);
                return await pp?.AVTransport?.Play();
            }
            catch (Exception ex)
            {
                logger.ServerErrorsAdd("ReplacePlaylistGet", ex, "Childbase");
                throw;
            }
        }

        public async Task<Boolean> ReplacePlaylist(string v, int volume)
        {
            try
            {
                SonosPlayer pl = GetChild();
                await pl.AVTransport.RemoveAllTracksFromQueue();
                await Task.Delay(300);
                await pl.GroupRenderingControl.SetGroupVolume(volume);
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
                logger.ServerErrorsAdd("ReplacePlaylist:" + v, ex, ChildName+"Controller");
                throw;
            }
        }

        public async Task<Boolean> Random(string _playlist, int volume)
        {
            try
            {
                var player = GetChild();
                if(player == null) return false;
                var randomitems = new Dictionary<String, List<String>>();
                if(RandomPlaylistItems.TryGetValue(_playlist, out Dictionary<string, List<string>> value))
                    randomitems = value;
                if (sonosHelper.ChildGenrelist.Count == 0)
                {
                    await Start();
                }

                if (randomitems.Count == 0)
                {
                    if (ChildPlaylistButtonsIDs.Count == 0)
                    {
                        ReadConfiguration();
                    }


                    SonosItem playlist = ChildPlaylistButtonsIDs.FirstOrDefault(x => x.Title.Equals(_playlist, StringComparison.CurrentCultureIgnoreCase));
                    if (playlist == null) return false;
                    //Browse
                    var browsedplaylist = await player.ContentDirectory.Browse(playlist.ContainerID);
                    foreach (var item in browsedplaylist.Result)
                    {
                        var lis = new List<string>();
                        if (!randomitems.ContainsKey(item.Artist))
                        {
                            lis.Add(item.Album);
                        }
                        else
                        {
                            lis = randomitems[item.Artist];
                        }
                        if (!lis.Contains(item.Album))
                        {
                            lis.Add(item.Album);
                        }
                        randomitems[item.Artist] = lis;
                    }
                }
                RandomPlaylistItems[_playlist] = randomitems;
                Random rand = new();
                List<SonosItem> ItemstoPlay = [];
                foreach (var artist in randomitems.Keys)
                {
                    var selectedartist = sonosHelper.ChildGenrelist.FirstOrDefault(x => x.Artist == artist);
                    var countartist = randomitems[artist].Count;
                    for (int i = 0; i < countartist; i++)
                    {
                        var nextrand = rand.Next(0, selectedartist.Childs.Count-1);
                        var t = selectedartist.Childs[nextrand];
                        ItemstoPlay.Add(t);
                    }
                }
                try
                {
                    await player.AVTransport.RemoveAllTracksFromQueue();
                    await Task.Delay(300);
                    foreach (var item in ItemstoPlay)
                    {
                        await sonosDiscovery.ZoneMethods.AddToQueue(item.ContainerID, player);
                    }

                    
                    if (player.PlayerProperties.AVTransportURI != SonosConstants.xrinconqueue + player.UUID + "#0")
                    {
                        await player.AVTransport.SetAVTransportURI(SonosConstants.xrinconqueue + player.UUID + "#0");
                    }


                    await player.RenderingControl.GetVolume();
                    if (player.PlayerProperties.Volume != volume)
                        await player.RenderingControl.SetVolume(volume);
                    await Task.Delay(300);
                    await player.AVTransport.Play();
                }
                catch (Exception ex)
                {
                     logger.ServerErrorsAdd("Random:AVTransport", ex, "Childbase");
                    return false;
                }
                return true;
            }
            catch(Exception ex)
            {
                 logger.ServerErrorsAdd("Random", ex, "Childbase");
                return false;
            }

        }
        #endregion Methods
    }



}
