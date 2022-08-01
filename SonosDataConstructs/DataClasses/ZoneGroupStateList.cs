using SonosData.Enums;
using System.Xml.Linq;

namespace SonosData.DataClasses
{
    public class ZoneGroupStateList
    {
        public ZoneGroupStateList() { }
        public ZoneGroupStateList(string MetaData)
        {
            ZoneGroupStates = ParseZoneGroupStates(MetaData);
        }

        public List<ZoneGroup> ZoneGroupStates { get; set; } = new();

        public void UpdateZoneGroups(string MetaData)
        {
            var tocheck = ParseZoneGroupStates(MetaData);
            if (tocheck.Any())
            {
                var swgen = tocheck[0].SoftwareGeneration;
                if (ZoneGroupStates.Any())
                    ZoneGroupStates.RemoveAll(x => x.SoftwareGeneration == swgen);
                ZoneGroupStates = ZoneGroupStates.Union(tocheck).ToList();
            }
        }
        private static List<ZoneGroup> ParseZoneGroupStates(string meta)
        {
            try
            {
                var xml = XElement.Parse(meta);
                var zoneGroups = xml.Descendants("ZoneGroups");
                var items = zoneGroups.Elements("ZoneGroup");

                var list = new List<ZoneGroup>();

                foreach (var item in items)
                {
                    try
                    {
                        var member = item.Elements("ZoneGroupMember");
                        ZoneGroup zg = new()
                        {
                            CoordinatorUUID = (string)item.Attribute("Coordinator") ?? "",
                            CoordinatorVersionID = (string)item.Attribute("ID") ?? "",
                            ZoneGroupMember = new List<ZoneGroupMember>()
                        };

                        foreach (var zgmxml in member)
                        {
                            ZoneGroupMember zgm = new()
                            {
                                UUID = (string)zgmxml.Attribute("UUID") ?? "",
                                Location = (string)zgmxml.Attribute("Location") ?? "",
                                ZoneName = (string)zgmxml.Attribute("ZoneName") ?? "",
                                Icon = (string)zgmxml.Attribute("Icon") ?? "",
                                Configuration = Convert.ToInt16((string)zgmxml.Attribute("Configuration")),
                                SoftwareVersion = (string)zgmxml.Attribute("SoftwareVersion") ?? ""
                            };
                            var swgentemp = (string)zgmxml.Attribute("SWGen") ?? "";
                            if (Enum.TryParse(swgentemp, out SoftwareGeneration swgen))
                            {
                                zgm.SoftwareGeneration = swgen;
                            }
                            else
                            {
                                switch (swgentemp)
                                {
                                    case "2":
                                        zgm.SoftwareGeneration = SoftwareGeneration.ZG2;
                                        break;
                                    default:
                                        zgm.SoftwareGeneration = SoftwareGeneration.ZG1;
                                        break;
                                }

                            }

                            zgm.MinCompatibleVersion = (string)zgmxml.Attribute("MinCompatibleVersion") ?? "";
                            zgm.LegacyCompatibleVersion = (string)zgmxml.Attribute("LegacyCompatibleVersion") ?? "";
                            zgm.BootSeq = Convert.ToInt16((string)zgmxml.Attribute("BootSeq"));
                            if ((string)zgmxml.Attribute("TVConfigurationError")!= null && (string)zgmxml.Attribute("TVConfigurationError") =="1")
                            {
                                zgm.TVConfigurationError = true;
                            }
                            if ((string)zgmxml.Attribute("HdmiCecAvailable") != null && (string)zgmxml.Attribute("HdmiCecAvailable") == "1")
                            {
                                zgm.HdmiCecAvailable = true;
                            }
                            if ((string)zgmxml.Attribute("WirelessMode") != null && (string)zgmxml.Attribute("WirelessMode") == "1")
                            {
                                zgm.WirelessMode = true;
                            }
                            if ((string)zgmxml.Attribute("WirelessLeafOnly") != null && (string)zgmxml.Attribute("WirelessLeafOnly") == "1")
                            {
                                zgm.WirelessLeafOnly = true;
                            }
                            if ((string)zgmxml.Attribute("HasConfiguredSSID") != null && (string)zgmxml.Attribute("HasConfiguredSSID") == "1")
                            {
                                zgm.HasConfiguredSSID = true;
                            }
                            if ((string)zgmxml.Attribute("BehindWifiExtender") != null && (string)zgmxml.Attribute("BehindWifiExtender") == "1")
                            {
                                zgm.BehindWifiExtender = true;
                            }
                            if ((string)zgmxml.Attribute("WifiEnabled") != null && (string)zgmxml.Attribute("WifiEnabled") == "1")
                            {
                                zgm.WifiEnabled = true;
                            }
                            if ((string)zgmxml.Attribute("VoiceConfigState") != null && (string)zgmxml.Attribute("VoiceConfigState") == "1")
                            {
                                zgm.VoiceConfigState = true;
                            }
                            if ((string)zgmxml.Attribute("AirPlayEnabled") != null && (string)zgmxml.Attribute("AirPlayEnabled") == "1")
                            {
                                zgm.AirPlayEnabled = true;
                            }
                            if ((string)zgmxml.Attribute("MicEnabled") != null && (string)zgmxml.Attribute("MicEnabled") == "1")
                            {
                                zgm.MicEnabled = true;
                            }
                            if ((string)zgmxml.Attribute("IdleState") != null && (string)zgmxml.Attribute("IdleState") == "1")
                            {
                                zgm.IdleState = true;
                            }
                            if ((string)zgmxml.Attribute("Orientation") != null && (string)zgmxml.Attribute("Orientation") == "1")
                            {
                                zgm.Orientation = true;
                            }
                            zgm.ChannelFreq = Convert.ToInt16((string)zgmxml.Attribute("ChannelFreq"));
                            zgm.RoomCalibrationState = Convert.ToInt16((string)zgmxml.Attribute("RoomCalibrationState"));
                            zgm.SecureRegState = Convert.ToInt16((string)zgmxml.Attribute("SecureRegState"));
                            zgm.MoreInfo = (string)zgmxml.Attribute("MoreInfo") ?? "";
                            zg.ZoneGroupMember.Add(zgm);
                        }
                        try
                        {
                            zg.SoftwareGeneration = zg.ZoneGroupMember[0].SoftwareGeneration;
                        }
                        catch
                        {
                            //ignore
                        }
                        list.Add(zg);
                    }
                    catch
                    {
                        continue;
                    }

                }
                return list;
            }
            catch
            {
                return new List<ZoneGroup>();
            }
        }
    }
}
