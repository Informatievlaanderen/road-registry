namespace RoadRegistry.BackOffice.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AcceptedChange : IVerifiedChange
    {
        private readonly IRequestedChange _change;
        private readonly IReadOnlyCollection<Problem> _problems;

        public AcceptedChange(IRequestedChange change, IReadOnlyCollection<Problem> problems)
        {
            _change = change ?? throw new ArgumentNullException(nameof(change));
            _problems = problems ?? throw new ArgumentNullException(nameof(problems));
        }

        public Messages.AcceptedChange Translate()
        {
            var message = new Messages.AcceptedChange
            {
                Problems = _problems.Select(warning => warning.Translate()).ToArray()
            };
            _change.TranslateTo(message);
            return message;
        }
    }
}
