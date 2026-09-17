namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutlineV2;

using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;

// The request body is bound with Newtonsoft; a body that does not fit the contract makes the model state invalid, which
// the API answers with a 400 'JsonInvalid' before the validator or the controller ever see it.
public class WhenCreateOutlineV2RequestIsDeserialized
{
    [Theory]
    [InlineData("morfologie")]
    [InlineData("wegverharding")]
    [InlineData("toegang")]
    [InlineData("straatnaam")]
    [InlineData("wegbeheerder")]
    [InlineData("wegcategorie")]
    [InlineData("verkeerstypeAuto")]
    [InlineData("verkeerstypeFiets")]
    [InlineData("verkeerstypeVoetganger")]
    public void WhenVanPositieAndTotPositieAreNull_ThenTheRequestIsAccepted(string attribute)
    {
        var json = ValidJson();
        var item = (JObject)GetProperty(json, attribute).Value.First!;
        item["vanPositie"] = JValue.CreateNull();
        item["totPositie"] = JValue.CreateNull();

        var parameters = Deserialize(json);

        Assert.NotNull(parameters);
    }

    [Theory]
    [InlineData("morfologie")]
    [InlineData("wegverharding")]
    [InlineData("toegang")]
    [InlineData("straatnaam")]
    [InlineData("wegbeheerder")]
    [InlineData("wegcategorie")]
    [InlineData("verkeerstypeAuto")]
    [InlineData("verkeerstypeFiets")]
    [InlineData("verkeerstypeVoetganger")]
    public void WhenVanPositieAndTotPositieAreLeftOut_ThenTheRequestIsAccepted(string attribute)
    {
        var json = ValidJson();
        var item = (JObject)GetProperty(json, attribute).Value.First!;
        item.Remove("vanPositie");
        item.Remove("totPositie");

        var parameters = Deserialize(json);

        Assert.NotNull(parameters);
    }

    [Theory]
    [InlineData("morfologie", "morfologie")]
    [InlineData("wegverharding", "wegverharding")]
    [InlineData("toegang", "toegang")]
    [InlineData("straatnaam", "kant")]
    [InlineData("straatnaam", "identificator")]
    [InlineData("wegbeheerder", "kant")]
    [InlineData("wegbeheerder", "wegbeheerder")]
    [InlineData("wegcategorie", "wegcategorie")]
    [InlineData("verkeerstypeAuto", "richting")]
    [InlineData("verkeerstypeFiets", "richting")]
    [InlineData("verkeerstypeVoetganger", "richting")]
    public void WhenASubParameterOtherThanThePositionsIsMissing_ThenTheJsonIsInvalid(string attribute, string subParameter)
    {
        var json = ValidJson();
        var item = (JObject)GetProperty(json, attribute).Value.First!;
        Assert.True(item.Remove(subParameter), $"'{subParameter}' is not part of '{attribute}'");

        Assert.Throws<JsonSerializationException>(() => Deserialize(json));
    }

    private static JObject ValidJson() => JObject.Parse(JsonConvert.SerializeObject(CreateOutlineV2Parameters.Valid()));

    private static JProperty GetProperty(JObject json, string name) =>
        json.Properties().Single(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

    private static CreateOutlinedRoadSegmentV2Parameters Deserialize(JObject json) =>
        JsonConvert.DeserializeObject<CreateOutlinedRoadSegmentV2Parameters>(json.ToString());
}
