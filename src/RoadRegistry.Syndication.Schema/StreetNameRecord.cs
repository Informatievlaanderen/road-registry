namespace RoadRegistry.Syndication.Schema
{
    using System;

    public class StreetNameRecord
    {
        public Guid StreetNameId { get; set; }
        public int? PersistentLocalId { get; set; }
        public Guid MunicipalityId { get; set; }
        public string NisCode { get; set; }
        public string Name { get; set; }
        public string DutchName { get; set; }
        public string FrenchName { get; set; }
        public string GermanName { get; set; }
        public string EnglishName { get; set; }
        public StreetNameStatus? StreetNameStatus { get; set; }
    }
}
