namespace RoadRegistry.BackOffice.Validators.AddRoadNode.After
{
    using System.Linq;
    using Core;
    using FluentValidation;
    using FluentValidation.Validators;

    internal class TypeMatchesConnectedValidator : PropertyValidator<AddRoadNodeWithAfterVerificationContext, AddRoadNode>
    {
        public override bool IsValid(ValidationContext<AddRoadNodeWithAfterVerificationContext> context, AddRoadNode value)
        {
            var node = context.InstanceToValidate.AfterVerificationContext.AfterView.View.Nodes[value.Id];

            var problems = Problems.None;

            problems = context.InstanceToValidate.AfterVerificationContext.AfterView.Segments.Values
                .Where(s =>
                    !node.Segments.Contains(s.Id) &&
                    s.Geometry.IsWithinDistance(value.Geometry, Distances.TooClose)
                )
                .Aggregate(
                    problems,
                    (current, segment) => current.Add(new RoadNodeTooClose(context.InstanceToValidate.AfterVerificationContext.Translator.TranslateToTemporaryOrId(segment.Id))));

            problems = problems.AddRange(node.VerifyTypeMatchesConnectedSegmentCount(context.InstanceToValidate.AfterVerificationContext.AfterView.View, context.InstanceToValidate.AfterVerificationContext.Translator));
            if (problems.Any())
            {
                throw problems.ToValidationException();
            }

            return true;
        }

        public override string Name => nameof(TypeMatchesConnectedValidator);
    }
}
