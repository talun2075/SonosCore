using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using SonosUPNPCore.Interfaces;

namespace Sonos.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class ResetController : Controller
    {
        IHostApplicationLifetime applicationLifetime;
        ISonosDiscovery sonos;

        public ResetController(IHostApplicationLifetime appLifetime, ISonosDiscovery _sonos)
        {
            applicationLifetime = appLifetime;
            sonos = _sonos;
        }
        [HttpGet("")]
        public bool Reset()
        {
            applicationLifetime.StopApplication();

            return true;
        }
    }
}
