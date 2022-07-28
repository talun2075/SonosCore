using Microsoft.AspNetCore.Mvc;
using Sonos.Classes;
using SonosUPnP;
using SonosUPnP.DataClasses;
using SonosConst;
using System;
using System.Linq;
using System.Threading.Tasks;
using Sonos.Classes.Interfaces;
using HomeLogging;

namespace Sonos.Controllers
{
    [Route("/[controller]")]
    public class StreamDeckController : Controller
    {
        private readonly ILogging _logger;
        private readonly IStreamDeckResponse _streamDeckResponse;
        private readonly ISonosDiscovery _sonos;
        public StreamDeckController(IStreamDeckResponse sdr, ILogging log, ISonosDiscovery sonos)
        {
            _logger = log;
            _streamDeckResponse = sdr;
            _sonos = sonos;
        }

        [HttpGet("Cover/{id}")]
        public IStreamDeckResponse StreamDeck(string id)
        {
            SonosPlayer pl = _sonos.GetPlayerbyUuid(id);

            try
            {
                var rand = new Random().Next(SonosConstants.MusicPictureHashes.Rows.Count-1);
                _streamDeckResponse.RandomCover = "http://"+ Request.Host.Value + SonosConstants.CoverHashPathForBrowser + SonosConstants.MusicPictureHashes.Rows[rand].ItemArray[1] + ".png";
            }
            catch (Exception ex)
            {
                AddServerErrors("StreamDeck1", ex);
            }
            try
            {
                if (pl == null) return null;
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
                return null;
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
