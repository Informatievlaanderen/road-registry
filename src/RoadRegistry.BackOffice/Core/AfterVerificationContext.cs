namespace RoadRegistry.BackOffice.Core
{
    using System;

    public class AfterVerificationContext
    {
        public IScopedRoadNetworkView BeforeView { get; }
        public IScopedRoadNetworkView AfterView { get; }
        public IRequestedChangeIdentityTranslator Translator { get; }
        public VerificationContextTolerances Tolerances { get; }

        internal AfterVerificationContext(IScopedRoadNetworkView beforeView, IScopedRoadNetworkView afterView, IRequestedChangeIdentityTranslator translator, VerificationContextTolerances tolerances)
        {
            BeforeView = beforeView ?? throw new ArgumentNullException(nameof(beforeView));
            AfterView  = afterView ?? throw new ArgumentNullException(nameof(afterView));
            Translator = translator ?? throw new ArgumentNullException(nameof(translator));
            Tolerances = tolerances ?? throw new ArgumentNullException(nameof(tolerances));
        }
    }
}
