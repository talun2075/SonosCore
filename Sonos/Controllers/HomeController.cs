using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

namespace Sonos.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration configuration;
        public HomeController(IConfiguration config)
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
    }
}
