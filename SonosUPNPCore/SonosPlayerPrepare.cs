using HomeLogging;
using Microsoft.Extensions.Configuration;
using SonosData.DataClasses;
using SonosUPNPCore.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace SonosUPNPCore
{
    public class SonosPlayerPrepare : ISonosPlayerPrepare
    {
        /// <summary>
        /// Init over DI
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        public SonosPlayerPrepare(ILogging logger, IConfiguration config)
        {
            Logger = logger;
            Configuration = config;
            GetLocalPlayerIcons();
            if (Boolean.TryParse(Configuration["UseSubscription"], out Boolean outusesubscriptions))
            {
                UseSubscription = outusesubscriptions;
            }
            if (UseSubscription)
            {
                PrepareUsedServices();
            }
        }

        public Boolean UseSubscription { get; set; }
        public Dictionary<string, string> Icons { get; set; } = new();
        public List<SonosEnums.Services> ServiceEnums { get; set; } = new();

        public ILogging Logger { get; set; }
        public IConfiguration Configuration { get; set; }


        /// <summary>
        /// Fill Images from Path root + @"\\wwwroot\\images\\player";
        /// To Use as Device Icons
        /// </summary>
        private void GetLocalPlayerIcons()
        {
            try
            {
                var root = Directory.GetCurrentDirectory();
                //var root = _config.GetValue<string>(WebHostDefaults.ContentRootKey);
                var path = root + @"\\wwwroot\\images\\player";
                var playerimages = Directory.GetFiles(path);
                var url = "/images/player/";
                foreach (var item in playerimages)
                {
                    //cut path
                    string imagename = item.Substring(item.LastIndexOf("\\") + 1);
                    if (!Icons.ContainsKey(imagename))
                    {
                        Icons.Add(imagename, url + imagename);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ServerErrorsAdd("GetLocalPlayerIcons", ex, "SonosHelper");
            }
        }

        /// <summary>
        /// Prepare Servcies from Config if useSubscriptions
        /// </summary>
        private void PrepareUsedServices()
        {
            if (!UseSubscription) return;
            var allowedservices = Configuration["UseOnlyThisSubscriptions"];
            try
            {
                if (allowedservices.Contains(','))
                {
                    var x = allowedservices.Split(',');
                    foreach (var item in x)
                    {
                        if (Enum.TryParse(item.Trim(), out SonosEnums.Services se))
                            ServiceEnums.Add(se);
                    }
                }
                else
                {
                    if (Enum.TryParse(allowedservices.Trim(), out SonosEnums.Services se))
                        ServiceEnums.Add(se);
                }
            }
            catch (Exception ex)
            {
                Logger.ServerErrorsAdd("SonosHelper:InitialSonos:configurations", ex);
            }
        }

    }
}
