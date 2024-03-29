using MP3File;
using SonosConst;
using SonosData.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace SonosData
{
    /// <summary>
    /// Informationen �ber ein einzeles Lied oder eine Playlist
    /// </summary>
	public class SonosItem
    {
        #region Klassenvariablen
        /// <summary>
        /// Key mit true hei�t ist stream.
        /// </summary>
        //private Dictionary<String, Boolean> ProtocolInfoToStream = new Dictionary<string, bool>();
        /*
        x-file-cifs:*:audio/flac:* = Flac Normal
        x-file-cifs:*:audio/mpeg: = MP3 Normal
        sonos.com-http:*:audio/flac: = Amazon Musik Song
        x-rincon-mp3radio:*:*:* = tunein Radio
        sonos.com-hls-radio:*:audio/mpegurl:* = tunein Radio
        aac:*:application/octet-stream:* = tunein Radio
        sonos.com-rtrecent:*:audio/x-sonos-recent:* = radio Podcast
        sonos.com-http:*:audio/flac:* = amazon Musik Playlist Song URI beginnt mit x-sonos-http:catalog
        sonos.com-http:*:audio/flac:* = amazon Musik Radio Sender URI beginnt mit x-sonosprog-http:catalog

         * */
        private static readonly XNamespace ns = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/";
        private static readonly XNamespace dc = "http://purl.org/dc/elements/1.1/";
        private static readonly XNamespace upnp = "urn:schemas-upnp-org:metadata-1-0/upnp/";
        private static readonly XNamespace r = "urn:schemas-rinconnetworks-com:metadata-1-0/";
        #endregion Klassenvariablen
        public string Uri { get; set; } = String.Empty;
        [IgnoreDataMember]
        public string MetaData { get; set; } = String.Empty;
        public string AlbumArtURI { get; set; } = String.Empty;
        public string Artist { get; set; } = String.Empty;
        public string Album { get; set; } = String.Empty;
        public string Title { get; set; } = String.Empty;
        /// <summary>
        /// Beschreibung die bei den Playlisten f�r die Typisierung benutzt wird.
        /// </summary>
        public string Description { get; set; } = String.Empty;
        public string ContainerID { get; set; } = String.Empty;
        public string ParentID { get; set; } = String.Empty;
        public string ItemID { get; set; } = String.Empty;
        public MP3File.MP3File MP3 { get; set; } = new MP3File.MP3File();
        /// <summary>
        /// Ist es ein Stream
        /// </summary>
        public bool Stream
        {
            get
            {
                return !ProtocolInfo.StartsWith("x-file-cifs:*:audio");
            }
        }
        public Boolean IsEmpty
        {
            get
            {
                return string.IsNullOrEmpty(Artist) && string.IsNullOrEmpty(Album) && string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Uri);
            }
        }
        public string StreamContent { get; set; } = String.Empty; // Streamconten aufgrund vom Protcoll bef�llen.
        public string ClassType { get; set; } = String.Empty;
        public string ProtocolInfo { get; set; } = String.Empty;
        /// <summary>
        /// aktuelle Wiedergabe Zeit des Tracks
        /// </summary>
        public SonosTimeSpan RelTime { get; set; } = new SonosTimeSpan();
        /// <summary>
        /// Laufzeit des aktuellen Songs
        /// </summary>
        public SonosTimeSpan Duration { get; set; } = new SonosTimeSpan();

        /// <summary>
        /// Konstruktor mit Leerem SonosItem
        /// </summary>
        public SonosItem()
        {
        }
        /// <summary>
        /// Konstruktor mit komplettem SonosItem
        /// </summary>
        /// <param name="_tr"></param>
        public SonosItem(SonosItem _tr)
        {
            Uri = _tr.Uri;
            MetaData = _tr.MetaData;
            Title = _tr.Title;
            AlbumArtURI = _tr.AlbumArtURI;
            Artist = _tr.Artist;
            Album = _tr.Album;
            Description = _tr.Description;
            ContainerID = _tr.ContainerID;
            ParentID = _tr.ParentID;
            ItemID = _tr.ItemID;
            //Stream = _tr.Stream;
            StreamContent = _tr.StreamContent ?? String.Empty;
            ClassType = _tr.ClassType;
        }
        /// <summary>
        /// Liefert eine Liste mit SonosItems zur�ck. (Tracks oder Playlisten)
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
		public static List<SonosItem> Parse(string xmlString)
        {
            var xml = XElement.Parse(xmlString);
            var items = xml.Elements(ns + "item");
            var list = new List<SonosItem>();

            foreach (var item in items)
            {
                try
                {
                    var track = ParseItem(item);
                    if (string.IsNullOrEmpty(track.MetaData))
                    {
                        //Wenn die Metadata nicht bef�llt sind, werden diese selber gebaut
                        string meta = "<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns:r=\"urn:schemas-rinconnetworks-com:metadata-1-0/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\"><item id=\"" + track.ItemID + "\" parentID=\"" + track.ParentID + "\" restricted=\"true\"><dc:title>" + track.Title + "</dc:title><upnp:class>object.item.audioItem.musicTrack</upnp:class><desc id=\"cdudn\" nameSpace=\"urn:schemas-rinconnetworks-com:metadata-1-0/\">RINCON_AssociatedZPUDN</desc></item></DIDL-Lite>";
                        track.MetaData = meta;
                    }
                    list.Add(track);
                }
                catch
                {
                    continue;
                }

            }
            //Playlisten sind nicht item sondern Container
            if (list.Count == 0)
            {
                items = xml.Elements(ns + "container");
                foreach (var item in items)
                {
                    var track = new SonosItem();
                    track.Uri = (string)item.Element(ns + "res") ?? "";
                    track.Title = (string)item.Element(dc + "title") ?? "";
                    track.AlbumArtURI = (string)item.Element(upnp + "albumArtURI") ?? "";
                    track.ClassType = (string)item.Element(upnp + "class") ?? "";

                    if (item.FirstAttribute != null)
                    {
                        track.ContainerID = item.FirstAttribute.Value;
                        track.ParentID = item.FirstAttribute.NextAttribute != null ? item.FirstAttribute.NextAttribute.Value : "";
                    }
                    var tar = (string)item.Element(dc + "creator");
                    if (string.IsNullOrEmpty(tar))
                    {
                        tar = (string)item.Element(upnp + "artist");
                    }
                    track.Artist = tar ?? String.Empty;
                    list.Add(track);
                }
                if (list.Count == 1)
                {
                    //wenn nur ein Eintrag, dann ist die �bergebene variable auch der Metadata eintrag
                    list[0].MetaData = xmlString;
                }
            }
            return list;
        }
        /// <summary>
        /// Ermittelt aus dem DIDL XML ein SonosItem
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static SonosItem ParseSingleItem(string xmlString)
        {
            SonosItem returnval = new();
            if (!string.IsNullOrEmpty(xmlString) && xmlString != SonosConstants.NotImplemented)
            {
                var xml = XElement.Parse(xmlString);
                var items = xml.Elements(ns + "item");
                var list = new List<SonosItem>();
                foreach (var item in items)
                {
                    try
                    {
                        list.Add(ParseItem(item));
                    }
                    catch
                    {
                        continue;
                    }
                    break;
                }

                if (list.Any())
                {
                   return list.First();
                }
            }
            return returnval;
        }

        public static SonosItem ParseItem(XElement item)
        {
            var track = new SonosItem();
            try
            {
                var resElement = item.Element(ns + "res");
                if (resElement != null)
                {
                    track.Uri = resElement.Value;
                    track.ProtocolInfo = (string)resElement.Attribute("protocolInfo")??"";
                    if (TimeSpan.TryParse(resElement.Attribute("duration")?.Value, out TimeSpan tsres))
                        track.Duration = new SonosTimeSpan(tsres);
                }
                track.ItemID = (string)item.Attribute("id") ?? String.Empty;
                track.ParentID = (string)item.Attribute("parentID") ?? String.Empty;
                track.MetaData = (string)item.Element(r + "resMD") ?? String.Empty;
                track.AlbumArtURI = (string)item.Element(upnp + "albumArtURI") ?? String.Empty;
                track.ClassType = (string)item.Element(upnp + "class") ?? String.Empty;
                track.Album = (string)item.Element(upnp + "album") ?? String.Empty;
                var tar = (string)item.Element(dc + "creator");
                if (string.IsNullOrEmpty(tar))
                {
                    tar = (string)item.Element(upnp + "artist");
                }
                track.Artist = tar ?? String.Empty;
                //Title | Wenn Streamcontent vorhanden, dann wird radio abgespielt und der Titel ist falsch. 
                track.StreamContent = (string)item.Element(r + "streamContent") ?? String.Empty;
                track.Title = (string)item.Element(dc + "title") ?? String.Empty;
                track.Description = (string)item.Element(r + "description") ?? String.Empty;

            }
            catch
            {
                return track;
            }
            return track;
        }

        /// <summary>
        /// F�llt den Currenttrack mit Leben f�r die MP3.
        /// </summary>
        /// <returns></returns>
        public Boolean FillMP3AndItemFromHDD()
        {
            try
            {
                if (string.IsNullOrEmpty(Uri)) return false;
                if (Stream || !string.IsNullOrEmpty(StreamContent)) return true;
                var _uri = Uri;
                string RemoveFromUri = "x-file-cifs:";
                _uri = _uri.Replace(RemoveFromUri, "");
                _uri = System.Uri.UnescapeDataString(_uri);
                _uri = _uri.Replace("/", "\\");

                MP3 = MP3ReadWrite.ReadMetaData(_uri);
                if (Duration.IsZero && MP3.Laufzeit != TimeSpan.Zero)
                {
                    Duration = new SonosTimeSpan(MP3.Laufzeit);
                }
                if (String.IsNullOrEmpty(Artist) && !string.IsNullOrEmpty(MP3.Artist))
                    Artist = MP3.Artist;
                if (String.IsNullOrEmpty(Album) && !string.IsNullOrEmpty(MP3.Album))
                    Album = MP3.Album;
                if (String.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(MP3.Titel))
                    Title = MP3.Titel;
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Pr�ft ob der Currenttrack leer ist.
        /// </summary>
        /// <returns></returns>
        public bool IsEmtpy()
        {
            return string.IsNullOrEmpty(Artist) && string.IsNullOrEmpty(Album) && string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Uri);
        }

    }
}