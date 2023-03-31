namespace RoadRegistry.BackOffice.Extensions;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Core;
using Core.ProblemCodes;
using DutchTranslations;
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
        rule.WithState((item, value) =>
        {
            return new[] { new ProblemParameter(propertyName, valueConverter is not null
                ? valueConverter(value)
                : string.Format(CultureInfo.InvariantCulture, "{0}", value)
                ) };
        });

        return rule;
    }

    public static ValidationException TranslateToDutch(this ValidationException ex)
    {
        return new ValidationException(ex.Errors.TranslateToDutch());
    }

    public static IEnumerable<ValidationFailure> TranslateToDutch(this IEnumerable<ValidationFailure> errors)
    {
        return errors
            .Select(x => new
            {
                x.PropertyName,
                Problem = new Messages.Problem
                {
                    Severity = ProblemSeverity.Error,
                    Reason = x.ErrorCode,
                    Parameters = ((ProblemParameter[])x.CustomState)?
                        .Select(problemParameter => problemParameter.Translate())
                        .ToArray() ?? Array.Empty<Messages.ProblemParameter>()
                }
            })
            .Select(x =>
            {
                var problem = ProblemTranslator.Dutch(x.Problem);
                return new ValidationFailure
                {
                    PropertyName = x.PropertyName,
                    ErrorCode = problem.Code,
                    ErrorMessage = problem.Message
                };
            });
    }

    public static ProblemTranslation TranslateToDutch(this Problem problem)
    {
        return ProblemTranslator.Dutch(problem.Translate());
    }

}
