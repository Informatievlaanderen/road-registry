namespace RoadRegistry.BackOffice.Messages
{
    public class RoadNodeAdded
    {
        public int Id { get; set;  }
        public int TemporaryId { get; set;  }
        public string Type { get; set; }
        public RoadNodeGeometry Geometry { get; set; }
    }
}
