namespace RoadRegistry.BackOffice.Model
{
    using System.Collections.Generic;
    using System.Linq;

    public class RejectedChange : IVerifiedChange
    {
        private readonly IRequestedChange _change;
        private readonly IReadOnlyCollection<Problem> _problems;

        public RejectedChange(IRequestedChange change, IReadOnlyCollection<Problem> problems)
        {
            _change = change;
            _problems = problems;
        }

        public Messages.RejectedChange Translate()
        {
            var message = new Messages.RejectedChange
            {
                Problems = _problems.Select(problem => problem.Translate()).ToArray()
            };
            _change.TranslateTo(message);
            return message;
        }
    }
}
