namespace RoadRegistry.Infrastructure;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DutchTranslations;
using FluentValidation;
using FluentValidation.Results;
using RoadRegistry.Infrastructure.Messages;
using RoadRegistry.ValueObjects.ProblemCodes;
using Problem = ValueObjects.Problems.Problem;
using ProblemParameter = ValueObjects.Problems.ProblemParameter;
using ProblemSeverity = Messages.ProblemSeverity;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithProblemCode<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        ProblemCode problemCode,
        Func<TProperty, string>? valueConverter = null)
    {
        rule.WithErrorCode(problemCode);

        string? propertyName = null;
        rule.Configure(r => { propertyName = r.PropertyName; });
        rule.WithState((_, value) =>
        {
            return new[]
            {
                new ProblemParameter(propertyName ?? "request", valueConverter is not null
                    ? valueConverter(value)
                    : string.Format(CultureInfo.InvariantCulture, "{0}", value)
                )
            };
        });

        return rule;
    }

    public static IRuleBuilderOptions<T, TProperty> WithProblemCode<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, ProblemCode problemCode, Func<TProperty, Problem> problemBuilder)
    {
        rule.WithErrorCode(problemCode);
        rule.WithState((_, value) => ToCustomState(problemBuilder(value).Parameters?.Select(x => x.Translate())));

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
        if (ex is DutchValidationException dutchValidationException)
        {
            return dutchValidationException;
        }

        return new DutchValidationException(ex.Errors.TranslateToDutch());
    }

    public static IEnumerable<ValidationFailure> TranslateToDutch(this IEnumerable<ValidationFailure> errors)
    {
        return errors
            .Select(x => new
            {
                ValidationFailure = x,
                Problem = new Infrastructure.Messages.Problem
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

    public static ProblemTranslation TranslateToDutch(this Problem problem)
    {
        return problem.Translate().TranslateToDutch();
    }

    public static ProblemTranslation TranslateToDutch(this Infrastructure.Messages.Problem problem)
    {
        return ProblemTranslator.Dutch(problem);
    }

    public static ValidationFailure ToValidationFailure(this Infrastructure.Messages.Problem problem, string? propertyName = null)
    {
        return new ValidationFailure
        {
            PropertyName = propertyName ?? string.Empty,
            ErrorCode = problem.Reason,
            ErrorMessage = ProblemTranslator.Dutch(problem).Message,
            CustomState = ToCustomState(problem.Parameters)
        };
    }

    public static async Task ValidateAndThrowAsync<T>(
        this IValidator<T> validator,
        T instance,
        Func<ValidationResult, Exception?> exceptionBuilder,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(instance, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw exceptionBuilder(validationResult) ?? new ValidationException(validationResult.Errors);
        }
    }

    private static Infrastructure.Messages.ProblemParameter[] ParseCustomState(object? customState)
    {
        if (customState is null)
        {
            return [];
        }

        if (customState is Dictionary<string, object> customStateDictionary)
        {
            return customStateDictionary.Select(x => new Infrastructure.Messages.ProblemParameter
            {
                Name = x.Key,
                Value = x.Value.ToString()!
            }).ToArray();
        }

        if (customState is Infrastructure.Messages.ProblemParameter[] problemParameters)
        {
            return problemParameters;
        }

        return ((ProblemParameter[])customState)
            .Select(problemParameter => problemParameter.Translate())
            .ToArray();
    }

    private static object ToCustomState(IEnumerable<Infrastructure.Messages.ProblemParameter>? parameters)
    {
        return parameters?.ToArray() ?? [];
    }
}
