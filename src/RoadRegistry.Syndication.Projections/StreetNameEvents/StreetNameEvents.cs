namespace RoadRegistry.Syndication.Projections.StreetNameEvents;

using System;
using System.Runtime.Serialization;
using NodaTime;

[DataContract(Name = "StreetNameWasRegistered", Namespace = "")]
public class StreetNameWasRegistered
{
    [DataMember(Name = "MunicipalityId", Order = 2)]
    public Guid MunicipalityId { get; set; }

    [DataMember(Name = "NisCode", Order = 3)]
    public string NisCode { get; set; }

    [DataMember(Name = "StreetNameId", Order = 1)]
    public Guid StreetNameId { get; set; }
}

[DataContract(Name = "StreetNamePersistentLocalIdentifierWasAssigned", Namespace = "")]
public class StreetNamePersistentLocalIdentifierWasAssigned
{
    [DataMember(Name = "AssignmentDate", Order = 3)]
    public Instant AssignmentDate { get; set; }

    [DataMember(Name = "PersistentLocalId", Order = 2)]
    public int PersistentLocalId { get; set; }

    [DataMember(Name = "StreetNameId", Order = 1)]
    public Guid StreetNameId { get; set; }
}

[DataContract(Name = "StreetNameWasNamed", Namespace = "")]
public class StreetNameWasNamed
{
    [IgnoreDataMember]
    public StreetNameLanguage? Language =>
        string.IsNullOrEmpty(LanguageValue)
            ? null
            : Enum.Parse<StreetNameLanguage>(LanguageValue);

    [DataMember(Name = "Language", Order = 3)]
    public string LanguageValue { get; set; }

    [DataMember(Name = "Name", Order = 2)] public string Name { get; set; }

    [DataMember(Name = "StreetNameId", Order = 1)]
    public Guid StreetNameId { get; set; }
}

[DataContract(Name = "StreetNameNameWasCleared", Namespace = "")]
public class StreetNameNameWasCleared
{
    [IgnoreDataMember]
    public StreetNameLanguage? Language =>
        string.IsNullOrEmpty(LanguageValue)
            ? null
            : Enum.Parse<StreetNameLanguage>(LanguageValue);

    [DataMember(Name = "Language", Order = 2)]
    public string LanguageValue { get; set; }

    [DataMember(Name = "StreetNameId", Order = 1)]
    public Guid StreetNameId { get; set; }
}

[DataContract(Name = "StreetNameNameWasCorrected", Namespace = "")]
public class StreetNameNameWasCorrected
{
    [IgnoreDataMember]
    public StreetNameLanguage? Language =>
        string.IsNullOrEmpty(LanguageValue)
            ? null
            : Enum.Parse<StreetNameLanguage>(LanguageValue);

    [DataMember(Name = "Language", Order = 3)]
    public string LanguageValue { get; set; }

    [DataMember(Name = "Name", Order = 2)] public string Name { get; set; }

    [DataMember(Name = "StreetNameId", Order = 1)]
    public Guid StreetNameId { get; set; }
}

[DataContract(Name = "StreetNameNameWasCorrectedToCleared", Namespace = "")]
public class StreetNameNameWasCorrectedToCleared
{
    [IgnoreDataMember]
    public StreetNameLanguage? Language =>
        string.IsNullOrEmpty(LanguageValue)
            ? null
            : Enum.Parse<StreetNameLanguage>(LanguageValue);

    [DataMember(Name = "Language", Order = 2)]
    public string LanguageValue { get; set; }

    [DataMember(Name = "StreetNameId", Order = 1)]
    public Guid StreetNameId { get; set; }
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

[DataContract(Name = "StreetNameHomonymAdditionWasCleared", Namespace = "")]
public class StreetNameHomonymAdditionWasCleared
{
    [IgnoreDataMember]
    public StreetNameLanguage? Language =>
        string.IsNullOrEmpty(LanguageValue)
            ? null
            : Enum.Parse<StreetNameLanguage>(LanguageValue);

    [DataMember(Name = "Language", Order = 2)]
    public string LanguageValue { get; set; }

    [DataMember(Name = "StreetNameId", Order = 1)]
    public Guid StreetNameId { get; set; }
}

[DataContract(Name = "StreetNameHomonymAdditionWasCorrected", Namespace = "")]
public class StreetNameHomonymAdditionWasCorrected
{
    [DataMember(Name = "HomonymAddition", Order = 2)]
    public string HomonymAddition { get; set; }

    [IgnoreDataMember]
    public StreetNameLanguage? Language =>
        string.IsNullOrEmpty(LanguageValue)
            ? null
            : Enum.Parse<StreetNameLanguage>(LanguageValue);

    [DataMember(Name = "Language", Order = 3)]
    public string LanguageValue { get; set; }

    [DataMember(Name = "StreetNameId", Order = 1)]
    public Guid StreetNameId { get; set; }
}

[DataContract(Name = "StreetNameHomonymAdditionWasCorrectedToCleared", Namespace = "")]
public class StreetNameHomonymAdditionWasCorrectedToCleared
{
    [IgnoreDataMember]
    public StreetNameLanguage? Language =>
        string.IsNullOrEmpty(LanguageValue)
            ? null
            : Enum.Parse<StreetNameLanguage>(LanguageValue);

    [DataMember(Name = "Language", Order = 2)]
    public string LanguageValue { get; set; }

    [DataMember(Name = "StreetNameId", Order = 1)]
    public Guid StreetNameId { get; set; }
}

[DataContract(Name = "StreetNameHomonymAdditionWasDefined", Namespace = "")]
public class StreetNameHomonymAdditionWasDefined
{
    [DataMember(Name = "HomonymAddition", Order = 2)]
    public string HomonymAddition { get; set; }

    [IgnoreDataMember]
    public StreetNameLanguage? Language =>
        string.IsNullOrEmpty(LanguageValue)
            ? null
            : Enum.Parse<StreetNameLanguage>(LanguageValue);

    [DataMember(Name = "Language", Order = 3)]
    public string LanguageValue { get; set; }

    [DataMember(Name = "StreetNameId", Order = 1)]
    public Guid StreetNameId { get; set; }
}

[DataContract(Name = "StreetNamePrimaryLanguageWasCleared", Namespace = "")]
public class StreetNamePrimaryLanguageWasCleared
{
    [DataMember(Name = "MunicipalityId", Order = 2)]
    public Guid MunicipalityId { get; set; }

    [DataMember(Name = "StreetNameId", Order = 1)]
    public Guid StreetNameId { get; set; }
}

[DataContract(Name = "StreetNamePrimaryLanguageWasCorrected", Namespace = "")]
public class StreetNamePrimaryLanguageWasCorrected
{
    [DataMember(Name = "MunicipalityId", Order = 2)]
    public Guid MunicipalityId { get; set; }

    [IgnoreDataMember]
    public StreetNameLanguage? PrimaryLanguage =>
        string.IsNullOrEmpty(PrimaryLanguageValue)
            ? null
            : Enum.Parse<StreetNameLanguage>(PrimaryLanguageValue);

    [DataMember(Name = "PrimaryLanguage", Order = 3)]
    public string PrimaryLanguageValue { get; set; }

    [DataMember(Name = "StreetNameId", Order = 1)]
    public Guid StreetNameId { get; set; }
}

[DataContract(Name = "StreetNamePrimaryLanguageWasCorrectedToCleared", Namespace = "")]
public class StreetNamePrimaryLanguageWasCorrectedToCleared
{
    [DataMember(Name = "MunicipalityId", Order = 2)]
    public Guid MunicipalityId { get; set; }

    [DataMember(Name = "StreetNameId", Order = 1)]
    public Guid StreetNameId { get; set; }
}

[DataContract(Name = "StreetNamePrimaryLanguageWasDefined", Namespace = "")]
public class StreetNamePrimaryLanguageWasDefined
{
    [DataMember(Name = "MunicipalityId", Order = 2)]
    public Guid MunicipalityId { get; set; }

    [IgnoreDataMember]
    public StreetNameLanguage? PrimaryLanguage =>
        string.IsNullOrEmpty(PrimaryLanguageValue)
            ? null
            : Enum.Parse<StreetNameLanguage>(PrimaryLanguageValue);

    [DataMember(Name = "PrimaryLanguage", Order = 3)]
    public string PrimaryLanguageValue { get; set; }

    [DataMember(Name = "StreetNameId", Order = 1)]
    public Guid StreetNameId { get; set; }
}

[DataContract(Name = "StreetNamePrimaryLanguageWasCleared", Namespace = "")]
public class StreetNameSecondaryLanguageWasCleared
{
    [DataMember(Name = "MunicipalityId", Order = 2)]
    public Guid MunicipalityId { get; set; }

    [DataMember(Name = "StreetNameId", Order = 1)]
    public Guid StreetNameId { get; set; }
}

[DataContract(Name = "StreetNameSecondaryLanguageWasCorrected", Namespace = "")]
public class StreetNameSecondaryLanguageWasCorrected
{
    [DataMember(Name = "MunicipalityId", Order = 2)]
    public Guid MunicipalityId { get; set; }

    [IgnoreDataMember]
    public StreetNameLanguage? SecondaryLanguage =>
        string.IsNullOrEmpty(SecondaryLanguageValue)
            ? null
            : Enum.Parse<StreetNameLanguage>(SecondaryLanguageValue);

    [DataMember(Name = "SecondaryLanguage", Order = 3)]
    public string SecondaryLanguageValue { get; set; }

    [DataMember(Name = "StreetNameId", Order = 1)]
    public Guid StreetNameId { get; set; }
}

[DataContract(Name = "StreetNameSecondaryLanguageWasCorrectedToCleared", Namespace = "")]
public class StreetNameSecondaryLanguageWasCorrectedToCleared
{
    [DataMember(Name = "MunicipalityId", Order = 2)]
    public Guid MunicipalityId { get; set; }

    [DataMember(Name = "StreetNameId", Order = 1)]
    public Guid StreetNameId { get; set; }
}

[DataContract(Name = "StreetNameSecondaryLanguageWasDefined", Namespace = "")]
public class StreetNameSecondaryLanguageWasDefined
{
    [DataMember(Name = "MunicipalityId", Order = 2)]
    public Guid MunicipalityId { get; set; }

    [IgnoreDataMember]
    public StreetNameLanguage? SecondaryLanguage =>
        string.IsNullOrEmpty(SecondaryLanguageValue)
            ? null
            : Enum.Parse<StreetNameLanguage>(SecondaryLanguageValue);

    [DataMember(Name = "SecondaryLanguage", Order = 3)]
    public string SecondaryLanguageValue { get; set; }

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