using SonosData.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonosData.DataClasses
{
    public class ZoneGroup
    {
        public string CoordinatorUUID { get; set; } = "";
        public string CoordinatorVersionID { get; set; } = "";
        public SoftwareGeneration SoftwareGeneration { get; set; } = SoftwareGeneration.ZG1;

        public List<ZoneGroupMember> ZoneGroupMember { get; set; } = new();
    }
}
