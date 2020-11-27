namespace RoadRegistry.BackOffice.Messages
{
    public class Polygon
    {
        public Ring Shell { get; set; }
        public Ring[] Holes { get; set; }
    }
}
