namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using Extracts;
using FeatureCompare;
using FeatureCompare.Translators;

public class GradeSeparatedJunctionZipArchiveValidator : FeatureReaderZipArchiveValidator<GradeSeparatedJunctionFeatureCompareAttributes>
{
    public GradeSeparatedJunctionZipArchiveValidator(FileEncoding encoding)
        : base(ExtractFileName.RltOgkruising, new[] { FeatureType.Extract, FeatureType.Change },
            new GradeSeparatedJunctionFeatureCompareFeatureReader(encoding))
    {
    }
}
