namespace RoadRegistry.BackOffice.Core;

using System;

public class BeforeVerificationContext
{
    internal BeforeVerificationContext(
        IRoadNetworkView rootView,
        IScopedRoadNetworkView view,
        IRequestedChangeIdentityTranslator translator,
        VerificationContextTolerances tolerances)
    {
        RootView = rootView ?? throw new ArgumentNullException(nameof(rootView));
        BeforeView = view ?? throw new ArgumentNullException(nameof(view));
        Translator = translator ?? throw new ArgumentNullException(nameof(translator));
        Tolerances = tolerances ?? throw new ArgumentNullException(nameof(tolerances));
    }

    public IRoadNetworkView RootView { get; }
    public IScopedRoadNetworkView BeforeView { get; }
    public VerificationContextTolerances Tolerances { get; }
    public IRequestedChangeIdentityTranslator Translator { get; }

    public AfterVerificationContext CreateAfterVerificationContext(IRoadNetworkView afterView)
    {
        return new AfterVerificationContext(RootView, BeforeView, afterView.CreateScopedView(BeforeView.Scope), Translator, Tolerances);
    }
}
