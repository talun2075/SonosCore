using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Sonos.Classes;
using SonosUPnP;
using SonosUPnP.DataClasses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sonos.Controllers
{
    [Produces("application/json")]
    [Route("/[controller]")]
    public class IanController : ChildBase
    {
        
        public IanController(IConfiguration iConfig)
        {
            SonosHelper.Configuration = iConfig;
            ChildName = "Ian";
            ReadConfiguration();
        }
        #region Public
        
        [HttpGet("GetStart")]
        public async Task<IList<SonosBrowseList>> GetStart()
        {
            return await Start();
        }
        [HttpGet("GetBaseURL")]
        public async Task<string> GetBaseURL()
        {
            return await BaseURL();
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
        [HttpGet("test")]
        public async void Test()
        {
           await MusicPictures.GenerateDBContent();
           await MusicPictures.GetDBContent();
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
           return await ReplacePlaylist(v, SonosConstants.IanzimmerVolume);
        }

        [HttpPost("SetButton/{id}/{RemoveOld}")]
        public async Task<QueueData> SetButton(string id, Boolean RemoveOld, [FromBody] string containerid)
        {
           return await DefineButton(id, RemoveOld, containerid);
        }
        [HttpGet("PlayRandom/{playlist}")]
        public async Task<Boolean> PlayRandom(string playlist)
        {
            return await Random(playlist, SonosConstants.IanzimmerVolume);
        }
        #endregion Public
    }

}
