namespace RoadRegistry.Editor.Projections.DutchTranslations;

using System;
using System.Linq;
using BackOffice.Core;

public static class ProblemWithDownload
{
    public static readonly Converter<Problem, string> Translator =
        problem =>
        {
            return problem.Reason switch
            {
                nameof(ShapeFileInvalidHeader) => $"Kan header van de shape file niet lezen: '{problem.Parameters.ElementAt(0).Value}'",
                nameof(ShapeFileInvalidPolygonShellOrientation) => "De orientatie van de polygoon moet in wijzerzin zijn.",
                nameof(ShapeFileGeometryTypeMustBePolygon) => "Geometrie type moet een polygoon of multi polygoon zijn.",
                nameof(ShapeFileGeometrySridMustBeEqual) => "SRID van alle geometrieën moeten dezelfde zijn.",
                nameof(ShapeFileHasNoValidPolygons) => "Het shape bestand bevat geen geldige polygonen.",
                _ => $"'{problem.Reason}' has no translation. Please fix it."
            };
        };
}
