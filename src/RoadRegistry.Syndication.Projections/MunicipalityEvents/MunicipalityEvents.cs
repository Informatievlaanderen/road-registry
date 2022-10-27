namespace RoadRegistry.Syndication.Projections.MunicipalityEvents;

using System;
using System.Runtime.Serialization;
using Be.Vlaanderen.Basisregisters.EventHandling;

[DataContract(Name = "MunicipalityWasRegistered", Namespace = "")]
public class MunicipalityWasRegistered : IMessage
{
    [DataMember(Name = "MunicipalityId", Order = 1)]
    public Guid MunicipalityId { get; set; }

    [DataMember(Name = "NisCode", Order = 2)]
    public string NisCode { get; set; }
}

[DataContract(Name = "MunicipalityWasNamed", Namespace = "")]
public class MunicipalityWasNamed : IMessage
{
    [DataMember(Name = "Language", Order = 3)]
    public MunicipalityLanguage Language { get; set; }

    [DataMember(Name = "MunicipalityId", Order = 1)]
    public Guid MunicipalityId { get; set; }

    [DataMember(Name = "Name", Order = 2)] public string Name { get; set; }
}

[DataContract(Name = "MunicipalityNameWasCleared", Namespace = "")]
public class MunicipalityNameWasCleared : IMessage
{
    [DataMember(Name = "Language", Order = 2)]
    public MunicipalityLanguage Language { get; set; }

    [DataMember(Name = "MunicipalityId", Order = 1)]
    public Guid MunicipalityId { get; set; }
}

[DataContract(Name = "MunicipalityNameWasCorrected", Namespace = "")]
public class MunicipalityNameWasCorrected : IMessage
{
    [DataMember(Name = "Language", Order = 3)]
    public MunicipalityLanguage Language { get; set; }

    [DataMember(Name = "MunicipalityId", Order = 1)]
    public Guid MunicipalityId { get; set; }

    [DataMember(Name = "Name", Order = 2)] public string Name { get; set; }
}

[DataContract(Name = "MunicipalityNameWasCorrectedToCleared", Namespace = "")]
public class MunicipalityNameWasCorrectedToCleared : IMessage
{
    [DataMember(Name = "Language", Order = 2)]
    public MunicipalityLanguage Language { get; set; }

    [DataMember(Name = "MunicipalityId", Order = 1)]
    public Guid MunicipalityId { get; set; }
}

[DataContract(Name = "MunicipalityNisCodeWasDefined", Namespace = "")]
public class MunicipalityNisCodeWasDefined : IMessage
{
    [DataMember(Name = "MunicipalityId", Order = 1)]
    public Guid MunicipalityId { get; set; }

    [DataMember(Name = "NisCode", Order = 2)]
    public string NisCode { get; set; }
}

[DataContract(Name = "MunicipalityNisCodeWasCorrected", Namespace = "")]
public class MunicipalityNisCodeWasCorrected : IMessage
{
    [DataMember(Name = "MunicipalityId", Order = 1)]
    public Guid MunicipalityId { get; set; }

    [DataMember(Name = "NisCode", Order = 2)]
    public string NisCode { get; set; }
}

[DataContract(Name = "MunicipalityBecameCurrent", Namespace = "")]
public class MunicipalityBecameCurrent : IMessage
{
    [DataMember(Name = "MunicipalityId", Order = 1)]
    public Guid MunicipalityId { get; set; }
}

[DataContract(Name = "MunicipalityWasCorrectedToCurrent", Namespace = "")]
public class MunicipalityWasCorrectedToCurrent : IMessage
{
    [DataMember(Name = "MunicipalityId", Order = 1)]
    public Guid MunicipalityId { get; set; }
}

[DataContract(Name = "MunicipalityWasRetired", Namespace = "")]
public class MunicipalityWasRetired : IMessage
{
    [DataMember(Name = "MunicipalityId", Order = 1)]
    public Guid MunicipalityId { get; set; }
}

[DataContract(Name = "MunicipalityWasCorrectedToRetired", Namespace = "")]
public class MunicipalityWasCorrectedToRetired : IMessage
{
    [DataMember(Name = "MunicipalityId", Order = 1)]
    public Guid MunicipalityId { get; set; }
}

[DataContract(Name = "MunicipalityOfficialLanguageWasAdded", Namespace = "")]
public class MunicipalityOfficialLanguageWasAdded : IMessage
{
}

[DataContract(Name = "MunicipalityFacilityLanguageWasAdded", Namespace = "")]
public class MunicipalityFacilityLanguageWasAdded : IMessage
{
}

public enum MunicipalityLanguage
{
    Dutch = 0,
    French = 1,
    German = 2,
    English = 3
}
