/*   
Copyright 2006 - 2010 Intel Corporation

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Diagnostics;
using System.Text;
using HomeLogging;

namespace OpenSource.Utilities
{

    /// <summary>
    /// This class can be used as the master event logging class for all
    /// of an application and its libraries. The application should set the
    /// system log file upon startup. All exceptions should be sent to the log
    /// using the Log(Exception) method.
    /// </summary>
    public sealed class EventLogger
    {

        public static bool EnabledTrace = false;
        public static bool Enabled = true;
        public static bool ShowAll = false;

        public static Logging Logger { get; set; } = new(new LoggerWrapperConfig() { ConfigName = "SonosUPNP", TraceFileName = "traceUPNP.txt", ErrorFileName = "ErrorUPNP.txt" });

        public static void Log(object sender, EventLogEntryType LogType, string information)
        {
            if (sender == null)
            {
                sender = new object();
            }
            if (EnabledTrace)
            {
                if (ShowAll || LogType == EventLogEntryType.Error || LogType == EventLogEntryType.SuccessAudit)
                {
                    string origin = sender.GetType().FullName;
                    StringBuilder trace = new();

                    if (LogType == EventLogEntryType.Error)
                    {
                        StackTrace t = new();
                        for (int i = 0; i < t.FrameCount; ++i)
                        {
                            var declaringType = t.GetFrame(i).GetMethod().DeclaringType;
                            if (declaringType != null)
                                trace.Append(declaringType.FullName + "." + t.GetFrame(i).GetMethod().Name + "\r\n");
                        }
                    }
                    if (Logger != null)
                    {
                        try
                        {
                            Logger.InfoLog(origin, information + "\r\n\r\nTRACE:\r\n" + trace);
                        }
                        catch
                        {
                            //continue
                        }
                    }
                }
            }
        }

        public static void Log(Exception exception)
        {
            Log(exception, "");
        }

        /// <summary>
        /// Log an exception into the system log.
        /// </summary>
        /// <param name="exception">Exception to be logged</param>
        /// <param name="additional"></param>
        public static void Log(Exception exception, string additional)
        {
            try
            {
                if (Enabled)
                {
                    string name = exception.GetType().FullName;
                    string message = exception.Message;
                    Exception t = exception;
                    int i = 0;
                    while (t.InnerException != null)
                    {
                        t = t.InnerException;
                        name += " : " + t.GetType().FullName;
                        // message = t.Message;
                        // NKIDD - ADDED
                        message += "\r\n\r\nInnerException #" + i + ":\r\nMessage: " + t.Message + "\r\nSource: " + t.Source +
                                   "\r\nStackTrace: " + t.StackTrace;
                        i++;
                    }

                    name += "\r\n\r\n Additional Info: " + additional + "\r\n" + message;


                    if (Logger != null)
                    {
                        try
                        {
                            if (!t.Message.Contains("The hostname could not be parsed"))
                                Logger.ServerErrorsAdd(exception.Source, exception, "SonosUpnp");
                        }
                        catch (Exception ex)
                        {
                            var k = ex.Message;
                        }
                    }
                }
            }
            catch
            {
                //continue
            }
        }

    }
}
