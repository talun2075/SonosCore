using Microsoft.AspNetCore.Mvc;

namespace Sonos.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
