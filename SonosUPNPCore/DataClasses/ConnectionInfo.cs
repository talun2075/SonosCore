namespace SonosUPnP.DataClasses
{
    public class ConnectionInfo
    {
        public int ConnectionID { get; set; }
        public int RcsID { get; set; }
        public int AVTransportID { get; set; }
        public string ProtocolInfo { get; set; }
        public string PeerConnectionManager { get; set; }
        public int PeerConnectionID { get; set; }
        public string Direction { get; set; }
        public string Status { get; set; }

    }
}
