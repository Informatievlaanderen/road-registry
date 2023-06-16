namespace RoadRegistry.BackOffice.FeatureCompare;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Uploads;

[Serializable]
public sealed class DbaseReaderNotFoundException : ApplicationException
{
    public DbaseSchema ActualSchema { get; }
    public ICollection<DbaseSchema> ExpectedSchemas { get; }

    public DbaseReaderNotFoundException(string fileName, DbaseSchema dbaseSchema, ICollection<DbaseSchema> supportedDbaseSchemas)
        : base($"No reader found for file '{fileName}' with schema:\n{dbaseSchema.Describe()}\nOnly the following schemas are supported:\n{string.Join("\n", supportedDbaseSchemas.Select(x => x.Describe()))}")
    {
        ActualSchema = dbaseSchema;
        ExpectedSchemas = supportedDbaseSchemas;
    }

    private DbaseReaderNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
