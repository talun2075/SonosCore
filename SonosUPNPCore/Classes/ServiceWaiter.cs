using OSTL.UPnP;
using System;
using System.Threading.Tasks;

namespace SonosUPnP.Classes
{
    /// <summary>
    /// Wertet UPNP Argumente aus und gibt der Verwendeten Methode die Möglichkeit die Werte zurückzugeben, wenn diese gefüllt sind
    /// </summary>
    public static class ServiceWaiter
    {
       
        /// <summary>
        /// Überprüft die Argumente anhand des argNumber Indexes. Wenn dieser gefüllt ist macht er ein Return
        /// </summary>
        /// <param name="upnparg">Überwachenden Argumente</param>
        /// <param name="argNumber">Index des zu überwachenden Wertes</param>
        /// <param name="sleep">Wie lange wird gewartet bis wieder geprüft wird in Millisekunden</param>
        /// <param name="countermax">Abbruch Counter falls der Wert nie gefüllt wird</param>
        /// <param name="wt">Typ des zu Überprüfenden Wertes</param>
        /// <returns></returns>
        public static async Task<Boolean> WaitWhileAsync(UPnPArgument[] upnparg, int argNumber, int sleep, int countermax, WaiterTypes wt)
        {
            try
            {
                Boolean okdata = false;
                int counter = 0;

                while (!okdata)
                {
                    switch (wt)
                    {
                        case WaiterTypes.String:
                            if (string.IsNullOrEmpty(upnparg[argNumber].DataValue?.ToString()))
                            {
                                await Task.Delay(sleep);
                                counter++;
                            }
                            else
                            {
                                okdata = true;
                            }
                            break;
                    }
                    if (counter > countermax)//wenn der counter zu groß ist, dann ist etwas schief gegangen.
                        okdata = true;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
    public enum WaiterTypes
    {
        String
    }
}
