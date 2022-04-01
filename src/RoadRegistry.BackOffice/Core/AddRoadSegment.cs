namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NetTopologySuite.Geometries;
    using Validators;
    using Validators.AddRoadSegment.After;
    using Validators.AddRoadSegment.Before;

    public class AddRoadSegment : IRequestedChange
    {
        public AddRoadSegment(
            RoadSegmentId id,
            RoadSegmentId temporaryId,
            RoadNodeId startNodeId,
            RoadNodeId? temporaryStartNodeId,
            RoadNodeId endNodeId,
            RoadNodeId? temporaryEndNodeId,
            MultiLineString geometry,
            OrganizationId maintenanceAuthorityId,
            OrganizationName? maintenanceAuthorityName,
            RoadSegmentGeometryDrawMethod geometryDrawMethod,
            RoadSegmentMorphology morphology,
            RoadSegmentStatus status,
            RoadSegmentCategory category,
            RoadSegmentAccessRestriction accessRestriction,
            CrabStreetnameId? leftSideStreetNameId,
            CrabStreetnameId? rightSideStreetNameId,
            IReadOnlyList<RoadSegmentLaneAttribute> lanes,
            IReadOnlyList<RoadSegmentWidthAttribute> widths,
            IReadOnlyList<RoadSegmentSurfaceAttribute> surfaces)
        {
            Id = id;
            TemporaryId = temporaryId;
            StartNodeId = startNodeId;
            TemporaryStartNodeId = temporaryStartNodeId;
            EndNodeId = endNodeId;
            TemporaryEndNodeId = temporaryEndNodeId;
            Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
            MaintenanceAuthorityId = maintenanceAuthorityId;
            MaintenanceAuthorityName = maintenanceAuthorityName;
            GeometryDrawMethod = geometryDrawMethod ?? throw new ArgumentNullException(nameof(geometryDrawMethod));
            Morphology = morphology ?? throw new ArgumentNullException(nameof(morphology));
            Status = status ?? throw new ArgumentNullException(nameof(status));
            Category = category ?? throw new ArgumentNullException(nameof(category));
            AccessRestriction = accessRestriction ?? throw new ArgumentNullException(nameof(accessRestriction));
            LeftSideStreetNameId = leftSideStreetNameId;
            RightSideStreetNameId = rightSideStreetNameId;
            Lanes = lanes ?? throw new ArgumentNullException(nameof(lanes));
            Widths = widths ?? throw new ArgumentNullException(nameof(widths));
            Surfaces = surfaces ?? throw new ArgumentNullException(nameof(surfaces));
        }

        public RoadSegmentId Id { get; }
        public RoadSegmentId TemporaryId { get; }
        public RoadNodeId StartNodeId { get; }
        public RoadNodeId? TemporaryStartNodeId { get; }
        public RoadNodeId EndNodeId { get; }
        public RoadNodeId? TemporaryEndNodeId { get; }
        public MultiLineString Geometry { get; }
        public OrganizationId MaintenanceAuthorityId { get; }
        public OrganizationName? MaintenanceAuthorityName { get; }
        public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }
        public RoadSegmentMorphology Morphology { get; }
        public RoadSegmentStatus Status { get; }
        public RoadSegmentCategory Category { get; }
        public RoadSegmentAccessRestriction AccessRestriction { get; }
        public CrabStreetnameId? LeftSideStreetNameId { get; }
        public CrabStreetnameId? RightSideStreetNameId { get; }
        public IReadOnlyList<RoadSegmentLaneAttribute> Lanes { get; }
        public IReadOnlyList<RoadSegmentWidthAttribute> Widths { get; }
        public IReadOnlyList<RoadSegmentSurfaceAttribute> Surfaces { get; }

        public Problems VerifyBefore(BeforeVerificationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var instanceToValidate = new AddRoadSegmentWithBeforeVerificationContext(this, context);
            var validator = new AddRoadSegmentWithBeforeVerificationContextValidator();
            return validator.Validate(instanceToValidate).ToProblems();
        }

        public Problems VerifyAfter(AfterVerificationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var instanceToValidate = new AddRoadSegmentWithAfterVerificationContext(this, context);
            var validator = new AddRoadSegmentWithAfterVerificationContextValidator();
            return validator.Validate(instanceToValidate).ToProblems();
        }

        public void TranslateTo(Messages.AcceptedChange message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            message.RoadSegmentAdded = new Messages.RoadSegmentAdded
            {
                Id = Id,
                TemporaryId = TemporaryId,
                StartNodeId = StartNodeId,
                EndNodeId = EndNodeId,
                Geometry = GeometryTranslator.Translate(Geometry),
                MaintenanceAuthority = new Messages.MaintenanceAuthority
                {
                    Code = MaintenanceAuthorityId,
                    Name = MaintenanceAuthorityName ?? ""
                },
                GeometryDrawMethod = GeometryDrawMethod,
                Morphology = Morphology,
                Status = Status,
                Category = Category,
                AccessRestriction = AccessRestriction,
                LeftSide = new Messages.RoadSegmentSideAttributes
                {
                    StreetNameId = LeftSideStreetNameId.GetValueOrDefault()
                },
                RightSide = new Messages.RoadSegmentSideAttributes
                {
                    StreetNameId = RightSideStreetNameId.GetValueOrDefault()
                },
                Lanes = Lanes
                    .Select(item => new Messages.RoadSegmentLaneAttributes
                    {
                        AttributeId = item.Id,
                        AsOfGeometryVersion = 1,
                        Count = item.Count,
                        Direction = item.Direction,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray(),
                Widths = Widths
                    .Select(item => new Messages.RoadSegmentWidthAttributes
                    {
                        AttributeId = item.Id,
                        AsOfGeometryVersion = 1,
                        Width = item.Width,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray(),
                Surfaces = Surfaces
                    .Select(item => new Messages.RoadSegmentSurfaceAttributes
                    {
                        AttributeId = item.Id,
                        AsOfGeometryVersion = 1,
                        Type = item.Type,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray()
            };
        }

        public void TranslateTo(Messages.RejectedChange message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            message.AddRoadSegment = new Messages.AddRoadSegment
            {
                TemporaryId = TemporaryId,
                StartNodeId = TemporaryStartNodeId ?? StartNodeId,
                EndNodeId = TemporaryEndNodeId ?? EndNodeId,
                Geometry = GeometryTranslator.Translate(Geometry),
                MaintenanceAuthority = MaintenanceAuthorityId,
                GeometryDrawMethod = GeometryDrawMethod,
                Morphology = Morphology,
                Status = Status,
                Category = Category,
                AccessRestriction = AccessRestriction,
                LeftSideStreetNameId = LeftSideStreetNameId.GetValueOrDefault(),
                RightSideStreetNameId = RightSideStreetNameId.GetValueOrDefault(),
                Lanes = Lanes
                    .Select(item => new Messages.RequestedRoadSegmentLaneAttribute
                    {
                        AttributeId = item.TemporaryId,
                        Count = item.Count,
                        Direction = item.Direction,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray(),
                Widths = Widths
                    .Select(item => new Messages.RequestedRoadSegmentWidthAttribute
                    {
                        AttributeId = item.TemporaryId,
                        Width = item.Width,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray(),
                Surfaces = Surfaces
                    .Select(item => new Messages.RequestedRoadSegmentSurfaceAttribute
                    {
                        AttributeId = item.TemporaryId,
                        Type = item.Type,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray()
            };
        }
    }
}
