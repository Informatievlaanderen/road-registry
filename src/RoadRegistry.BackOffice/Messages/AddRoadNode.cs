namespace RoadRegistry.BackOffice.Messages
{
    public class AddRoadNode
    {
        public int TemporaryId { get; set; }
        public string Type { get; set; }
        public RoadNodeGeometry Geometry { get; set; }
    }
}
