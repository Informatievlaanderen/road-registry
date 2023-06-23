namespace RoadRegistry.BackOffice.Api.RoadSegments.Change;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Infrastructure.Controllers.Attributes;
using Newtonsoft.Json;

public class ChangeSurfaceAttributeParameters : ChangePositionAttributeParameters
{
    /// <summary>
    ///     Type wegverharding van het wegsegment.
    /// </summary>
    [DataMember(Name = "Type", Order = 3)]
    [JsonProperty("type", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentSurfaceType))]
    public string Type { get; set; }
}
