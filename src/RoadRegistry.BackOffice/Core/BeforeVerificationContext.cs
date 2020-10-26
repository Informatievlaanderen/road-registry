namespace RoadRegistry.BackOffice.Core
{
    using System;

    public class BeforeVerificationContext
    {
        public IRoadNetworkView BeforeView { get; }
        public IRequestedChangeIdentityTranslator Translator { get; }
        public double Tolerance { get; }

        internal BeforeVerificationContext(IRoadNetworkView view, IRequestedChangeIdentityTranslator translator, double tolerance)
        {
            BeforeView = view ?? throw new ArgumentNullException(nameof(view));
            Translator = translator ?? throw new ArgumentNullException(nameof(translator));
            if (tolerance <= 0.0) throw new ArgumentOutOfRangeException(nameof(tolerance),"The tolerance must be greater than 0.");
            Tolerance = tolerance;
        }

        public AfterVerificationContext CreateAfterVerificationContext(IRoadNetworkView afterView)
        {
            return new AfterVerificationContext(BeforeView, afterView, Translator, Tolerance);
        }
    }
}
