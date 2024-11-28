using Microsoft.AspNetCore.Mvc;


namespace Sonos.Controllers
{
    [Produces("application/json")]
    [Route("/[controller]")]
    public class FinnController : Controller
    {

          
        #region Public

        public ActionResult Index()
        {
            ViewBag.Child = "Finn";
            return View();
        }
        #endregion Public
    }

}
