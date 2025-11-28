namespace RoadRegistry.BackOffice.Abstractions.Extensions;

using BackOffice.Extensions;
using FluentValidation;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandHandling;
using ValueObjects.ProblemCodes;

public static class ValidationExtensions
{
    public static Task ValidateRoadSegmentIdAndThrowAsync(this AbstractValidator<int> validator, int instance, CancellationToken cancellationToken)
    {
        return validator.ValidateAndThrowAsync(instance, validationResult =>
        {
            if (validationResult.Errors.Any(x => x.ErrorCode == ProblemCode.RoadSegment.NotFound))
            {
                return new RoadSegmentNotFoundException();
            }

            return null;
        }, cancellationToken);
    }
}
