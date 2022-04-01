namespace RoadRegistry.BackOffice.Validators
{
    using System.Collections.Generic;
    using System.Linq;
    using Core;
    using FluentValidation;
    using FluentValidation.Results;

    internal static class FluentValidationExtensions
    {
        internal static ValidationException ToValidationException(this Problem problem) => new ValidationException(problem.Reason, problem.Parameters.Select(x => new ValidationFailure(x.Name, string.Empty, x)));

        internal static ValidationException ToValidationException(this Problems problems) => new ValidationException(problems.FirstOrDefault()?.Reason, problems
            .SelectMany(x => x.Parameters)
            .Select(x => new ValidationFailure(x.Name, string.Empty, x)));

        internal static ValidationException ToValidationException(this IEnumerable<IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction> segments)
        {
            var segmentList = segments.ToList();
            return new ValidationException(
                segmentList.FirstOrDefault()?.Reason, segmentList
                    .SelectMany(x => x.Parameters
                        .Select(x => new ValidationFailure(x.Name, x.Value))));
        }

        internal static Problems ToProblems(this ValidationResult validationResult)
        {
            var problems = Problems.None;

            foreach (var error in validationResult.Errors)
            {
                problems.Add(new Error(error.ErrorMessage));
            }

            return problems;
        }
    }
}
