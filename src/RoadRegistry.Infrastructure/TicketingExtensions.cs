namespace RoadRegistry.Infrastructure
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using DutchTranslations;
    using RoadRegistry.Infrastructure.Messages;
    using TicketingService.Abstractions;
    using ProblemContext = RoadRegistry.ValueObjects.Problems.ProblemContext;

    public static class TicketingExtensions
    {
        public static TicketError ToTicketError(this Problem problem, IProblemTranslator translator)
        {
            var translation = problem.TranslateToDutch(translator);
            var code = $"{problem.Severity}{translation.Code}";

            var errorContext = BuildErrorContext(problem);
            return errorContext.Count > 0
                ? new TicketError(translation.Message, code, errorContext)
                : new TicketError(translation.Message, code);
        }

        // A problem's context travels as ordinary parameters in a message, so it is picked out again by name.
        private static Dictionary<string, object> BuildErrorContext(Problem problem)
        {
            var errorContext = new Dictionary<string, object>();

            foreach (var parameter in (problem.Parameters ?? []).Where(x => ProblemContext.ParameterNames.Contains(x.Name)))
            {
                errorContext.TryAdd(parameter.Name, int.TryParse(parameter.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
                    ? number
                    : parameter.Value);
            }

            return errorContext;
        }
    }
}
