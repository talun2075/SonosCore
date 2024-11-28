using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Sonos.Controllers
{
    public class HomeController(IWebHostEnvironment env) : Controller
    {
        private static DateTimeOffset lastmod = DateTimeOffset.MinValue;

        public ActionResult Index()
        {
            ViewBag.Version = lastmod.UtcTicks;
            return View();
        }
    }
}
