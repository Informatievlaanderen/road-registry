namespace RoadRegistry.BackOffice.Validators.AddRoadNode.After
{
    using System.Collections.Generic;
    using System.Linq;
    using Core;
    using FluentValidation;
    using FluentValidation.Validators;

    internal class GeometryTakenValidator : PropertyValidator<AddRoadNodeWithAfterVerificationContext, IReadOnlyDictionary<RoadNodeId, RoadNode>>
    {
        public override bool IsValid(ValidationContext<AddRoadNodeWithAfterVerificationContext> context, IReadOnlyDictionary<RoadNodeId, RoadNode> value)
        {
            var byOtherNode = value.Values.FirstOrDefault(n =>
                n.Id != context.InstanceToValidate.AddRoadNode.Id &&
                n.Geometry.EqualsWithinTolerance(context.InstanceToValidate.AddRoadNode.Geometry, context.InstanceToValidate.AfterVerificationContext.Tolerances.GeometryTolerance));
            if (byOtherNode != null)
            {
                throw new RoadNodeGeometryTaken(
                    context.InstanceToValidate.AfterVerificationContext.Translator.TranslateToTemporaryOrId(byOtherNode.Id)
                ).ToValidationException();
            }

            return true;
        }

        public override string Name => nameof(GeometryTakenValidator);
    }
}
