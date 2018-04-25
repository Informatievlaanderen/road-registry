namespace Shaperon
{
    public class BoundingBox2D
    {
        public BoundingBox2D(double xMin, double yMin, double xMax, double yMax)
        {
            XMin = xMin;
            YMin = yMin;
            XMax = xMax;
            YMax = yMax;
        }

        public double XMin { get; }
        public double YMin { get; }
        public double XMax { get; }
        public double YMax { get; }
    }
}
