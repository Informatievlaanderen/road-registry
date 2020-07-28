namespace RoadRegistry.Syndication.Projections.StreetNameEvents
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Name = "StreetNameWasRegistered", Namespace = "")]
    public class StreetNameWasRegistered
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "MunicipalityId", Order = 2)]
        public Guid MunicipalityId { get; set; }

        [DataMember(Name = "NisCode", Order = 3)]
        public string NisCode { get; set; }
    }
}
