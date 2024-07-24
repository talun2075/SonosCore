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
            //Datum ermitteln, wann eine JS das letzte mal angepasst wurde. Das machen wir einmal pro system start.
            if (lastmod == DateTimeOffset.MinValue)
            {
                var physicalProvider = env.ContentRootFileProvider;
                var path = Path.Combine("wwwroot", "js");
                var content = physicalProvider.GetDirectoryContents(path);

                foreach (var item in content)
                {
                    if (lastmod == DateTimeOffset.MinValue)
                    {
                        lastmod = item.LastModified;
                    }
                    if (lastmod < item.LastModified)
                    {
                        lastmod = item.LastModified;
                    }
                }
                //nun für css dateien.
                path = Path.Combine("wwwroot", "css");
                content = physicalProvider.GetDirectoryContents(path);

                foreach (var item in content)
                {
                    if (lastmod == DateTimeOffset.MinValue)
                    {
                        lastmod = item.LastModified;
                    }
                    if (lastmod < item.LastModified)
                    {
                        lastmod = item.LastModified;
                    }
                }

            }
            ViewBag.Version = lastmod.UtcTicks;
            return View();
        }
    }
}
