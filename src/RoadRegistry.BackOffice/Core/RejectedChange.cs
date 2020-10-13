namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Linq;

    public class RejectedChange : IVerifiedChange
    {
        private readonly IRequestedChange _requestedChange;
        private readonly Problems _problems;

        public RejectedChange(IRequestedChange change, Problems problems)
        {
            _requestedChange = change ?? throw new ArgumentNullException(nameof(change));
            _problems = problems ?? throw new ArgumentNullException(nameof(problems));
        }

        public Messages.RejectedChange Translate()
        {
            var message = new Messages.RejectedChange
            {
                Problems = _problems.Select(problem => problem.Translate()).ToArray()
            };
            _requestedChange.TranslateTo(message);
            return message;
        }
    }
}
