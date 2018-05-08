namespace Shaperon.IO
{
    public class BoundingBox3D
    {
        public BoundingBox3D(double xMin, double yMin, double xMax, double yMax, double zMin, double zMax, double mMin, double mMax)
        {
            XMin = xMin;
            YMin = yMin;
            XMax = xMax;
            YMax = yMax;
            ZMin = zMin;
            ZMax = zMax;
            MMin = mMin;
            MMax = mMax;
        }

        public double XMin { get; }
        public double YMin { get; }
        public double XMax { get; }
        public double YMax { get; }
        public double ZMin { get; }
        public double ZMax { get; }
        public double MMin { get; }
        public double MMax { get; }
    }
}
