using SonosData.Enums;
using System.Xml.Linq;

namespace SonosData.DataClasses
{
    public class ZoneGroupStateList
    {
        //todo: ansehen wegen hinweisen.
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
                        //todo: Das NULL-Literal oder ein möglicher NULL-Wert wird in einen Non-Nullable-Typ konvertiert.
                        var member = item.Elements("ZoneGroupMember");
                        ZoneGroup zg = new();
                        zg.CoordinatorUUID = item.Attribute("Coordinator").Value;
                        zg.CoordinatorVersionID = item.Attribute("ID").Value;
                        zg.ZoneGroupMember = new List<ZoneGroupMember>();

                        foreach (var zgmxml in member)
                        {
                            ZoneGroupMember zgm = new();
                            zgm.UUID = zgmxml.Attribute("UUID").Value;
                            zgm.Location = zgmxml.Attribute("Location").Value;
                            zgm.ZoneName = zgmxml.Attribute("ZoneName").Value;
                            zgm.Icon = zgmxml.Attribute("Icon").Value;
                            zgm.Configuration = Convert.ToInt16(zgmxml.Attribute("Configuration").Value);
                            zgm.SoftwareVersion = zgmxml.Attribute("SoftwareVersion").Value;
                            var swgentemp = zgmxml.Attribute("SWGen").Value;
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
                            zgm.MinCompatibleVersion = zgmxml.Attribute("MinCompatibleVersion").Value;
                            zgm.LegacyCompatibleVersion = zgmxml.Attribute("LegacyCompatibleVersion").Value;
                            zgm.BootSeq = Convert.ToInt16(zgmxml.Attribute("BootSeq").Value);
                            zgm.TVConfigurationError = Convert.ToBoolean(Convert.ToInt16(zgmxml.Attribute("TVConfigurationError").Value));
                            zgm.HdmiCecAvailable = Convert.ToBoolean(Convert.ToInt16(zgmxml.Attribute("HdmiCecAvailable").Value));
                            zgm.WirelessMode = Convert.ToBoolean(Convert.ToInt16(zgmxml.Attribute("WirelessMode").Value));
                            zgm.WirelessLeafOnly = Convert.ToBoolean(Convert.ToInt16(zgmxml.Attribute("WirelessLeafOnly").Value));
                            zgm.HasConfiguredSSID = zgmxml.Attribute("HasConfiguredSSID") != null && Convert.ToBoolean(Convert.ToInt16(zgmxml.Attribute("HasConfiguredSSID").Value));
                            zgm.ChannelFreq = Convert.ToInt16(zgmxml.Attribute("ChannelFreq").Value);
                            zgm.BehindWifiExtender = Convert.ToBoolean(Convert.ToInt16(zgmxml.Attribute("BehindWifiExtender").Value));
                            zgm.WifiEnabled = Convert.ToBoolean(Convert.ToInt16(zgmxml.Attribute("WifiEnabled").Value));
                            zgm.Orientation = Convert.ToBoolean(Convert.ToInt16(zgmxml.Attribute("Orientation").Value));
                            zgm.RoomCalibrationState = Convert.ToInt16(zgmxml.Attribute("RoomCalibrationState").Value);
                            zgm.SecureRegState = Convert.ToInt16(zgmxml.Attribute("SecureRegState").Value);
                            zgm.VoiceConfigState = Convert.ToBoolean(Convert.ToInt16(zgmxml.Attribute("VoiceConfigState").Value));
                            zgm.MicEnabled = Convert.ToBoolean(Convert.ToInt16(zgmxml.Attribute("MicEnabled").Value));
                            zgm.AirPlayEnabled = Convert.ToBoolean(Convert.ToInt16(zgmxml.Attribute("AirPlayEnabled").Value));
                            zgm.IdleState = Convert.ToBoolean(Convert.ToInt16(zgmxml.Attribute("IdleState").Value));
                            zgm.MoreInfo = zgmxml.Attribute("MoreInfo").Value;
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
