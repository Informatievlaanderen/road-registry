namespace RoadRegistry.Tests.BackOffice.Extracts
{
    using RoadRegistry.BackOffice.Extracts;
    using RoadRegistry.BackOffice.FeatureCompare;

    public class ExtractFileNameTests
    {
        [Fact]
        public void AllExtractFileNamesShouldHaveAnExpectedFileName()
        {
            var expectedDbaseNames = new Dictionary<ExtractFileName, string>
            {
                { ExtractFileName.Transactiezones, "Transactiezones.dbf" },
                { ExtractFileName.Wegknoop, "Wegknoop.dbf" },
                { ExtractFileName.Wegsegment, "Wegsegment.dbf" },
                { ExtractFileName.AttRijstroken, "AttRijstroken.dbf" },
                { ExtractFileName.AttWegbreedte, "AttWegbreedte.dbf" },
                { ExtractFileName.AttWegverharding, "AttWegverharding.dbf" },
                { ExtractFileName.AttEuropweg, "AttEuropweg.dbf" },
                { ExtractFileName.AttNationweg, "AttNationweg.dbf" },
                { ExtractFileName.AttGenumweg, "AttGenumweg.dbf" },
                { ExtractFileName.RltOgkruising, "RltOgkruising.dbf" }
            };

            var extractFileNameNames = Enum.GetNames<ExtractFileName>();
            foreach (var extractFileNameName in extractFileNameNames)
            {
                var extractFileName = Enum.Parse<ExtractFileName>(extractFileNameName);

                if (expectedDbaseNames.TryGetValue(extractFileName, out var expectedDbaseFileName))
                {
                    Assert.Equal(expectedDbaseFileName, extractFileName.ToDbaseFileName());
                }
                else
                {
                    throw new Exception($"No expected filename defined for {nameof(ExtractFileName)}.{extractFileNameName}");
                }
            }
        }

        [Theory]
        [InlineData(FeatureType.Change, "")]
        [InlineData(FeatureType.Extract, "e")]
        [InlineData(FeatureType.Integration, "i")]
        public void FeatureTypeShouldProduceExpectedFileName(FeatureType featureType, string expectedPrefix)
        {
            var expectedDbaseNames = new Dictionary<ExtractFileName, string>
            {
                { ExtractFileName.Transactiezones, $"{expectedPrefix}Transactiezones.dbf" },
                { ExtractFileName.Wegknoop, $"{expectedPrefix}Wegknoop.dbf" },
                { ExtractFileName.Wegsegment, $"{expectedPrefix}Wegsegment.dbf" },
                { ExtractFileName.AttRijstroken, $"{expectedPrefix}AttRijstroken.dbf" },
                { ExtractFileName.AttWegbreedte, $"{expectedPrefix}AttWegbreedte.dbf" },
                { ExtractFileName.AttWegverharding, $"{expectedPrefix}AttWegverharding.dbf" },
                { ExtractFileName.AttEuropweg, $"{expectedPrefix}AttEuropweg.dbf" },
                { ExtractFileName.AttNationweg, $"{expectedPrefix}AttNationweg.dbf" },
                { ExtractFileName.AttGenumweg, $"{expectedPrefix}AttGenumweg.dbf" },
                { ExtractFileName.RltOgkruising, $"{expectedPrefix}RltOgkruising.dbf" }
            };

            var extractFileNameNames = Enum.GetNames<ExtractFileName>();
            foreach (var extractFileNameName in extractFileNameNames)
            {
                var extractFileName = Enum.Parse<ExtractFileName>(extractFileNameName);

                if (expectedDbaseNames.TryGetValue(extractFileName, out var expectedDbaseFileName))
                {
                    Assert.Equal(expectedDbaseFileName, featureType.ToDbaseFileName(extractFileName));
                }
            }
        }
    }
}
