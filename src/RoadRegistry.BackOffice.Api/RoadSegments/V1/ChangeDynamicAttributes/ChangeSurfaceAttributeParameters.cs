namespace RoadRegistry.BackOffice.Api.RoadSegments.V1.ChangeDynamicAttributes;

using System.Runtime.Serialization;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Api.Infrastructure;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

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
