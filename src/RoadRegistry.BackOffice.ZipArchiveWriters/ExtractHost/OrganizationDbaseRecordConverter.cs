namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.Text;
using BackOffice.Extensions;
using Core;
using Dbase;
using Microsoft.IO;

public class OrganizationDbaseRecordConverter : VersionedDbaseRecordReader<Organization>
{
    public OrganizationDbaseRecordConverter(RecyclableMemoryStreamManager manager, Encoding encoding)
        : base(
            new V2Converter(manager, encoding),
            new V1Converter(manager, encoding)
        )
    {
    }

    private sealed class V2Converter : DbaseRecordReader<Extracts.Dbase.Organizations.V2.OrganizationDbaseRecord, Organization>
    {
        public V2Converter(RecyclableMemoryStreamManager manager, Encoding encoding)
            : base(manager, encoding, WellKnownDbaseSchemaVersions.V2)
        {
        }

        protected override Organization Convert(Extracts.Dbase.Organizations.V2.OrganizationDbaseRecord dbaseRecord)
        {
            return Organization.Factory()
                .Create(
                    new OrganizationId(dbaseRecord.ORG.Value),
                    new OrganizationName(dbaseRecord.LBLORG.Value),
                    OrganizationOvoCode.FromValue(dbaseRecord.OVOCODE.GetValue())
                );
        }
    }

    private sealed class V1Converter : DbaseRecordReader<Extracts.Dbase.Organizations.V1.OrganizationDbaseRecord, Organization>
    {
        public V1Converter(RecyclableMemoryStreamManager manager, Encoding encoding)
            : base(manager, encoding, WellKnownDbaseSchemaVersions.V1)
        {
        }

        protected override Organization Convert(Extracts.Dbase.Organizations.V1.OrganizationDbaseRecord dbaseRecord)
        {
            return Organization.Factory()
                .Create(
                    new OrganizationId(dbaseRecord.ORG.Value),
                    new OrganizationName(dbaseRecord.LBLORG.Value),
                    null
                );
        }
    }
}
