namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.Text;
using Abstractions.Organizations;
using BackOffice.Extensions;
using Dbase;
using Microsoft.IO;

public class OrganizationDbaseRecordReader : VersionedDbaseRecordReader<OrganizationDetail>
{
    public OrganizationDbaseRecordReader(RecyclableMemoryStreamManager manager, Encoding encoding)
        : base(
            new V2Converter(manager, encoding),
            new V1Converter(manager, encoding)
        )
    {
    }

    private sealed class V2Converter : DbaseRecordReader<Extracts.Dbase.Organizations.V2.OrganizationDbaseRecord, OrganizationDetail>
    {
        public V2Converter(RecyclableMemoryStreamManager manager, Encoding encoding)
            : base(manager, encoding, WellKnownDbaseSchemaVersions.V2)
        {
        }

        protected override OrganizationDetail Convert(Extracts.Dbase.Organizations.V2.OrganizationDbaseRecord dbaseRecord)
        {
            return new OrganizationDetail
            {
                Code = new OrganizationId(dbaseRecord.ORG.Value),
                Name = new OrganizationName(dbaseRecord.LBLORG.Value),
                OvoCode = OrganizationOvoCode.FromValue(dbaseRecord.OVOCODE.GetValue())
            };
        }
    }

    private sealed class V1Converter : DbaseRecordReader<Extracts.Dbase.Organizations.V1.OrganizationDbaseRecord, OrganizationDetail>
    {
        public V1Converter(RecyclableMemoryStreamManager manager, Encoding encoding)
            : base(manager, encoding, WellKnownDbaseSchemaVersions.V1)
        {
        }

        protected override OrganizationDetail Convert(Extracts.Dbase.Organizations.V1.OrganizationDbaseRecord dbaseRecord)
        {
            return new OrganizationDetail
            {
                Code = new OrganizationId(dbaseRecord.ORG.Value),
                Name = new OrganizationName(dbaseRecord.LBLORG.Value),
                OvoCode = null
            };
        }
    }
}
