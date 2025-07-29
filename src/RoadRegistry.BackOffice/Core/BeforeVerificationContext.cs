namespace RoadRegistry.BackOffice.Core;

using System;

public class BeforeVerificationContext
{
    internal BeforeVerificationContext(
        IRoadNetworkView rootView,
        IScopedRoadNetworkView view,
        IRequestedChangeIdentityTranslator translator,
        VerificationContextTolerances tolerances
        )
    {
        RootView = rootView.ThrowIfNull();
        BeforeView = view.ThrowIfNull();
        Translator = translator.ThrowIfNull();
        Tolerances = tolerances.ThrowIfNull();
    }

    public IRoadNetworkView RootView { get; }
    public IScopedRoadNetworkView BeforeView { get; }
    public VerificationContextTolerances Tolerances { get; }
    public IRequestedChangeIdentityTranslator Translator { get; }

    public AfterVerificationContext CreateAfterVerificationContext(IRoadNetworkView afterView, IOrganizations organizations)
    {
        return new AfterVerificationContext(RootView, BeforeView, afterView.CreateScopedView(BeforeView.Scope), Translator, Tolerances, organizations);
    }
}
