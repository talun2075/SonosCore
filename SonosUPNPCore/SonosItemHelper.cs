using SonosUPnP.DataClasses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace SonosUPnP
{
    public static class SonosItemHelper
    {

        private const string ClassName = "SonosItemHelper";
        private const string audio = "Audio Eingang";
        private const string radio = "Radio";
        private const string service = "Dienst";
        private const string xsonosapi = "x-sonosapi";
        private const string xsonosapiradio = xsonosapi + "-radio";
        private const string xsonosapistream = xsonosapi + "-stream";
        private const string xsonosapihlsstatic = xsonosapi + "-hls-static";
        private static readonly List<string> _bekannteStreamingPfade = new() { "x-rincon-stream:RINCON", "x-rincon-mp3radio", xsonosapiradio, xsonosapihlsstatic, xsonosapistream, "x-sonos-http", "x-sonosprog-http", "aac:" };
        /// <summary>
        /// Prüft ob ein Item ein Streaming Item (Stream, Dienst wie Amazon) ist
        /// </summary>
        /// <param name="si">Zu bearbeitendes SonosItems</param>
        /// <param name="pl">Player um Prüfungen vorzunehmen.</param>
        /// <returns>Bearbeitetes SonosItem</returns>
        public static async Task<SonosItem> CheckItemForStreaming(SonosItem si, SonosPlayer pl)
        {
            if (pl == null) return si;
            try
            {
                if (CheckItemForStreamingUriCheck(si.Uri))
                {
                    //si.Stream = true;
                    if (si.Uri.StartsWith("x-rincon-stream:RINCON"))
                    {
                        //Eingang eines Players
                        si.StreamContent = audio;
                        si.Title = audio;
                        si.AlbumArtURI = "/Images/35klinke.png";
                    }
                    else
                    {
                        if (si.StreamContent == audio)
                        {
                            si.StreamContent = String.Empty;
                        }
                    }
                    if (si.Uri.StartsWith(xsonosapistream) || si.Uri.StartsWith(xsonosapiradio) ||
                        si.Uri.StartsWith("aac:") || si.Uri.StartsWith("x-rincon-mp3radio"))
                    {
                        //Radio
                        si = await GetStreamRadioStuff(si, pl);
                    }
                    MediaInfo minfo = await pl.AVTransport.GetMediaInfo();
                    if (si.Uri.StartsWith("x-sonos-http:") || si.Uri.StartsWith(xsonosapihlsstatic))
                    {
                        //HTTP Dienst wie Amazon
                        si.StreamContent = service;
                        if (minfo.URI.StartsWith(xsonosapiradio))
                        {
                            si.ClassType = "object.item.audioItem.audioBroadcast";
                        }
                    }
                    if (si.Uri.StartsWith("x-sonosprog-http:song") || si.Uri.StartsWith("x-sonos-http:song"))
                    {
                        //HTTP Dienst Apple
                        //prüfen ob Apple Radio
                        if (minfo.URI.StartsWith(xsonosapiradio))
                        {
                            si.StreamContent = radio;
                        }
                        else
                        {
                            si.StreamContent = "Apple";
                        }

                    }
                }
                else
                {
                    if (si.StreamContent == audio)
                    {
                        si.StreamContent = String.Empty;
                        //si.Stream = false;
                    }
                }
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("SonosItemHelper:CheckItemForStreaming", ClassName, ex);
            }
            return si;
        }
        /// <summary>
        /// Prüft ob es sich bei der uri um einen Streamingpfad handelt.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Boolean CheckItemForStreamingUriCheck(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                return false;

            foreach (string s in _bekannteStreamingPfade)
            {
                if (uri.Contains(s))
                    return true;
            }

            return false;
        }
        /// <summary>
        /// Baut Cover, Titel und Artist fpr Radio Sender auf.
        /// </summary>
        /// <param name="si"></param>
        /// <param name="pl"></param>
        /// <returns></returns>
        private static async Task<SonosItem> GetStreamRadioStuff(SonosItem si, SonosPlayer pl)
        {
            try
            {
                try
                {

                    if (si.Title.StartsWith(xsonosapi) || si.Title == "Playlist" || !CheckRadioTitle(si.Title))
                    {
                        si.Title = String.Empty;
                    }
                    if (CheckRadioTitle(si.StreamContent))
                    {
                        si.Title = si.StreamContent;
                    }
                    si.StreamContent = radio;
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("SonosItemHelper:GetStreamRadioStuff:Block1", ClassName, ex);
                }
                MediaInfo k =await pl.AVTransport.GetMediaInfo();
                try
                {
                    
                    if (!string.IsNullOrEmpty(k.URI))
                    {
                        si.AlbumArtURI = "/getaa?s=1&u=" + k.URI;
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("SonosItemHelper:GetStreamRadioStuff:Block2:CoverArt", ClassName, ex);
                }
                try
                {
                    if (string.IsNullOrEmpty(k.URIMetaData))
                    {
                        var pi = await pl.AVTransport.GetPositionInfo();
                        SonosItem streaminfo = SonosItem.ParseSingleItem(pi.TrackMetaData);
                        var x = SonosItem.ParseSingleItem(k.URIMetaData);
                        si.Artist = x.Title;
                        if (CheckRadioTitle(streaminfo.StreamContent))
                        {
                            si.Title = streaminfo.StreamContent.Contains("|")
                                ? streaminfo.StreamContent.Split('|')[0]
                                : streaminfo.StreamContent;
                        }
                    }
                }
                catch (Exception ex)
                {
                    pl.ServerErrorsAdd("SonosItemHelper:GetStreamRadioStuff:Block3", ClassName, ex);
                }
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("SonosItemHelper:GetStreamRadioStuff", ClassName, ex);
            }
            return si;
        }
        /// <summary>
        /// Prüft den Streamcontent auf bekannte Lückenfüller, die nicht angezeigt werden sollen. 
        /// </summary>
        /// <param name="org"></param>
        /// <returns></returns>
        private static Boolean CheckRadioTitle(string org)
        {
            return (!string.IsNullOrEmpty(org) && !org.StartsWith(xsonosapi) && !org.Contains("-live-mp3") && !org.StartsWith("ADBREAK_") && !org.StartsWith("ZPSTR_CONNECTING"));

        }

        /// <summary>
        /// Der Übergebene Container wird, zu einer gültigen URI. 
        /// </summary>
        /// <param name="_cont">Container ID die zu einer URI werden soll.</param>
        /// <param name="playerid">Id des SonosPlayers in der Liste um die UUID zu bestimmen</param>
        /// <returns>URI</returns>
        public static String ContainertoURI(string _cont, string playerid)
        {
            //Kein Filter angesetzt
            string rinconpl = String.Empty;
            if (_cont.StartsWith("S:"))
            {
                rinconpl = _cont.Replace("S:", SonosConstants.xfilecifs); //Playlist
            }
            if (_cont.StartsWith(SonosConstants.xfilecifs))
            {
                rinconpl = _cont; //Song
            }
            if (String.IsNullOrEmpty(rinconpl))
            {
                rinconpl = SonosConstants.xrinconplaylist + playerid + "#" + _cont; //Container
            }
            return rinconpl;
        }
        /// <summary>
        /// Ersetzt den Pfad für die MP3 Verarbeitung
        /// </summary>
        /// <param name="_uri"></param>
        /// <returns></returns>
        public static String URItoPath(string _uri)
        {
            try
            {
                if (string.IsNullOrEmpty(_uri)) return String.Empty;
                _uri = _uri.Replace(SonosConstants.xfilecifs, "");
                _uri = Uri.UnescapeDataString(_uri);
                return _uri.Replace("/", "\\");
            }
            catch
            {
                return _uri;
            }
        }
        /// <summary>
        /// Entfernt vom übergebenen link die Version Parameter wie &v=xxx
        /// Der Parameter muss am ende stehen. 
        /// </summary>
        /// <param name="cover"></param>
        /// <returns></returns>
        public static String RemoveVersionInUri(string cover)
        {
            if (!cover.Contains("&")) return cover;
            var sublen = cover.Length - (cover.Length - cover.LastIndexOf("&"));
            cover = cover.Substring(0, sublen);
            return cover;
        }
        /// <summary>
        /// Für ein Album Cover den Pfad zur Datei erstellen
        /// </summary>
        /// <param name="_uri"></param>
        /// <returns></returns>
        public static String AlbumArtToFile(string _uri)
        {
            try
            {
                if (string.IsNullOrEmpty(_uri)) return String.Empty;
                _uri = HttpUtility.UrlDecode(_uri);
                _uri = Uri.UnescapeDataString(_uri);
                _uri = _uri.Replace("getaa?u="+SonosConstants.xfilecifs, "");
                _uri = RemoveVersionInUri(_uri);
                _uri = _uri.Replace("/", @"\");
                return _uri.Replace("\\\\\\", "\\\\");

            }
            catch
            {
                return _uri;
            }
        }
    }
}