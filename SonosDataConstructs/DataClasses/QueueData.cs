namespace SonosData.DataClasses
{
    public class QueueData
    {
        public int FirstTrackNumberEnqueued { get; set; }
        public int NumTracksAdded { get; set; } = 0;
        public int NewQueueLength { get; set; } = 0;
        public ushort NewUpdateID { get; set; }
        public int QueueID { get; set; }
        public int AddAtIndex { get; set; }
        public string QueueOwnerContext { get; set; } = "";
        public string AssignedObjectID { get; set; } = "";
        public int QueueLengthChange { get; set; }

        public bool IsEmpty
        {
            get
            {
                return NumTracksAdded == 0 && NewQueueLength == 0;
            }
        }
    }
}
