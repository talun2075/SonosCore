using Microsoft.AspNetCore.Mvc;
using Sonos.Classes;
using SonosUPnP;
using SonosConst;
using System;
using System.Linq;
using System.Threading.Tasks;
using Sonos.Classes.Interfaces;
using HomeLogging;
using SonosData.DataClasses;
using SonosSQLiteWrapper.Interfaces;
using SonosUPNPCore.Interfaces;

namespace Sonos.Controllers
{
    [Route("/[controller]")]
    public class StreamDeckController : Controller
    {
        private readonly ILogging _logger;
        private readonly IStreamDeckResponse _streamDeckResponse;
        private readonly ISonosDiscovery _sonos;
        private readonly IMusicPictures _musicpictures;
        public StreamDeckController(IStreamDeckResponse sdr, ILogging log, ISonosDiscovery sonos, IMusicPictures musicpictures)
        {
            _logger = log;
            _streamDeckResponse = sdr;
            _sonos = sonos;
            _musicpictures = musicpictures;
        }

        [HttpGet("Cover/{id}")]
        public IStreamDeckResponse StreamDeck(string id)
        {
            SonosPlayer pl = _sonos.GetPlayerbyUuid(id);

            try
            {
                if(_musicpictures.CurrentMusicPictures.Rows.Count > 0)
                {
                    var rand = new Random().Next(_musicpictures.CurrentMusicPictures.Rows.Count - 1);
                    _streamDeckResponse.RandomCover = "http://" + Request.Host.Value + SonosConstants.CoverHashPathForBrowser + _musicpictures.CurrentMusicPictures.Rows[rand].ItemArray[1] + ".png";
                }
                else
                {
                    _streamDeckResponse.RandomCover = "";
                }
                
            }
            catch (Exception ex)
            {
                AddServerErrors("StreamDeck1", ex);
            }
            try
            {
                if (pl == null) return _streamDeckResponse;
                string cover;
                if (pl.PlayerProperties.CurrentTrack.AlbumArtURI.StartsWith(SonosConstants.CoverHashPathForBrowser))
                {
                    cover = "http://" + Request.Host.Value + pl.PlayerProperties.CurrentTrack.AlbumArtURI;
                }
                else
                {
                    cover = "http://" + pl.PlayerProperties.BaseUrl + pl.PlayerProperties.CurrentTrack.AlbumArtURI;
                }
                _streamDeckResponse.CoverString = cover;
                _streamDeckResponse.Playing = pl.PlayerProperties.TransportState == SonosEnums.TransportState.PLAYING;
                return _streamDeckResponse;
            }
            catch (Exception ex)
            {
                AddServerErrors("StreamDeck2", ex);
                return _streamDeckResponse;
            }
        }
        /// <summary>
        /// Fügt Exception zum Sonoshelper
        /// </summary>
        /// <param name="Func"></param>
        /// <param name="ex"></param>
        private void AddServerErrors(string Func, Exception ex)
        {
            _logger.ServerErrorsAdd(Func, ex, "StreamDeckController");
        }
    }
}
