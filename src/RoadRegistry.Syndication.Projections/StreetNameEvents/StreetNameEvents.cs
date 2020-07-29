namespace RoadRegistry.Syndication.Projections.StreetNameEvents
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using NodaTime;

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

    [DataContract(Name = "StreetNamePersistentLocalIdWasAssigned", Namespace = "")]
    public class StreetNamePersistentLocalIdWasAssigned
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "PersistentLocalId", Order = 2)]
        public int PersistentLocalId { get; set; }

        [DataMember(Name = "AssignmentDate", Order = 3)]
        public Instant AssignmentDate { get; set; }
    }

    [DataContract(Name = "StreetNameWasNamed", Namespace = "")]
    public class StreetNameWasNamed
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "Name", Order = 2)]
        public string Name { get; set; }

        [DataMember(Name = "Language", Order = 3)]
        public string LanguageValue { get; set; }

        [IgnoreDataMember]
        public StreetNameLanguage? Language
        {
            get
            {
                return string.IsNullOrEmpty(LanguageValue)
                    ? (StreetNameLanguage?) null
                    : Enum.Parse<StreetNameLanguage>(LanguageValue);
            }
        }
    }

    [DataContract(Name = "StreetNameNameWasCleared", Namespace = "")]
    public class StreetNameNameWasCleared
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "Language", Order = 2)]
        public string LanguageValue { get; set; }

        [IgnoreDataMember]
        public StreetNameLanguage? Language
        {
            get
            {
                return string.IsNullOrEmpty(LanguageValue)
                    ? (StreetNameLanguage?) null
                    : Enum.Parse<StreetNameLanguage>(LanguageValue);
            }
        }
    }

    [DataContract(Name = "StreetNameNameWasCorrected", Namespace = "")]
    public class StreetNameNameWasCorrected
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "Name", Order = 2)]
        public string Name { get; set; }

        [DataMember(Name = "Language", Order = 3)]
        public string LanguageValue { get; set; }

        [IgnoreDataMember]
        public StreetNameLanguage? Language
        {
            get
            {
                return string.IsNullOrEmpty(LanguageValue)
                    ? (StreetNameLanguage?) null
                    : Enum.Parse<StreetNameLanguage>(LanguageValue);
            }
        }
    }

    [DataContract(Name = "StreetNameNameWasCorrectedToCleared", Namespace = "")]
    public class StreetNameNameWasCorrectedToCleared
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "Language", Order = 2)]
        public string LanguageValue { get; set; }

        [IgnoreDataMember]
        public StreetNameLanguage? Language
        {
            get
            {
                return string.IsNullOrEmpty(LanguageValue)
                    ? (StreetNameLanguage?) null
                    : Enum.Parse<StreetNameLanguage>(LanguageValue);
            }
        }
    }

    [DataContract(Name = "StreetNameBecameCurrent", Namespace = "")]
    public class StreetNameBecameCurrent
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameWasCorrectedToCurrent", Namespace = "")]
    public class StreetNameWasCorrectedToCurrent
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameWasProposed", Namespace = "")]
    public class StreetNameWasProposed
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameWasCorrectedToProposed", Namespace = "")]
    public class StreetNameWasCorrectedToProposed
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameWasRetired", Namespace = "")]
    public class StreetNameWasRetired
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameWasCorrectedToRetired", Namespace = "")]
    public class StreetNameWasCorrectedToRetired
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameStatusWasRemoved", Namespace = "")]
    public class StreetNameStatusWasRemoved
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameStatusWasCorrectedToRemoved", Namespace = "")]
    public class StreetNameStatusWasCorrectedToRemoved
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameBecameComplete", Namespace = "")]
    public class StreetNameBecameComplete
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameBecameIncomplete", Namespace = "")]
    public class StreetNameBecameIncomplete
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameWasRemoved", Namespace = "")]
    public class StreetNameWasRemoved
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }


    public enum StreetNameLanguage
    {
        Dutch = 0,
        French = 1,
        German = 2,
        English = 3
    }
}
