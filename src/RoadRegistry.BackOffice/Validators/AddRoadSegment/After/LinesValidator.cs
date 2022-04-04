namespace RoadRegistry.BackOffice.Validators.AddRoadSegment.After
{
    using System.Linq;
    using Core;
    using FluentValidation;
    using FluentValidation.Validators;
    using NetTopologySuite.Geometries;

    internal class LinesValidator : PropertyValidator<AddRoadSegmentWithAfterVerificationContext, LineString>
    {
        public override bool IsValid(ValidationContext<AddRoadSegmentWithAfterVerificationContext> context, LineString value)
        {
            if (!context.InstanceToValidate.AfterVerificationContext.AfterView.View.Nodes.TryGetValue(context.InstanceToValidate.AddRoadSegment.StartNodeId, out var startNode))
            {
                throw new RoadSegmentStartNodeMissing().ToValidationException();
            }

            var verifyTypeMatchesConnectedSegments = startNode.VerifyTypeMatchesConnectedSegmentCount(context.InstanceToValidate.AfterVerificationContext.AfterView.View,
                context.InstanceToValidate.AfterVerificationContext.Translator);
            if (verifyTypeMatchesConnectedSegments.Any())
            {
                throw verifyTypeMatchesConnectedSegments.ToValidationException();
            }

            if (value.StartPoint != null && !value.StartPoint.EqualsWithinTolerance(startNode.Geometry, context.InstanceToValidate.AfterVerificationContext.Tolerances.GeometryTolerance))
            {
                throw new RoadSegmentStartPointDoesNotMatchNodeGeometry().ToValidationException();
            }

            return true;
        }

        public override string Name => nameof(LinesValidator);
    }
}
