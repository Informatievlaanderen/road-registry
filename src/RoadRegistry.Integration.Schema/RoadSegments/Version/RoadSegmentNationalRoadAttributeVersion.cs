namespace RoadRegistry.Integration.Schema.RoadSegments.Version
{
    using System;

    public class RoadSegmentNationalRoadAttributeVersion
    {
        public required long Position { get; init; }
        public required int Id { get; init; }
        public required int RoadSegmentId { get; init; }

        public string Number { get; set; }
        public bool IsRemoved { get; set; }
        public string OrganizationId { get; set; }
        public string OrganizationName { get; set; }

        public DateTimeOffset VersionTimestamp { get; set; }
        public DateTimeOffset CreatedOnTimestamp { get; set; }

        public RoadSegmentNationalRoadAttributeVersion Clone(long newPosition)
        {
            var newItem = new RoadSegmentNationalRoadAttributeVersion
            {
                Position = newPosition,
                Id = Id,
                RoadSegmentId = RoadSegmentId,

                Number = Number,
                IsRemoved = IsRemoved,
                OrganizationId = OrganizationId,
                OrganizationName = OrganizationName,

                VersionTimestamp = VersionTimestamp,
                CreatedOnTimestamp = CreatedOnTimestamp
            };

            return newItem;
        }
    }
}
