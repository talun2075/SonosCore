using MP3File;
using SonosConst;
using SonosData;
using SonosData.DataClasses;
using SonosUPnP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SonosUPNPCore.Classes
{

    /// <summary>
    /// Übernimmt Verschachtelungen um den SonosPlayer zu entlasten.
    /// </summary>
    public class ZoneMethods
    {
        private const string ClassName = "ZoneMethods";
        /// <summary>
        /// Generiert aufgrund der Übergabe ein Favoriten DIDL
        /// </summary>
        /// <param name="pl">SonosPlayer</param>
        /// <param name="objectID">Sonos ID des zufügenden Elements</param>
        /// <returns></returns>
        public async Task<bool> CreateFavorite(SonosPlayer pl, string objectID)
        {
            if (pl == null) return false;
            if (pl.ContentDirectory.ContentDirectoryService == null)
            {
                pl.ServerErrorsAdd("CreateFavorite", ClassName, new Exception("CreateFavorite:ContentDirectory ist null"));
                return false;
            }
            try
            {
                string description;
                string didlmd;
                string didlstring;
                string didlmdItemID;
                string didlstringItemId;
                //Prüfen, ob es ein Song ist
                if (objectID.StartsWith("x-file-cifs"))
                {
                    objectID = objectID.Replace("x-file-cifs", "S");
                }
                var br = await pl.ContentDirectory.Browse(objectID, 0, 0, SonosEnums.BrowseFlagData.BrowseMetadata);
                SonosItem item = br.Result.First(); ;
                switch (item.ClassType)
                {
                    //hie nun die upno Class ermitteln
                    case "object.container.genre.musicGenre":
                    case "object.container.person.musicArtist":
                    case "object.container.playlistContainer":
                        if (item.ClassType.EndsWith("musicGenre"))
                        {
                            description = "Musikrichtung";
                            didlmdItemID = item.ContainerID;
                            didlstringItemId = SonosConstants.xrinconplaylist + pl.UUID + "#" + item.ContainerID;
                        }
                        else if (item.ClassType.EndsWith("musicArtist"))
                        {
                            description = "Interpret";
                            didlmdItemID = item.ContainerID;
                            didlstringItemId = SonosConstants.xrinconplaylist + pl.UUID + "#" + item.ContainerID;
                        }
                        else
                        {
                            description = "Musikbibliothek Playliste";
                            didlmdItemID = item.ItemID;
                            didlstringItemId = item.Uri;
                        }
                        didlmd = "<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns:r=\"urn:schemas-rinconnetworks-com:metadata-1-0/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\">" +
                                 "<item id=\"" + didlmdItemID + "\" parentID=\"" + item.ParentID + "\" restricted=\"true\"><dc:title>" + item.Title + "</dc:title><upnp:class>" + item.ClassType + "</upnp:class><desc id=\"cdudn\" nameSpace=\"urn:schemas-rinconnetworks-com:metadata-1-0/\">RINCON_AssociatedZPUDN</desc></item></DIDL-Lite>";
                        didlstring = "<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\"    xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\"    xmlns:r=\"urn:schemas-rinconnetworks-com:metadata-1-0/\"    xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\">" +
                                     "<item><dc:title>" + item.Title + "</dc:title>" +
                                     "<r:type>instantPlay</r:type><res protocolInfo=\"" + SonosConstants.xrinconplaylist + "*:*:*\">" + didlstringItemId + "</res><r:description>" + description + "</r:description>" +
                                     "<r:resMD>" + HttpUtility.HtmlEncode(didlmd) + "</r:resMD></item></DIDL-Lite>";
                        break;
                    case "object.item.audioItem.musicTrack":
                    case "object.container.album.musicAlbum":
                        var br2 = await pl.ContentDirectory.Browse(item.ParentID, 0, 0, SonosEnums.BrowseFlagData.BrowseMetadata);
                        SonosItem parentItem = br2.Result.First();
                        if (item.ClassType.EndsWith("musicTrack"))
                        {
                            description = "Track von " + item.Artist;
                            didlmdItemID = item.ItemID;
                            didlstringItemId = item.Uri;
                        }
                        else
                        {
                            description = "Album von " + parentItem.Title;
                            didlmdItemID = item.ContainerID;
                            didlstringItemId = SonosConstants.xrinconplaylist + pl.UUID + "#" + item.ContainerID;
                        }

                        didlmd = "<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns:r=\"urn:schemas-rinconnetworks-com:metadata-1-0/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\">" +
                                 "<item id=\"" + didlmdItemID + "\" parentID=\"" + item.ParentID + "\" restricted=\"true\"><dc:title>" + item.Title + "</dc:title><upnp:class>" + item.ClassType + "</upnp:class><desc id=\"cdudn\" nameSpace=\"urn:schemas-rinconnetworks-com:metadata-1-0/\">RINCON_AssociatedZPUDN</desc></item></DIDL-Lite>";
                        didlstring = "<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\"    xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\"    xmlns:r=\"urn:schemas-rinconnetworks-com:metadata-1-0/\"    xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\">" +
                                     "<item><dc:title>" + item.Title + "</dc:title><r:type>instantPlay</r:type>" + "<upnp:albumArtURI>" + item.AlbumArtURI.Replace("&", "&amp;") + "</upnp:albumArtURI>" +
                                     "<res protocolInfo=\"" + SonosConstants.xrinconplaylist + "*:*:*\">" + didlstringItemId + "</res><r:description>" + description + "</r:description>" +
                                     "<r:resMD>" + HttpUtility.HtmlEncode(didlmd) + "</r:resMD></item></DIDL-Lite>";

                        break;
                    default:
                        return false;
                }
                return await pl.ContentDirectory.CreateObject(didlstring);
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("CreateFavorite", ClassName, ex);
                return false;
            }
        }
        public async Task<bool> AddToQueue(string v, SonosPlayer pl)
        {
            if (pl == null) return false;
            if (!pl.RatingFilter.IsDefault && !v.StartsWith(SonosConstants.SQ) && !v.StartsWith(SonosConstants.FV2))
            {
                //Wenn es ein Genre ist, wird bei Klick auf ALL unabhängig der Ebene der Filter angewendet. Bei Interpreten führt das zu Fehlern.
                if (v.StartsWith(SonosConstants.aGenre))
                {
                    int count;
                    if (v.Substring(v.Length - 1) == "/")
                    {
                        //Letztes Zeichen ist ein / und es handelt sich um ALL als Auswahl
                        count = v.Count(c => c == '/');
                        if (count == 2)
                        {
                            v += "/";
                        }

                    }
                    else
                    {
                        //Kein Slash am Ende aber in der Mitte, es wird eines zugefügt, weil wir uns im Root befinden und bei Über gabe ohne würde browse zuviel ermitteln.
                        //Zwei weil es sich um ALL handelt
                        count = v.Count(c => c == '/');
                        if (count == 1)
                        {
                            v += "//";
                        }
                        if (count == 2)
                        {
                            v += "/";
                        }
                    }
                }
                if (v.StartsWith(SonosConstants.aAlbumArtist))
                {
                    if (v.Substring(v.Length - 1) != "/")
                    {
                        int count = v.Count(c => c == '/');
                        //Root von Artist
                        if (count == 1)
                        {
                            v += "/";
                        }
                    }
                }
                if (v.StartsWith(SonosConstants.xfilecifs))
                {
                    //Es soll ein bereits gefilterter Song genommen werden, daher muß hier kein Browsing gemacht werden und es ist eine URI
                    await pl.AVTransport.AddURIToQueue(new SonosItem { Uri = v });
                }
                else
                {
                    IList<SonosItem> k = await Browsing(pl, v, true);
                    SonosItem multi = new();
                    int counter = 0;
                    foreach (SonosItem item in k)
                    {
                        //  DevicesController.Sonos.Players[id].Enqueue(item);
                        multi.Uri += item.Uri + " ";
                        multi.MetaData += item.MetaData + " ";
                        counter++;
                        if (counter == 10)
                        {
                            //Zwischendurch absetzen, weil Metadata auf dem Sonos Maximiert ist und diese sonst zu groß sind.
                            await pl.AVTransport.AddMultipleURIsToQueue(counter, multi);
                            multi.Uri = string.Empty;
                            multi.MetaData = string.Empty;
                            counter = 0;
                        }
                    }
                    if (counter > 0)
                    {
                        await pl.AVTransport.AddMultipleURIsToQueue(counter, multi);
                    }
                }
            }
            else
            {

                if (v.StartsWith(SonosConstants.SQ))
                {
                    //Sonos Playlisten werden nie gefiltert.
                    var br = await pl.ContentDirectory.Browse(v, 0, 0, SonosEnums.BrowseFlagData.BrowseMetadata);
                    var sonospl = br.Result.First();
                    await pl.AVTransport.AddURIToQueue(sonospl);
                }
                //Favoriten
                else if (v.StartsWith(SonosConstants.FV2))
                {
                    var favbr = await pl.ContentDirectory.Browse(v, 0, 0, SonosEnums.BrowseFlagData.BrowseMetadata);
                    var favpl = favbr.Result.First();
                    if (favpl.Uri.StartsWith(SonosConstants.xsonosapistream))
                    {
                        //RadioStream
                        favpl.Artist = favpl.Title;
                        await pl.AVTransport.SetAVTransportURI(favpl.Uri, favpl.MetaData);
                        await Task.Delay(100);
                        await pl.AVTransport.Play();
                        return true;
                    }
                    await pl.AVTransport.AddURIToQueue(favpl);
                }
                else
                {
                    {
                        //Kein Filter angesetzt und alles außer einer Sonos Playlist
                        string rinconpl = SonosConstants.ContainertoURI(v, pl.UUID);
                        await pl.AVTransport.AddURIToQueue(new SonosItem { Uri = rinconpl }, true);
                    }
                }
            }
            if (pl.PlayerProperties.TransportState != SonosEnums.TransportState.PLAYING)
            {
                await pl.AVTransport.Play();
            }
            return true;
        }

        /// <summary>
        /// Durchsucht die Bibliothekt nach dem Übergebenen String und prüft evtl. Filterregeln.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public async Task<List<SonosItem>> Browsing(SonosPlayer pl, string v, bool useRating = false, SonosEnums.BrowseFlagData bfd = SonosEnums.BrowseFlagData.BrowseDirectChildren,int requestLimit = 200)
        {
            if (pl == null) return null;
            List<SonosItem> browselist = new();
            int NumberReturned = 0;
            int TotalMatches = -1;
            while (NumberReturned != TotalMatches)
            {
                var browseresults = await pl.ContentDirectory.Browse(v, NumberReturned, requestLimit, bfd);
                NumberReturned += browseresults.NumberReturned;
                TotalMatches = browseresults.TotalMatches;
                if (browseresults.Result.Count > 0)
                {
                    browselist.AddRange(browseresults.Result);
                }
                else
                {
                    break;//kein ergebnis, daher abbrechen.
                }
            }
            if (useRating)
            {
                List<SonosItem> itemstodelete = new();

                browselist = RateList(browselist, itemstodelete, pl.RatingFilter, pl);
                if (itemstodelete.Count > 0)
                {
                    foreach (SonosItem item in itemstodelete)
                    {
                        browselist.Remove(item);
                    }
                }
            }
            return browselist;
        }
        /// <summary>
        /// Durchläuft die Liste und gibt diese zurück mit Ratings und Gelegeneheiten, falls vorhanden
        /// </summary>
        /// <param name="_list">Liste mit Sonositems (z.B. durch Browse geliefert)</param>
        /// <param name="_del">Liste mit Sonositems die gefiltert werden müssen. oder Null</param>
        /// <param name="srf">SonosRatingFilter zum Prüfen ob der Filter zieht.</param>
        /// <returns></returns>
        private static List<SonosItem> RateList(List<SonosItem> _list, List<SonosItem> _del, SonosRatingFilter srf, SonosPlayer sp)
        {
            try
            {
                foreach (SonosItem item in _list)
                {
                    MP3File.MP3File lied = new();
                    //Wenn kein Song weiter machen.
                    if (!string.IsNullOrEmpty(item.ContainerID))
                    {
                        item.MP3 = lied;
                        continue;
                    }

                    try
                    {
                        if (item.Uri != null)
                        {
                            string itemp = SonosConstants.URItoPath(item.Uri);
                            lied = MP3ReadWrite.ReadMetaData(itemp);
                        }
                        else
                        {
                            lied.VerarbeitungsFehler = true;
                        }

                    }
                    catch
                    {
                        lied.VerarbeitungsFehler = true;
                    }
                    item.MP3 = lied;
                    if (!srf.IsDefault && _del != null)
                    {
                        if (!srf.CheckSong(lied))
                        {
                            _del.Add(item);
                        }

                    }
                } //Foreach alle Items
                return _list;
            }
            catch (Exception x)
            {
                sp.ServerErrorsAdd("RateList", ClassName, x);
                return _list;
            }
        }
    }
}
