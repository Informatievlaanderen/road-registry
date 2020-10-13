namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Linq;

    public class RejectedChange : IVerifiedChange
    {
        public RejectedChange(IRequestedChange change, Problems problems)
        {
            RequestedChange = change ?? throw new ArgumentNullException(nameof(change));
            Problems = problems ?? throw new ArgumentNullException(nameof(problems));
        }

        public Messages.RejectedChange Translate()
        {
            var message = new Messages.RejectedChange
            {
                Problems = Problems.Select(problem => problem.Translate()).ToArray()
            };
            RequestedChange.TranslateTo(message);
            return message;
        }

        public IRequestedChange RequestedChange { get; }
        public Problems Problems { get; }
    }
}
