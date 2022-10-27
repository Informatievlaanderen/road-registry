namespace RoadRegistry.BackOffice.Core;

using FluentValidation;

public class RemoveRoadNodeValidator : AbstractValidator<Messages.RemoveRoadNode>
{
    public RemoveRoadNodeValidator()
    {
        RuleFor(c => c.Id).GreaterThanOrEqualTo(1);
    }
}