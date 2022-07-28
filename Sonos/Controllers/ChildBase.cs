using Microsoft.AspNetCore.Mvc;
using Sonos.Classes;
using SonosUPnP;
using SonosUPnP.DataClasses;
using SonosConst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeLogging;
using Sonos.Classes.Interfaces;

namespace Sonos.Controllers
{
    public class ChildBase : Controller
    {

        #region Props
        public static String ChildName { get; set; }
        public static List<SonosItem> ChildPlaylistButtonsIDs { get; set; } = new();
        public static Boolean IsRunning { get; set; } = false;
        public static Dictionary<String, Dictionary<String, List<String>>> RandomPlaylistItems { get; set; } = new();
        private readonly ILogging _logger;
        private readonly ISonosHelper _sonosHelper;
        private readonly ISonosDiscovery _sonos;
        #endregion Props

        public ChildBase(ILogging log, ISonosHelper sonosHelper, ISonosDiscovery sonos)
        {
            _logger = log;
            _sonosHelper = sonosHelper;
            _sonos = sonos;
        }

        #region Methods
        public async Task<IList<ISonosBrowseList>> Start()
        {
            List<SonosItem> genre = await _sonos.ZoneMethods.Browsing(GetChild(), SonosConstants.aGenre + "/Hörspiel", false);
            int ge = genre.Count - 1;
            if (ge == _sonosHelper.ChildGenrelist.Count)
            {
                return _sonosHelper.ChildGenrelist;//wenn ich hier hin komme, return weil fertig
            }
            if (_sonosHelper.ChildGenrelist.Count > ge)
            {
                ResetList(); //hier ist ein fehler passiert daher neu.
            }
            foreach (SonosItem item in genre)
            {
                var title = item.Title;
                var exist = _sonosHelper.ChildGenrelist.FirstOrDefault(x => x.Artist == title);
                if (exist != null) continue;//wenn schon vorhanden einfach weiter gehen. 

                //if (!string.IsNullOrEmpty(item.AlbumArtURI))
                //{
                //    var titem = await MusicPictures.UpdateItemToHashPath(item);
                //    item.AlbumArtURI = titem.AlbumArtURI;
                //}
                if (title == SonosConstants.aALL) continue;
                var sbl = new SonosBrowseList
                {
                    Artist = title,
                    Childs = await _sonos.ZoneMethods.Browsing(GetChild(), SonosConstants.aAlbumArtist + "/" + title, false)
                };
                if (sbl.Childs.Count > 0)
                {
                    sbl.Childs.RemoveRange(0, 1);
                }
                foreach (SonosItem citem in sbl.Childs)
                {
                    //hier die metadaten holen um die zeit zu bekommen? 
                    var cilditemchildslist = await _sonos.ZoneMethods.Browsing(GetChild(), citem.ContainerID, false);
                    //if (!string.IsNullOrEmpty(citem.AlbumArtURI))
                    //{
                    //    var titem = await MusicPictures.UpdateItemToHashPath(citem);
                    //    citem.AlbumArtURI = titem.AlbumArtURI;
                    //}
                }
                _sonosHelper.ChildGenrelist.Add(sbl);
            }
            return _sonosHelper.ChildGenrelist;
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
                        var sonosfromlist = ChildPlaylistButtonsIDs.FirstOrDefault(x => x.Title.ToLower() == temptitle);
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
            return _sonos.GetPlayerbyName(ChildName);
        }
        public void ResetList()
        {
            _sonosHelper.ChildGenrelist.Clear();
        }
        public string BaseURL()
        {
            var bu = GetChild();
            return bu.PlayerProperties.BaseUrl;
        }
        public async Task<int> Transport()
        {
            var bu = GetChild();
            var t = await bu.AVTransport?.GetTransportInfo();
            if (t == SonosEnums.TransportState.PLAYING)
                return 1;

            return 0;
        }
        public async Task<QueueData> DefineButton(string id, Boolean RemoveOld, [FromBody] string containerid)
        {
            var player = GetChild();
            SonosItem playlist = ChildPlaylistButtonsIDs.FirstOrDefault(x => x.Title.ToLower() == id.ToLower());

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
                var pl = GetChild();
                return await pl.RenderingControl.SetVolume(volume);
            }
            catch (Exception ex)
            {
                _logger.ServerErrorsAdd("SetVolume", ex, ChildName+"Controller");
                throw;
            }

        }
        public async Task<Boolean> ReplacePlaylist([FromBody] string v, int volume)
        {
            try
            {
                SonosPlayer pl = GetChild();
                await pl.AVTransport.RemoveAllTracksFromQueue();
                await Task.Delay(300);
                await pl.GroupRenderingControl.SetGroupVolume(volume);
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
                _logger.ServerErrorsAdd("ReplacePlaylist:" + v, ex, ChildName+"Controller");
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
                if(RandomPlaylistItems.ContainsKey(_playlist))
                    randomitems = RandomPlaylistItems[_playlist];
                if (!_sonosHelper.ChildGenrelist.Any())
                {
                    await Start();
                }

                if (randomitems.Count == 0)
                {
                    if (!ChildPlaylistButtonsIDs.Any())
                    {
                        ReadConfiguration();
                    }


                    SonosItem playlist = ChildPlaylistButtonsIDs.FirstOrDefault(x => x.Title.ToLower() == _playlist.ToLower());
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
                    var selectedartist = _sonosHelper.ChildGenrelist.FirstOrDefault(x => x.Artist == artist);
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
                    foreach (var item in ItemstoPlay)
                    {
                        await player.AVTransport.AddURIToQueue(item);
                    }
                    await player.RenderingControl.GetVolume();
                    if (player.PlayerProperties.Volume != volume)
                        await player.RenderingControl.SetVolume(volume);

                    await player.AVTransport.Play();
                }
                catch (Exception ex)
                {
                     _logger.ServerErrorsAdd("Random:AVTransport", ex, "Childbase");
                    return false;
                }
                return true;
            }
            catch(Exception ex)
            {
                 _logger.ServerErrorsAdd("Random", ex, "Childbase");
                return false;
            }

        }
        #endregion Methods
    }



}
