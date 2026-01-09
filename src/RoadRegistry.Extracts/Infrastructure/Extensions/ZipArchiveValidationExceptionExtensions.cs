namespace RoadRegistry.Extracts.Infrastructure.Extensions;

using System.Linq;
using DutchTranslations;
using FluentValidation.Results;
using RoadRegistry.Infrastructure.DutchTranslations;
using Uploads;

public static class ZipArchiveValidationExceptionExtensions
{
    public static DutchValidationException ToDutchValidationException(this ZipArchiveValidationException ex)
    {
        var validationFailures = ex.Problems
            .Select(problem => problem.Translate())
            .Select(problem =>
                {
                    var translatedProblem = problem.TranslateToDutch();

                    return new ValidationFailure(problem.File, translatedProblem.Message)
                    {
                        ErrorCode = $"{problem.Severity}{translatedProblem.Code}"
                    };
                }
            )
            .ToList();
        return new DutchValidationException(validationFailures);
    }
}
