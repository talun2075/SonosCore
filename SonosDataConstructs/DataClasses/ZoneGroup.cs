using SonosData.Enums;

namespace SonosData.DataClasses
{
    public class ZoneGroup
    {
        public string CoordinatorUUID { get; set; } = "";
        public string CoordinatorVersionID { get; set; } = "";
        public List<ZoneGroupMember> ZoneGroupMember { get; set; } = new();
    }
}
