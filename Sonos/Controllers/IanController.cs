using Microsoft.AspNetCore.Mvc;

namespace Sonos.Controllers
{
    [Produces("application/json")]
    [Route("/[controller]")]
    public class IanController : Controller
    {
        #region Public
        public ActionResult Index()
        {
            ViewBag.Child = "Ian";
            return View();
        }
        
        #endregion Public
    }

}
