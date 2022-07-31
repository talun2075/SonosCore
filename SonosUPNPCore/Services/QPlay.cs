using OSTL.UPnP;
using SonosData.DataClasses;
using SonosUPnP.Classes;
using System;
using System.Threading.Tasks;

namespace SonosUPnP.Services
{
    public class QPlay
    {
        private const string ClassName = "QPlay";
        private UPnPService qPlayService;
        private readonly SonosPlayer pl;
        public QPlay(SonosPlayer sp)
        {
            pl = sp;
        }

        /// <summary>
        /// Liefert den QPlay Service
        /// </summary>
        public UPnPService QPlayService
        {
            get
            {
                if (qPlayService != null)
                    return qPlayService;
                if (pl.Device == null)
                {
                    pl.LoadDevice();
                    return null;
                }
                qPlayService = pl.Device.GetService("urn:tencent-com:serviceId:QPlay");
                return qPlayService;
            }
        }

        public async Task<QPlayData> QPlayAuth(SonosPlayer pl, string Seed)
        {
            try
            {
                if (QPlayService == null)
                {
                    pl.ServerErrorsAdd("QPlayAuth",ClassName, new Exception(ClassName+" ist null"));
                    return new QPlayData();
                }
                var arguments = new UPnPArgument[4];
                arguments[0] = new UPnPArgument("Seed", Seed);
                arguments[1] = new UPnPArgument("Code", null);
                arguments[2] = new UPnPArgument("MID", null);
                arguments[3] = new UPnPArgument("DID", null);
                QPlayService.InvokeAsync("QPlayAuth", arguments);
                await ServiceWaiter.WaitWhileAsync(arguments, 1, 100, 10, WaiterTypes.String);
                QPlayData qd = new();
                await Task.Delay(100);
                qd.Code = arguments[1].DataValue.ToString();
                qd.MID = arguments[2].DataValue.ToString();
                qd.DID = arguments[3].DataValue.ToString();
                return qd;
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("QPlayAuth", ClassName, ex);
                return new QPlayData();
            }
        }
    }
}
