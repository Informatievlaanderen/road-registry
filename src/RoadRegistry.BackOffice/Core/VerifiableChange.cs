namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Linq;

    public class VerifiableChange
    {
        private readonly IRequestedChange _requestedChange;
        private readonly Problems _problems;

        public VerifiableChange(IRequestedChange change)
        {
            _requestedChange = change ?? throw new ArgumentNullException(nameof(change));
            _problems = Problems.None;
        }

        private VerifiableChange(IRequestedChange change, Problems problems)
        {
            _requestedChange = change;
            _problems = problems;
        }

        public bool HasErrors => _problems.OfType<Error>().Any();
        public bool HasWarnings => _problems.OfType<Warning>().Any();

        public VerifiableChange VerifyBefore(BeforeVerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return new VerifiableChange(
                _requestedChange,
                _problems.AddRange(_requestedChange.VerifyBefore(context)));
        }

        public VerifiableChange VerifyAfter(AfterVerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return new VerifiableChange(
                _requestedChange,
                _problems.AddRange(_requestedChange.VerifyAfter(context)));
        }

        public IVerifiedChange AsVerifiedChange()
        {
            IVerifiedChange change;
            if (HasErrors)
            {
                change = new RejectedChange(_requestedChange, _problems);
            }
            else
            {
                change = new AcceptedChange(_requestedChange, _problems);
            }
            return change;
        }
    }
}
