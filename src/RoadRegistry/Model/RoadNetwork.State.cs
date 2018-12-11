namespace RoadRegistry.Model
{
    using System;
    using Framework;
    using Messages;

    public partial class RoadNetwork : EventSourcedEntity
    {
        public static readonly Func<RoadNetwork> Factory = () => new RoadNetwork();
        public static readonly double TooCloseDistance = 2.0;

        private RoadNetworkView _view;

        private RoadNetwork()
        {
            _view = RoadNetworkView.Empty;

            On<ImportedRoadNode>(e =>
            {
                _view = _view.Given(e);
            });

            On<ImportedGradeSeparatedJunction>(e =>
            {
                _view = _view.Given(e);
            });

            On<ImportedRoadSegment>(e =>
            {
                _view = _view.Given(e);
            });

            On<RoadNetworkChangesAccepted>(e =>
            {
                _view = _view.Given(e);
            });
        }
    }
}
