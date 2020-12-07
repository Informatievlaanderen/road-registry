namespace RoadRegistry.BackOffice.Core
{
    using System;

    public class AfterVerificationContext
    {
        public IRoadNetworkView BeforeView { get; }
        public IRoadNetworkView AfterView { get; }
        public IRequestedChangeIdentityTranslator Translator { get; }
        public VerificationContextTolerances Tolerances { get; }

        internal AfterVerificationContext(IRoadNetworkView beforeView, IRoadNetworkView afterView, IRequestedChangeIdentityTranslator translator, VerificationContextTolerances tolerances)
        {
            BeforeView = beforeView ?? throw new ArgumentNullException(nameof(beforeView));
            AfterView  = afterView ?? throw new ArgumentNullException(nameof(afterView));
            Translator = translator ?? throw new ArgumentNullException(nameof(translator));
            Tolerances = tolerances ?? throw new ArgumentNullException(nameof(tolerances));
        }
    }
}
