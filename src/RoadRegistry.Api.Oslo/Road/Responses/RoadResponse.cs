namespace RoadRegistry.Api.Oslo.Road.Responses
{
    using System.Collections.Generic;
    using System.Linq;
    using Aiv.Vbr.Api.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Projections.Oslo.Road;
    using Swashbuckle.AspNetCore.Examples;
    using ValueObjects;

    public class RoadResponse
    {
        /// <summary>Gekende namen van de weg.</summary>
        public List<RoadLocalizedName> Namen { get; }

        public RoadResponse(Road road)
        {
            Namen = new List<RoadLocalizedName>
            {
                new RoadLocalizedName(Language.Dutch, road.NameDutch),
                new RoadLocalizedName(Language.French, road.NameFrench),
                new RoadLocalizedName(Language.German, road.NameGerman),
                new RoadLocalizedName(Language.English, road.NameEnglish),
            }.Where(x => !string.IsNullOrWhiteSpace(x.Naam)).ToList();
        }
    }

    public class RoadLocalizedName
    {
        /// <summary>Taal van de wegnaam.</summary>
        public Language Taal { get; }

        /// <summary>De wegnaam.</summary>
        public string Naam { get; }

        public RoadLocalizedName(Language language, string name)
        {
            Taal = language;
            Naam = name;
        }
    }

    public class RoadResponseExamples : IExamplesProvider
    {
        public object GetExamples()
        {
            return new RoadResponse(
                new Road
                {
                    NameDutch = "Brugge",
                    NameEnglish = "Bruges",
                    NameFrench = "Bruges",
                });
        }
    }

    public class RoadNotFoundResponseExamples : IExamplesProvider
    {
        public object GetExamples()
        {
            return new BasicApiProblem
            {
                HttpStatus = StatusCodes.Status404NotFound,
                Title = BasicApiProblem.DefaultTitle,
                Detail = "Onbestaande weg.",
                ProblemInstanceUri = BasicApiProblem.GetProblemNumber()
            };
        }
    }
}
