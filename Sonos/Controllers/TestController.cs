using Microsoft.AspNetCore.Mvc;
using Sonos.Classes.Interfaces;
using SonosSQLiteWrapper;
using SonosSQLiteWrapper.Interfaces;

namespace Sonos.Controllers
{
    [Route("/[controller]")]
    public class TestController : Controller
    {
        private IMusicPictures musicPictures;
        public TestController(IMusicPictures imu)
        {
            musicPictures = imu;
        }
        [HttpGet("test")]
        public string test()
        {

            return "huuuuu";
        }
    }
}
