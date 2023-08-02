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

        public ResetController(IHostApplicationLifetime appLifetime)
        {
            applicationLifetime = appLifetime;
        }
        [HttpGet("")]
        public bool Reset()
        {
            applicationLifetime.StopApplication();
            return true;
        }
    }
}
