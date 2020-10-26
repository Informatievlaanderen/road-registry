namespace RoadRegistry.BackOffice.Core
{
    using System;

    public class AfterVerificationContext
    {
        public IRoadNetworkView BeforeView { get; }
        public IRoadNetworkView AfterView { get; }
        public IRequestedChangeIdentityTranslator Translator { get; }
        public double Tolerance { get; }

        internal AfterVerificationContext(IRoadNetworkView beforeView, IRoadNetworkView afterView, IRequestedChangeIdentityTranslator translator, double tolerance)
        {
            BeforeView = beforeView ?? throw new ArgumentNullException(nameof(beforeView));
            AfterView = afterView ?? throw new ArgumentNullException(nameof(afterView));
            Translator = translator ?? throw new ArgumentNullException(nameof(translator));
            if (tolerance <= 0.0) throw new ArgumentOutOfRangeException(nameof(tolerance),"The tolerance must be greater than 0.");
            Tolerance = tolerance;
        }
    }
}
