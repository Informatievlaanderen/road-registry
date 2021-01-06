namespace RoadRegistry.BackOffice.Core
{
    using System;
    using NetTopologySuite.Geometries;

    public class BeforeVerificationContext
    {
        public IScopedRoadNetworkView BeforeView { get; }
        public IRequestedChangeIdentityTranslator Translator { get; }
        public VerificationContextTolerances Tolerances { get; }

        internal BeforeVerificationContext(
            IScopedRoadNetworkView view,
            IRequestedChangeIdentityTranslator translator,
            VerificationContextTolerances tolerances)
        {
            BeforeView = view ?? throw new ArgumentNullException(nameof(view));
            Translator = translator ?? throw new ArgumentNullException(nameof(translator));
            Tolerances = tolerances ?? throw new ArgumentNullException(nameof(tolerances));
        }

        public AfterVerificationContext CreateAfterVerificationContext(IRoadNetworkView afterView)
        {
            return new AfterVerificationContext(BeforeView, afterView.CreateScopedView(BeforeView.Scope), Translator, Tolerances);
        }
    }
}
