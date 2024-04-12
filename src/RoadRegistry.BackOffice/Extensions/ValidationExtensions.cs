namespace RoadRegistry.BackOffice.Extensions;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.ProblemCodes;
using DutchTranslations;
using Exceptions;
using FluentValidation;
using FluentValidation.Results;
using ProblemSeverity = Messages.ProblemSeverity;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithProblemCode<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, ProblemCode problemCode, Func<TProperty, string> valueConverter = null)
    {
        rule.WithErrorCode(problemCode);
        
        string propertyName = null;
        rule.Configure(r =>
        {
            propertyName = r.PropertyName;
        });
        rule.WithState((parent, value) =>
        {
            return new[] { new ProblemParameter(propertyName ?? "request", valueConverter is not null
                ? valueConverter(value)
                : string.Format(CultureInfo.InvariantCulture, "{0}", value)
                ) };
        });

        return rule;
    }

    public static IRuleBuilderOptions<T, TProperty> WithProblemCode<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, ProblemCode problemCode, Func<TProperty, Problem> problemBuilder)
    {
        rule.WithErrorCode(problemCode);
        rule.WithState((parent, value) => ToCustomState(problemBuilder(value).Parameters?.Select(x => x.Translate())));

        return rule;
    }

    public static IRuleBuilderOptions<T, TProperty> WithProblemCode<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, ProblemCode problemCode, Func<T, TProperty, Problem> problemBuilder)
    {
        rule.WithErrorCode(problemCode);
        rule.WithState((parent, value) => ToCustomState(problemBuilder(parent, value).Parameters?.Select(x => x.Translate())));

        return rule;
    }

    public static DutchValidationException TranslateToDutch(this ValidationException ex)
    {
        return new DutchValidationException(ex.Errors.TranslateToDutch());
    }

    public static IEnumerable<ValidationFailure> TranslateToDutch(this IEnumerable<ValidationFailure> errors)
    {
        return errors
            .Select(x => new
            {
                ValidationFailure = x,
                Problem = new Messages.Problem
                {
                    Severity = ProblemSeverity.Error,
                    Reason = x.ErrorCode,
                    Parameters = ParseCustomState(x.CustomState)
                }
            })
            .Select(x =>
            {
                if (ProblemCode.FromReason(x.Problem.Reason) is null)
                {
                    return x.ValidationFailure;
                }

                var problem = ProblemTranslator.Dutch(x.Problem);
                return new ValidationFailure
                {
                    PropertyName = x.ValidationFailure.PropertyName,
                    ErrorCode = problem.Code,
                    ErrorMessage = problem.Message
                };
            });
    }

    public static ValidationFailure ToValidationFailure(this Messages.Problem problem, string propertyName = null)
    {
        return new ValidationFailure
        {
            PropertyName = propertyName ?? string.Empty,
            ErrorCode = problem.Reason,
            ErrorMessage = ProblemTranslator.Dutch(problem).Message,
            CustomState = ToCustomState(problem.Parameters)
        };
    }

    public static ProblemTranslation TranslateToDutch(this Problem problem)
    {
        return problem.Translate().TranslateToDutch();
    }
    public static ProblemTranslation TranslateToDutch(this Messages.Problem problem)
    {
        return ProblemTranslator.Dutch(problem);
    }
    public static ProblemTranslation TranslateToDutch(this Messages.FileProblem problem)
    {
        return FileProblemTranslator.Dutch(problem);
    }

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

    public static async Task ValidateAndThrowAsync<T>(this IValidator<T> validator, T instance, Func<ValidationResult, Exception> exceptionBuilder, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(instance, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw exceptionBuilder(validationResult) ?? new ValidationException(validationResult.Errors);
        }
    }

    private static Messages.ProblemParameter[] ParseCustomState(object customState)
    {
        if (customState is null)
        {
            return Array.Empty<Messages.ProblemParameter>();
        }

        if (customState is Messages.ProblemParameter[] problemParameters)
        {
            return problemParameters;
        }

        return ((ProblemParameter[])customState)
            .Select(problemParameter => problemParameter.Translate())
            .ToArray();
    }

    private static object ToCustomState(IEnumerable<Messages.ProblemParameter> parameters)
    {
        return parameters?.ToArray() ?? Array.Empty<Messages.ProblemParameter>();
    }
}
