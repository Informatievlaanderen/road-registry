namespace Wegenregister.Road.Commands
{
    using ValueObjects;

    public class RegisterRoad
    {
        public RoadId RoadId { get; }

        public RegisterRoad(
            RoadId roadId)
        {
            RoadId = roadId;
        }
    }
}
