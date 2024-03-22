using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SonosConst;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeLogging;
using Sonos.Classes.Interfaces;
using SonosData.DataClasses;
using SonosSQLiteWrapper.Interfaces;
using SonosUPNPCore.Interfaces;
using Sonos.Classes;
using SonosUPnP;

namespace Sonos.Controllers
{
    [Produces("application/json")]
    [Route("/[controller]")]
    public class FinnController : ChildBase
    {
        
        public FinnController(ILogging log, ISonosHelper sh, ISonosDiscovery sonos, IMusicPictures imu) :base(log,sh,sonos,imu)
        {
            ChildName = "Finn";
            ReadConfiguration();
        }
        #region Public
        
        [HttpGet("GetStart")]
        public async Task<IList<ISonosBrowseList>> GetStart()
        {
            return await Start();
        }
        [HttpGet("GetBaseURL")]
        public string GetBaseURL()
        {
            return BaseURL();
        }
        [HttpGet("GetTransport")]
        public async Task<int> GetTransport()
        {
            return await Transport();
        }
        [HttpGet("SetPause")]
        public async Task<Boolean> SetPause()
        {
            return await Pause();
        }

        [HttpGet("SetVolume/{id}")]
        public async Task<Boolean> SetVolume(string id)
        {
            return await Volume(id);
        }
        /// <summary>
        /// Aktuelle Wiedergabeliste ersetzen
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        [HttpPost("ReplacePlaylist")]
        public async Task<Boolean> ReplacePlaylist([FromBody] string v)
        {
           return await ReplacePlaylist(v, SonosConstants.FinnzimmerVolume);
        }
        /// <summary>
        /// Aktuelle Wiedergabeliste ersetzen
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        [HttpGet("ReplacePlaylistGet/{id}")]
        public async Task<Boolean> ReplacePlaylistGet(string id)
        {
            return await ReplacePlaylistGet(id, SonosConstants.FinnzimmerVolume);
        }
        [HttpPost("SetButton/{id}/{RemoveOld}")]
        public async Task<QueueData> SetButton(string id, Boolean RemoveOld, [FromBody] string containerid)
        {
           return await DefineButton(id, RemoveOld, containerid);
        }
        [HttpGet("PlayRandom/{playlist}")]
        public async Task<Boolean> PlayRandom(string playlist)
        {
            return await Random(playlist, SonosConstants.FinnzimmerVolume);
        }
        #endregion Public
    }

}
