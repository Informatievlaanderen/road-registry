namespace RoadRegistry.BackOffice.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class Problem
    {
        private readonly string _reason;
        private readonly IReadOnlyCollection<ProblemParameter> _parameters;

        protected Problem(string reason, IReadOnlyCollection<ProblemParameter> parameters)
        {
            _reason = reason ?? throw new ArgumentNullException(nameof(reason));
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public Messages.Problem Translate() =>
            new Messages.Problem
            {
                Reason = _reason,
                Parameters = _parameters.Select(parameter => parameter.Translate()).ToArray()
            };
    }
}
