namespace RoadRegistry.Model
{
    using System.Collections.Generic;
    using System.Linq;

    public class RejectedChange : IVerifiedChange
    {
        private readonly IRequestedChange _change;
        private readonly IReadOnlyCollection<Error> _errors;
        private readonly IReadOnlyCollection<Warning> _warnings;

        public RejectedChange(IRequestedChange change, IReadOnlyCollection<Error> errors, IReadOnlyCollection<Warning> warnings)
        {
            _change = change;
            _errors = errors;
            _warnings = warnings;
        }

        public Messages.RejectedChange Translate()
        {
            var message = new Messages.RejectedChange
            {
                Errors = _errors.Select(error => error.Translate()).ToArray(),
                Warnings = _warnings.Select(warning => warning.Translate()).ToArray()
            };
            _change.TranslateTo(message);
            return message;
        }
    }
}
