namespace RoadRegistry.BackOffice.Core
{
    using System;

    public class VerificationContext
    {
        public static readonly double TooCloseDistance = 2.0;

        public IRoadNetworkView View { get; }
        public IRequestedChangeIdentityTranslator Translator { get; }
        public double Tolerance { get; }

        public VerificationContext(IRoadNetworkView view, IRequestedChangeIdentityTranslator translator)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
            Translator = translator ?? throw new ArgumentNullException(nameof(translator));
            Tolerance = 0.001; // 1 mm
        }
    }
}
