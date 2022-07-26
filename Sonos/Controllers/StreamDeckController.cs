using Microsoft.AspNetCore.Mvc;
using Sonos.Classes;
using SonosUPnP;
using SonosUPnP.DataClasses;
using SonosConst;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sonos.Controllers
{
    [Route("/[controller]")]
    public class StreamDeckController : Controller
    {
        [HttpGet("Cover/{id}")]
        public async Task<StreamDeckResponse> StreamDeck(string id)
        {
            SonosPlayer pl = await SonosHelper.GetPlayerbyUuid(id);
            StreamDeckResponse streamDeckResponse = new();

            try
            {
                var rand = new Random().Next(SonosConstants.MusicPictureHashes.Count);
                streamDeckResponse.RandomCover = "http://"+ Request.Host.Value + SonosConstants.CoverHashPathForBrowser + SonosConstants.MusicPictureHashes.ElementAt(rand).Value+ ".png";
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
                streamDeckResponse.CoverString = cover;
                streamDeckResponse.Playing = pl.PlayerProperties.TransportState == SonosEnums.TransportState.PLAYING;
                return streamDeckResponse;
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
        private static void AddServerErrors(string Func, Exception ex)
        {
            SonosHelper.Logger.ServerErrorsAdd(Func, ex, "PlayerController");
        }
    }
}
