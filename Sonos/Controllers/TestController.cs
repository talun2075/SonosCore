using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SonosSQLiteWrapper.Interfaces;

namespace Sonos.Controllers
{
    [Route("/[controller]")]
    public class TestController : Controller
    {
        private readonly IConfiguration configuration;
        public TestController(IConfiguration config)
        {
            configuration = config;
        }

        public ActionResult Index()
        {
            var ver = configuration["Version"];
            if (!string.IsNullOrEmpty(ver))
            {
                ViewBag.Version = ver;
            }
            else
            {
                //todo: JS fiel last modifieds
            }
            return View();
        }

        [HttpGet("test")]
        public string Test()
        {
            throw new System.Exception("dies ist ein blub");
            //return "huuuuu";
        }
    }
}
