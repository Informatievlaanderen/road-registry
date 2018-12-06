namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class Reason
    {
        private readonly string _because;
        private readonly IReadOnlyCollection<ReasonParameter> _parameters;

        protected Reason(string because, IReadOnlyCollection<ReasonParameter> parameters)
        {
            _because = because ?? throw new ArgumentNullException(nameof(because));
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public Messages.Reason Translate() =>
            new Messages.Reason
            {
                Because = _because,
                Parameters = _parameters.Select(parameter => parameter.Translate()).ToArray()
            };
    }
}
