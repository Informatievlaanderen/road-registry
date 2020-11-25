namespace RoadRegistry.BackOffice.Messages
{
    public class Ring
    {
        public Point[] Points { get; set; }
    }

    public class Polygon
    {
        public Ring Ring { get; set; }
        public Ring[] Holes { get; set; }
    }
}
