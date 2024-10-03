namespace RoadRegistry.Editor.Schema.Organizations;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OrganizationRecordV2Configuration : IEntityTypeConfiguration<OrganizationRecordV2>
{
    private const string TableName = "OrganizationV2";

    public void Configure(EntityTypeBuilder<OrganizationRecordV2> b)
    {
        b.ToTable(TableName, WellKnownSchemas.EditorSchema)
            .HasIndex(p => p.Id)
            .IsClustered(false);

        b.Property(p => p.Id).ValueGeneratedOnAdd().IsRequired();
        b.Property(p => p.Code).IsRequired();
        b.Property(p => p.Name).IsRequired();
        b.Property(p => p.OvoCode);
        b.Property(p => p.KboNumber);
        b.Property(p => p.IsMaintainer);

        b.HasIndex(p => p.Code);
        b.HasIndex(p => p.OvoCode);
        b.HasIndex(p => p.KboNumber);
        b.HasIndex(p => p.IsMaintainer);
    }
}
