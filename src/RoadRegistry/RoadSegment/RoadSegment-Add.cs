namespace RoadRegistry.RoadSegment;

using System.Collections.Immutable;
using BackOffice.Core;
using Changes;
using Events;
using RoadNetwork;
using ValueObjects;

public partial class RoadSegment
{
    public static (RoadSegment?, Problems) Add(AddRoadSegmentChange change, IRoadNetworkIdGenerator idGenerator, IIdentifierTranslator idTranslator)
    {
        var problems = Problems.None;

        var originalIdOrId = change.OriginalId ?? change.TemporaryId;

        problems += new RoadSegmentGeometryValidator().Validate(originalIdOrId, change.GeometryDrawMethod, change.Geometry);

        problems += new RoadSegmentAttributesValidator().Validate(originalIdOrId,
            new RoadSegmentAttributes
            {
                GeometryDrawMethod = change.GeometryDrawMethod,
                AccessRestriction = change.AccessRestriction,
                Category = change.Category,
                Morphology = change.Morphology,
                Status = change.Status,
                StreetNameId = change.StreetNameId,
                MaintenanceAuthorityId = change.MaintenanceAuthorityId,
                SurfaceType = change.SurfaceType,
                EuropeanRoadNumbers = change.EuropeanRoadNumbers.ToImmutableList(),
                NationalRoadNumbers = change.NationalRoadNumbers.ToImmutableList()
            },
            change.Geometry.Length);

        if (problems.HasError())
        {
            return (null, problems);
        }

        var segment = Create(new RoadSegmentAdded
        {
            RoadSegmentId = change.PermanentId ?? idGenerator.NewRoadSegmentId(),
            OriginalId = change.OriginalId ?? change.TemporaryId,
            Geometry = change.Geometry.ToGeometryObject(),
            StartNodeId = idTranslator.TranslateToPermanentId(change.StartNodeId),
            EndNodeId = idTranslator.TranslateToPermanentId(change.EndNodeId),
            GeometryDrawMethod = change.GeometryDrawMethod,
            AccessRestriction = change.AccessRestriction,
            Category = change.Category,
            Morphology = change.Morphology,
            Status = change.Status,
            StreetNameId = change.StreetNameId,
            MaintenanceAuthorityId = change.MaintenanceAuthorityId,
            SurfaceType = change.SurfaceType,
            EuropeanRoadNumbers = change.EuropeanRoadNumbers,
            NationalRoadNumbers = change.NationalRoadNumbers,
        });

        return (segment, problems);
    }
}
