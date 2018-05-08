namespace Shaperon
{
    using Wkx;

    public static class PointExtensions
    {
        public static Point WithMeasure(this Point point, double? value)
        {
            return new Point(point.X.Value, point.Y.Value, point.Z, value);
        }
    }
}