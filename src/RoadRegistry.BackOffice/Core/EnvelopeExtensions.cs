namespace RoadRegistry.BackOffice.Core
{
    using System;
    using NetTopologySuite.Geometries;

    internal static class EnvelopeExtensions
    {
        public static Envelope ExpandWith(this Envelope envelope, Envelope other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new Envelope(
                Math.Min(envelope.MinX, other.MinX),
                Math.Max(envelope.MaxX, other.MaxX),
                Math.Min(envelope.MinY, other.MinY),
                Math.Max(envelope.MaxY, other.MaxY));
        }
    }
}
