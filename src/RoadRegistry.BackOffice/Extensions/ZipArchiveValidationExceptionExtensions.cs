namespace RoadRegistry.BackOffice.Extensions;

using System.Linq;
using CommandHandling.DutchTranslations;
using DutchTranslations;
using Exceptions;
using FluentValidation.Results;

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
