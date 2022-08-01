using HomeLogging;
using Microsoft.Extensions.Configuration;
using SonosData.DataClasses;
using System.Collections.Generic;

namespace SonosUPNPCore.Interfaces
{
    public interface ISonosPlayerPrepare
    {
        Dictionary<string, string> Icons { get; set; }
        ILogging Logger { get; set; }
        List<SonosEnums.Services> ServiceEnums { get; set; }
        bool UseSubscription { get; set; }
        IConfiguration Configuration { get; set; }
    }
}