namespace RoadRegistry.Model
{
    using System;

    public class ChangeContext
    {
        public RoadNetworkView View { get; }
        public IRequestedChangeIdentityTranslator Translator { get; }
        public double Tolerance { get; }

        public ChangeContext(
            RoadNetworkView view,
            IRequestedChangeIdentityTranslator translator)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
            Translator = translator ?? throw new ArgumentNullException(nameof(translator));
            Tolerance = 0.001; // 1 mm
        }
    }
}
