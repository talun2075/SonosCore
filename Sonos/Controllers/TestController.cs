using Microsoft.AspNetCore.Mvc;
using SonosSQLiteWrapper.Interfaces;

namespace Sonos.Controllers
{
    [Route("/[controller]")]
    public class TestController : Controller
    {
        public TestController()
        {
        }
        [HttpGet("test")]
        public string Test()
        {

            return "huuuuu";
        }
    }
}
