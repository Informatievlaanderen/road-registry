namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeDynamicAttributes;

using System.Collections.Generic;
using System.Runtime.Serialization;
using Swashbuckle.AspNetCore.Filters;

[DataContract(Name = "WegsegmentenDynamischeAttributenWijzigen", Namespace = "")]
public class ChangeRoadSegmentsDynamicAttributesParameters : List<ChangeRoadSegmentDynamicAttributesParameters>
{
}

public class ChangeRoadSegmentsDynamicAttributesParametersExamples : IExamplesProvider<ChangeRoadSegmentsDynamicAttributesParameters>
{
    public ChangeRoadSegmentsDynamicAttributesParameters GetExamples()
    {
        return new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new()
            {
                WegsegmentId = 481110,
                Wegverharding = new ChangeSurfaceAttributeParameters []
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 151.45M,
                        Type = RoadSegmentSurfaceType.SolidSurface.ToDutchString()
                    }
                },
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 151.45M,
                        Aantal = "2",
                        Richting = RoadSegmentLaneDirection.Forward.ToDutchString()
                    }
                }
            },
            new()
            {
                WegsegmentId = 481111,
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new ()
                    {
                        VanPositie = 0,
                        TotPositie = 30.60M,
                        Breedte = "3"
                    },
                    new ()
                    {
                        VanPositie = 30.60M,
                        TotPositie = 302.13M,
                        Breedte = "5"
                    }
                }
            }
        };
    }
}
