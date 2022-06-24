using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Sonos.Classes;

namespace Sonos.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(IConfiguration iConfig)
        {
            SonosHelper.Configuration = iConfig;
        }
        public ActionResult Index()
        {
            return View();
        }
    }
}
