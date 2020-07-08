namespace RoadRegistry.Syndication.Projections.MunicipalityEvents
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Name = "MunicipalityWasRegistered", Namespace = "")]
    public class MunicipalityWasRegistered
    {
        [DataMember(Name = "MunicipalityId", Order = 1)]
        public Guid MunicipalityId { get; set; }

        [DataMember(Name = "NisCode", Order = 2)]
        public string NisCode { get; set; }
    }

    [DataContract(Name = "MunicipalityWasNamed", Namespace = "")]
    public class MunicipalityWasNamed
    {
        [DataMember(Name = "MunicipalityId", Order = 1)]
        public Guid MunicipalityId { get; set; }

        [DataMember(Name = "Name", Order = 3)]
        public string Name { get; set; }

        [DataMember(Name = "Language", Order = 3)]
        public Language Language { get; set; }
    }

    public enum Language
    {
        Dutch = 0,
        French = 1,
        German = 2,
        English = 3
    }
}
