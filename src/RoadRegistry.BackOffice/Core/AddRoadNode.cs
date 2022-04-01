namespace RoadRegistry.BackOffice.Core
{
    using System;
    using NetTopologySuite.Geometries;
    using Validators;
    using Validators.AddRoadNode.After;

    public class AddRoadNode : IRequestedChange
    {
        public AddRoadNode(RoadNodeId id, RoadNodeId temporaryId, RoadNodeType type, Point geometry)
        {
            Id = id;
            TemporaryId = temporaryId;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
        }

        public RoadNodeId Id { get; }
        public RoadNodeId TemporaryId { get; }
        public RoadNodeType Type { get; }
        public Point Geometry { get; }

        public Problems VerifyBefore(BeforeVerificationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Problems.None;
        }

        public Problems VerifyAfter(AfterVerificationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var instanceToValidate = new AddRoadNodeWithAfterVerificationContext(this, context);
            var validator = new AddRoadNodeWithAfterVerificationContextValidator();
            return validator.Validate(instanceToValidate).ToProblems();
        }

        public void TranslateTo(Messages.AcceptedChange message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            message.RoadNodeAdded = new Messages.RoadNodeAdded
            {
                Id = Id,
                TemporaryId = TemporaryId,
                Type = Type.ToString(),
                Geometry = new Messages.RoadNodeGeometry
                {
                    SpatialReferenceSystemIdentifier = Geometry.SRID,
                    Point = new Messages.Point
                    {
                        X = Geometry.X,
                        Y = Geometry.Y
                    }
                }
            };
        }

        public void TranslateTo(Messages.RejectedChange message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            message.AddRoadNode = new Messages.AddRoadNode
            {
                TemporaryId = TemporaryId,
                Type = Type.ToString(),
                Geometry = GeometryTranslator.Translate(Geometry)
            };
        }
    }
}
