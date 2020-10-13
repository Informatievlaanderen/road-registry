namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AcceptedChange : IVerifiedChange
    {
        public AcceptedChange(IRequestedChange change, Problems problems)
        {
            RequestedChange = change ?? throw new ArgumentNullException(nameof(change));
            Problems = problems ?? throw new ArgumentNullException(nameof(problems));
        }

        public IRequestedChange RequestedChange { get; }
        public Problems Problems { get; }

        public Messages.AcceptedChange Translate()
        {
            var message = new Messages.AcceptedChange
            {
                Problems = Problems.Select(warning => warning.Translate()).ToArray()
            };
            RequestedChange.TranslateTo(message);
            return message;
        }
    }
}
