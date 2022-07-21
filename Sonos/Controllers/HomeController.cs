using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SonosUPnP;

namespace Sonos.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(IConfiguration iConfig)
        {
            SonosConstants.Configuration = iConfig;
        }
        public ActionResult Index()
        {
            return View();
        }
    }
}
