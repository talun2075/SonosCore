using Microsoft.AspNetCore.Mvc;
using Sonos.Classes;
using SonosUPnP;
using SonosUPnP.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sonos.Controllers
{
    public class ChildBase : Controller
    {

        #region Props
        public static String ChildName { get; set; }
        public static List<SonosItem> ChildPlaylistButtonsIDs { get; set; } = new();
        public static Boolean IsRunning { get; set; } = false;
        //public static Dictionary<String, List<String>> RandomItems { get; set; } = new();
        public static Dictionary<String, Dictionary<String, List<String>>> RandomPlaylistItems { get; set; } = new();
        #endregion Props


        #region Methods
        public async Task<IList<SonosBrowseList>> Start()
        {
            List<SonosItem> genre = await SonosHelper.Sonos.ZoneMethods.Browsing(await GetChild(), SonosConstants.aGenre + "/Hörspiel", false);
            int ge = genre.Count - 1;
            if (ge == SonosHelper.ChildGenrelist.Count)
            {
                return SonosHelper.ChildGenrelist;//wenn ich hier hin komme, return weil fertig
            }
            if (SonosHelper.ChildGenrelist.Count > ge)
            {
                ResetList(); //hier ist ein fehler passiert daher neu.
            }
            foreach (SonosItem item in genre)
            {
                var title = item.Title;
                var exist = SonosHelper.ChildGenrelist.FirstOrDefault(x => x.Artist == title);
                if (exist != null) continue;//wenn schon vorhanden einfach weiter gehen. 

                if (!string.IsNullOrEmpty(item.AlbumArtURI))
                {
                    var titem = await MusicPictures.UpdateItemToHashPath(item);
                    item.AlbumArtURI = titem.AlbumArtURI;
                }
                if (title == SonosConstants.aALL) continue;
                var sbl = new SonosBrowseList() { Artist = title };
                sbl.Childs = await SonosHelper.Sonos.ZoneMethods.Browsing(await GetChild(), SonosConstants.aAlbumArtist + "/" + title, false);
                if (sbl.Childs.Count > 0)
                {
                    sbl.Childs.RemoveRange(0, 1);
                }
                foreach (SonosItem citem in sbl.Childs)
                {
                    //hier die metadaten holen um die zeit zu bekommen? 
                    var cilditemchildslist = await SonosHelper.Sonos.ZoneMethods.Browsing(await GetChild(), citem.ContainerID, false);
                    if (!string.IsNullOrEmpty(citem.AlbumArtURI))
                    {
                        var titem = await MusicPictures.UpdateItemToHashPath(citem);
                        citem.AlbumArtURI = titem.AlbumArtURI;
                    }
                }
                SonosHelper.ChildGenrelist.Add(sbl);
            }
            return SonosHelper.ChildGenrelist;
        }
        public async void ReadConfiguration()
        {
            //neue Idee
            SonosPlayer player = await GetChild();
            var playlistsToUse = await player.ContentDirectory.Browse(SonosConstants.SQ);
            foreach (SonosItem sonosItem in playlistsToUse.Result)
            {
                if (sonosItem.Title.StartsWith("zzz" + ChildName))
                {
                    var sonosfromlist = ChildPlaylistButtonsIDs.FirstOrDefault(x => x.Title == sonosItem.Title);
                    if (sonosfromlist == null)
                    {
                        ChildPlaylistButtonsIDs.Add(sonosItem);
                    }
                }
            }
        }
        public async Task<SonosPlayer> GetChild()
        {
            await SonosHelper.CheckSonosLiving();
            return await SonosHelper.GetPlayerbyName(ChildName);
        }
        public void ResetList()
        {
            SonosHelper.ChildGenrelist.Clear();
        }
        public async Task<string> BaseURL()
        {
            var bu = await GetChild();
            return bu.PlayerProperties.BaseUrl;
        }
        public async Task<int> Transport()
        {
            var bu = await GetChild();
            var t = await bu.AVTransport?.GetTransportInfo();
            if (t == SonosEnums.TransportState.PLAYING)
                return 1;

            return 0;
        }
        public async Task<QueueData> DefineButton(string id, Boolean RemoveOld, [FromBody] string containerid)
        {
            var player = await GetChild();
            SonosItem playlist = ChildPlaylistButtonsIDs.FirstOrDefault(x => x.Title == id);

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
            var bu = await GetChild();
            return await bu.AVTransport.Pause();
        }
        public async Task<Boolean> Volume(string id)
        {
            try
            {
                UInt16 volume = 5;
                switch (id)
                {
                    case "2":
                        volume = 10;
                        break;
                    case "3":
                        volume = 15;
                        break;
                }
                var pl = await GetChild();
                return await pl.RenderingControl.SetVolume(volume);
            }
            catch (Exception ex)
            {
                SonosHelper.Logger.ServerErrorsAdd("SetVolume", ex, ChildName+"Controller");
                throw;
            }

        }
        public async Task<Boolean> ReplacePlaylist([FromBody] string v, int volume)
        {
            try
            {
                SonosPlayer pl = await GetChild();
                await pl.AVTransport.RemoveAllTracksFromQueue();
                await Task.Delay(300);
                await pl.GroupRenderingControl.SetGroupVolume(volume);
                await SonosHelper.Sonos.ZoneMethods.AddToQueue(v, pl);
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
                SonosHelper.Logger.ServerErrorsAdd("ReplacePlaylist:" + v, ex, ChildName+"Controller");
                throw;
            }
        }
        
        public async Task<Boolean> Random(string _playlist, int volume)
        {
            try
            {
                var player = await GetChild();
                var randomitems = new Dictionary<String, List<String>>();
                if(RandomPlaylistItems.ContainsKey(_playlist))
                    randomitems = RandomPlaylistItems[_playlist];

                if (randomitems.Count == 0)
                {
                   
                    SonosItem playlist = ChildPlaylistButtonsIDs.FirstOrDefault(x => x.Title == _playlist);
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
                Random rand = new Random();
                List<SonosItem> ItemstoPlay = new();
                foreach (var artist in randomitems.Keys)
                {
                    var selectedartist = SonosHelper.ChildGenrelist.FirstOrDefault(x => x.Artist == artist);
                    var countartist = randomitems[artist].Count;
                    for (int i = 0; i < countartist; i++)
                    {
                        var nextrand = rand.Next(0, selectedartist.Childs.Count-1);
                        var t = selectedartist.Childs[nextrand];
                        ItemstoPlay.Add(t);
                    }
                }
                await player.AVTransport.RemoveAllTracksFromQueue();
                foreach (var item in ItemstoPlay)
                {
                    await player.AVTransport.AddURIToQueue(item);
                }
                await player.RenderingControl.GetVolume();
                if(player.PlayerProperties.Volume != volume)
                    await player.RenderingControl.SetVolume(volume);

                await player.AVTransport.Play();
                return true;
            }
            catch
            {
                return false;
            }

        }
        //public async Task<Boolean> Randombackup(string _playlist, int volume)
        //{
        //    try
        //    {
        //        var player = await GetChild();
        //        if (RandomItems.Count == 0)
        //        {

        //            SonosItem playlist = ChildPlaylistButtonsIDs.FirstOrDefault(x => x.Title == _playlist);
        //            if (playlist == null) return false;
        //            //Browse
        //            var browsedplaylist = await player.ContentDirectory.Browse(playlist.ContainerID);
        //            foreach (var item in browsedplaylist.Result)
        //            {
        //                var lis = new List<string>();
        //                if (!RandomItems.ContainsKey(item.Artist))
        //                {
        //                    lis.Add(item.Album);
        //                }
        //                else
        //                {
        //                    lis = RandomItems[item.Artist];
        //                }
        //                if (!lis.Contains(item.Album))
        //                {
        //                    lis.Add(item.Album);
        //                }
        //                RandomItems[item.Artist] = lis;
        //            }
        //        }
        //        Random rand = new Random();
        //        List<SonosItem> ItemstoPlay = new();
        //        foreach (var artist in RandomItems.Keys)
        //        {
        //            var selectedartist = SonosHelper.ChildGenrelist.FirstOrDefault(x => x.Artist == artist);
        //            var countartist = RandomItems[artist].Count;
        //            for (int i = 0; i < countartist; i++)
        //            {
        //                var nextrand = rand.Next(0, selectedartist.Childs.Count - 1);
        //                var t = selectedartist.Childs[nextrand];
        //                ItemstoPlay.Add(t);
        //            }
        //        }
        //        await player.AVTransport.RemoveAllTracksFromQueue();
        //        foreach (var item in ItemstoPlay)
        //        {
        //            await player.AVTransport.AddURIToQueue(item);
        //        }
        //        await player.RenderingControl.GetVolume();
        //        if (player.PlayerProperties.Volume != volume)
        //            await player.RenderingControl.SetVolume(volume);

        //        await player.AVTransport.Play();
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }

        //}
        #endregion Methods
    }



}
