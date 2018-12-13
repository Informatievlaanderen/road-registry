namespace RoadRegistry.Model
{
    using System.Collections.Generic;
    using System.Linq;

    public class AcceptedChange : IVerifiedChange
    {
        private readonly IRequestedChange _change;
        private readonly IReadOnlyCollection<Warning> _warnings;

        public AcceptedChange(IRequestedChange change, IReadOnlyCollection<Warning> warnings)
        {
            _change = change;
            _warnings = warnings;
        }

        public Messages.AcceptedChange Translate()
        {
            var message = new Messages.AcceptedChange
            {
                Warnings = _warnings.Select(warning => warning.Translate()).ToArray()
            };
            _change.TranslateTo(message);
            return message;
        }
    }
}
