// namespace RoadRegistry.Integration.Schema.RoadNodes;
//
// using BackOffice;
// using Be.Vlaanderen.Basisregisters.Shaperon;
//
// public class RoadNodeBoundingBox
// {
//     public double MaximumX { get; set; }
//     public double MaximumY { get; set; }
//     public double MinimumX { get; set; }
//     public double MinimumY { get; set; }
//
//     public static RoadNodeBoundingBox From(Point shape)
//     {
//         var point = GeometryTranslator.ToPoint(shape);
//
//         return new RoadNodeBoundingBox
//         {
//             MinimumX = point.EnvelopeInternal.MinX,
//             MinimumY = point.EnvelopeInternal.MinY,
//             MaximumX = point.EnvelopeInternal.MaxX,
//             MaximumY = point.EnvelopeInternal.MaxY
//         };
//     }
//
//     public BoundingBox3D ToBoundingBox3D(double minimumM = default, double maximumM = default)
//     {
//         return new BoundingBox3D(
//             MinimumX,
//             MinimumY,
//             MaximumX,
//             MaximumY,
//             0.0,
//             0.0,
//             minimumM,
//             maximumM
//         );
//     }
// }
