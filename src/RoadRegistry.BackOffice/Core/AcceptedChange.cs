namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AcceptedChange : IVerifiedChange
    {
        private readonly IRequestedChange _requestedChange;
        private readonly Problems _problems;

        public AcceptedChange(IRequestedChange change, Problems problems)
        {
            _requestedChange = change ?? throw new ArgumentNullException(nameof(change));
            _problems = problems ?? throw new ArgumentNullException(nameof(problems));
        }

        public Messages.AcceptedChange Translate()
        {
            var message = new Messages.AcceptedChange
            {
                Problems = _problems.Select(warning => warning.Translate()).ToArray()
            };
            _requestedChange.TranslateTo(message);
            return message;
        }
    }
}
