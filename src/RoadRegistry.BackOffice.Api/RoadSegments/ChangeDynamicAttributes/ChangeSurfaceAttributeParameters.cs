namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeDynamicAttributes;

using Infrastructure.Controllers.Attributes;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Api.Infrastructure;
using System.Runtime.Serialization;

[DataContract(Name = "WegsegmentWegverhardingWijzigen", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentWegverhardingWijzigen")]
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
