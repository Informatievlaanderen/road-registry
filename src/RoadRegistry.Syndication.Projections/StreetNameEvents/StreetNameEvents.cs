namespace RoadRegistry.Syndication.Projections.StreetNameEvents
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using NodaTime;

    [DataContract(Name = "StreetNameWasRegistered", Namespace = "")]
    public class StreetNameWasRegistered : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "MunicipalityId", Order = 2)]
        public Guid MunicipalityId { get; set; }

        [DataMember(Name = "NisCode", Order = 3)]
        public string NisCode { get; set; }
    }

    [DataContract(Name = "StreetNamePersistentLocalIdentifierWasAssigned", Namespace = "")]
    public class StreetNamePersistentLocalIdentifierWasAssigned : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "PersistentLocalId", Order = 2)]
        public int PersistentLocalId { get; set; }

        [DataMember(Name = "AssignmentDate", Order = 3)]
        public Instant AssignmentDate { get; set; }
    }

    [DataContract(Name = "StreetNameWasNamed", Namespace = "")]
    public class StreetNameWasNamed : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "Name", Order = 2)] public string Name { get; set; }

        [DataMember(Name = "Language", Order = 3)]
        public string LanguageValue { get; set; }

        [IgnoreDataMember]
        public StreetNameLanguage? Language =>
            string.IsNullOrEmpty(LanguageValue)
                ? (StreetNameLanguage?)null
                : Enum.Parse<StreetNameLanguage>(LanguageValue);
    }

    [DataContract(Name = "StreetNameNameWasCleared", Namespace = "")]
    public class StreetNameNameWasCleared : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "Language", Order = 2)]
        public string LanguageValue { get; set; }

        [IgnoreDataMember]
        public StreetNameLanguage? Language =>
            string.IsNullOrEmpty(LanguageValue)
                ? (StreetNameLanguage?)null
                : Enum.Parse<StreetNameLanguage>(LanguageValue);
    }

    [DataContract(Name = "StreetNameNameWasCorrected", Namespace = "")]
    public class StreetNameNameWasCorrected : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "Name", Order = 2)] public string Name { get; set; }

        [DataMember(Name = "Language", Order = 3)]
        public string LanguageValue { get; set; }

        [IgnoreDataMember]
        public StreetNameLanguage? Language =>
            string.IsNullOrEmpty(LanguageValue)
                ? (StreetNameLanguage?)null
                : Enum.Parse<StreetNameLanguage>(LanguageValue);
    }

    [DataContract(Name = "StreetNameNameWasCorrectedToCleared", Namespace = "")]
    public class StreetNameNameWasCorrectedToCleared : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "Language", Order = 2)]
        public string LanguageValue { get; set; }

        [IgnoreDataMember]
        public StreetNameLanguage? Language =>
            string.IsNullOrEmpty(LanguageValue)
                ? (StreetNameLanguage?)null
                : Enum.Parse<StreetNameLanguage>(LanguageValue);
    }

    [DataContract(Name = "StreetNameBecameCurrent", Namespace = "")]
    public class StreetNameBecameCurrent : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameWasCorrectedToCurrent", Namespace = "")]
    public class StreetNameWasCorrectedToCurrent : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameWasProposed", Namespace = "")]
    public class StreetNameWasProposed : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameWasCorrectedToProposed", Namespace = "")]
    public class StreetNameWasCorrectedToProposed : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameWasRetired", Namespace = "")]
    public class StreetNameWasRetired : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameWasCorrectedToRetired", Namespace = "")]
    public class StreetNameWasCorrectedToRetired : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameStatusWasRemoved", Namespace = "")]
    public class StreetNameStatusWasRemoved : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameStatusWasCorrectedToRemoved", Namespace = "")]
    public class StreetNameStatusWasCorrectedToRemoved : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameBecameComplete", Namespace = "")]
    public class StreetNameBecameComplete : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameBecameIncomplete", Namespace = "")]
    public class StreetNameBecameIncomplete : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameWasRemoved", Namespace = "")]
    public class StreetNameWasRemoved : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }
    }

    [DataContract(Name = "StreetNameHomonymAdditionWasCleared", Namespace = "")]
    public class StreetNameHomonymAdditionWasCleared : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "Language", Order = 2)]
        public string LanguageValue { get; set; }

        [IgnoreDataMember]
        public StreetNameLanguage? Language =>
            string.IsNullOrEmpty(LanguageValue)
                ? (StreetNameLanguage?)null
                : Enum.Parse<StreetNameLanguage>(LanguageValue);
    }

    [DataContract(Name = "StreetNameHomonymAdditionWasCorrected", Namespace = "")]
    public class StreetNameHomonymAdditionWasCorrected : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "HomonymAddition", Order = 2)]
        public string HomonymAddition { get; set; }

        [DataMember(Name = "Language", Order = 3)]
        public string LanguageValue { get; set; }

        [IgnoreDataMember]
        public StreetNameLanguage? Language =>
            string.IsNullOrEmpty(LanguageValue)
                ? (StreetNameLanguage?)null
                : Enum.Parse<StreetNameLanguage>(LanguageValue);
    }

    [DataContract(Name = "StreetNameHomonymAdditionWasCorrectedToCleared", Namespace = "")]
    public class StreetNameHomonymAdditionWasCorrectedToCleared : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "Language", Order = 2)]
        public string LanguageValue { get; set; }

        [IgnoreDataMember]
        public StreetNameLanguage? Language =>
            string.IsNullOrEmpty(LanguageValue)
                ? (StreetNameLanguage?)null
                : Enum.Parse<StreetNameLanguage>(LanguageValue);
    }

    [DataContract(Name = "StreetNameHomonymAdditionWasDefined", Namespace = "")]
    public class StreetNameHomonymAdditionWasDefined : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "HomonymAddition", Order = 2)]
        public string HomonymAddition { get; set; }

        [DataMember(Name = "Language", Order = 3)]
        public string LanguageValue { get; set; }

        [IgnoreDataMember]
        public StreetNameLanguage? Language =>
            string.IsNullOrEmpty(LanguageValue)
                ? (StreetNameLanguage?)null
                : Enum.Parse<StreetNameLanguage>(LanguageValue);
    }

    [DataContract(Name = "StreetNamePrimaryLanguageWasCleared", Namespace = "")]
    public class StreetNamePrimaryLanguageWasCleared : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "MunicipalityId", Order = 2)]
        public Guid MunicipalityId { get; set; }
    }

    [DataContract(Name = "StreetNamePrimaryLanguageWasCorrected", Namespace = "")]
    public class StreetNamePrimaryLanguageWasCorrected : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "MunicipalityId", Order = 2)]
        public Guid MunicipalityId { get; set; }

        [DataMember(Name = "PrimaryLanguage", Order = 3)]
        public string PrimaryLanguageValue { get; set; }

        [IgnoreDataMember]
        public StreetNameLanguage? PrimaryLanguage =>
            string.IsNullOrEmpty(PrimaryLanguageValue)
                ? (StreetNameLanguage?)null
                : Enum.Parse<StreetNameLanguage>(PrimaryLanguageValue);
    }

    [DataContract(Name = "StreetNamePrimaryLanguageWasCorrectedToCleared", Namespace = "")]
    public class StreetNamePrimaryLanguageWasCorrectedToCleared : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "MunicipalityId", Order = 2)]
        public Guid MunicipalityId { get; set; }
    }

    [DataContract(Name = "StreetNamePrimaryLanguageWasDefined", Namespace = "")]
    public class StreetNamePrimaryLanguageWasDefined : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "MunicipalityId", Order = 2)]
        public Guid MunicipalityId { get; set; }

        [DataMember(Name = "PrimaryLanguage", Order = 3)]
        public string PrimaryLanguageValue { get; set; }

        [IgnoreDataMember]
        public StreetNameLanguage? PrimaryLanguage =>
            string.IsNullOrEmpty(PrimaryLanguageValue)
                ? (StreetNameLanguage?)null
                : Enum.Parse<StreetNameLanguage>(PrimaryLanguageValue);
    }

    [DataContract(Name = "StreetNamePrimaryLanguageWasCleared", Namespace = "")]
    public class StreetNameSecondaryLanguageWasCleared : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "MunicipalityId", Order = 2)]
        public Guid MunicipalityId { get; set; }
    }

    [DataContract(Name = "StreetNameSecondaryLanguageWasCorrected", Namespace = "")]
    public class StreetNameSecondaryLanguageWasCorrected : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "MunicipalityId", Order = 2)]
        public Guid MunicipalityId { get; set; }

        [DataMember(Name = "SecondaryLanguage", Order = 3)]
        public string SecondaryLanguageValue { get; set; }

        [IgnoreDataMember]
        public StreetNameLanguage? SecondaryLanguage =>
            string.IsNullOrEmpty(SecondaryLanguageValue)
                ? (StreetNameLanguage?)null
                : Enum.Parse<StreetNameLanguage>(SecondaryLanguageValue);
    }

    [DataContract(Name = "StreetNameSecondaryLanguageWasCorrectedToCleared", Namespace = "")]
    public class StreetNameSecondaryLanguageWasCorrectedToCleared : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "MunicipalityId", Order = 2)]
        public Guid MunicipalityId { get; set; }
    }

    [DataContract(Name = "StreetNameSecondaryLanguageWasDefined", Namespace = "")]
    public class StreetNameSecondaryLanguageWasDefined : IMessage
    {
        [DataMember(Name = "StreetNameId", Order = 1)]
        public Guid StreetNameId { get; set; }

        [DataMember(Name = "MunicipalityId", Order = 2)]
        public Guid MunicipalityId { get; set; }

        [DataMember(Name = "SecondaryLanguage", Order = 3)]
        public string SecondaryLanguageValue { get; set; }

        [IgnoreDataMember]
        public StreetNameLanguage? SecondaryLanguage =>
            string.IsNullOrEmpty(SecondaryLanguageValue)
                ? (StreetNameLanguage?)null
                : Enum.Parse<StreetNameLanguage>(SecondaryLanguageValue);
    }


    public enum StreetNameLanguage
    {
        Dutch = 0,
        French = 1,
        German = 2,
        English = 3
    }
}
