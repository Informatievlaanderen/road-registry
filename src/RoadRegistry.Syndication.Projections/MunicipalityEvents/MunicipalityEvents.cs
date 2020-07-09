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

        [DataMember(Name = "Name", Order = 2)]
        public string Name { get; set; }

        [DataMember(Name = "Language", Order = 3)]
        public Language Language { get; set; }
    }

    [DataContract(Name = "MunicipalityNameWasCleared", Namespace = "")]
    public class MunicipalityNameWasCleared
    {
        [DataMember(Name = "MunicipalityId", Order = 1)]
        public Guid MunicipalityId { get; set; }

        [DataMember(Name = "Language", Order = 2)]
        public Language Language { get; set; }
    }

    [DataContract(Name = "MunicipalityNameWasCorrected", Namespace = "")]
    public class MunicipalityNameWasCorrected
    {
        [DataMember(Name = "MunicipalityId", Order = 1)]
        public Guid MunicipalityId { get; set; }

        [DataMember(Name = "Name", Order = 2)]
        public string Name { get; set; }

        [DataMember(Name = "Language", Order = 3)]
        public Language Language { get; set; }
    }

    [DataContract(Name = "MunicipalityNameWasCorrectedToCleared", Namespace = "")]
    public class MunicipalityNameWasCorrectedToCleared
    {
        [DataMember(Name = "MunicipalityId", Order = 1)]
        public Guid MunicipalityId { get; set; }


        [DataMember(Name = "Language", Order = 2)]
        public Language Language { get; set; }
    }

    [DataContract(Name = "MunicipalityNisCodeWasDefined", Namespace = "")]
    public class MunicipalityNisCodeWasDefined
    {
        [DataMember(Name = "MunicipalityId", Order = 1)]
        public Guid MunicipalityId { get; set; }


        [DataMember(Name = "NisCode", Order = 2)]
        public string NisCode { get; set; }
    }


    [DataContract(Name = "MunicipalityNisCodeWasCorrected", Namespace = "")]
    public class MunicipalityNisCodeWasCorrected
    {
        [DataMember(Name = "MunicipalityId", Order = 1)]
        public Guid MunicipalityId { get; set; }


        [DataMember(Name = "NisCode", Order = 2)]
        public string NisCode { get; set; }
    }

    [DataContract(Name = "MunicipalityBecameCurrent", Namespace = "")]
    public class MunicipalityBecameCurrent
    {
        [DataMember(Name = "MunicipalityId", Order = 1)]
        public Guid MunicipalityId { get; set; }
    }

    [DataContract(Name = "MunicipalityWasCorrectedToCurrent", Namespace = "")]
    public class MunicipalityWasCorrectedToCurrent
    {
        [DataMember(Name = "MunicipalityId", Order = 1)]
        public Guid MunicipalityId { get; set; }
    }

    [DataContract(Name = "MunicipalityBecameRetired", Namespace = "")]
    public class MunicipalityBecameRetired
    {
        [DataMember(Name = "MunicipalityId", Order = 1)]
        public Guid MunicipalityId { get; set; }
    }

    [DataContract(Name = "MunicipalityWasCorrectedToRetired", Namespace = "")]
    public class MunicipalityWasCorrectedToRetired
    {
        [DataMember(Name = "MunicipalityId", Order = 1)]
        public Guid MunicipalityId { get; set; }
    }

    public enum Language
    {
        Dutch = 0,
        French = 1,
        German = 2,
        English = 3
    }
}
