namespace RoadRegistry.BackOffice.Core;

using System;

public class AfterVerificationContext
{
    public AfterVerificationContext(IRoadNetworkView rootView, IScopedRoadNetworkView beforeView, IScopedRoadNetworkView afterView, IRequestedChangeIdentityTranslator translator, VerificationContextTolerances tolerances, IOrganizations organizations)
    {
        RootView = rootView ?? throw new ArgumentNullException(nameof(rootView));
        BeforeView = beforeView ?? throw new ArgumentNullException(nameof(beforeView));
        AfterView = afterView ?? throw new ArgumentNullException(nameof(afterView));
        Translator = translator ?? throw new ArgumentNullException(nameof(translator));
        Tolerances = tolerances ?? throw new ArgumentNullException(nameof(tolerances));
        Organizations = organizations;
    }

    public IRoadNetworkView RootView { get; }
    public IScopedRoadNetworkView BeforeView { get; }
    public IScopedRoadNetworkView AfterView { get; }
    public IRequestedChangeIdentityTranslator Translator { get; }
    public VerificationContextTolerances Tolerances { get; }
    public IOrganizations Organizations { get; }
}
