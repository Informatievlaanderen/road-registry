namespace RoadRegistry.Pbs.Schema.Records;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// Code lists: dumps of the known V2 domain types (code + Dutch label + definition). Kept in sync by a one-time
// runner, except WegsegmentCodelijstWegbeheerder which is fed by the organization projection.

public class RoadNodeTypeCodeListRecord
{
    public int TYPE { get; set; }
    public string LBLTYPE { get; set; }
    public string DEFTYPE { get; set; }
}

public class RoadNodeTypeCodeListRecordConfiguration : IEntityTypeConfiguration<RoadNodeTypeCodeListRecord>
{
    public const string TableName = "WegknoopCodelijstType";

    public void Configure(EntityTypeBuilder<RoadNodeTypeCodeListRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.PbsSchema).HasKey(p => p.TYPE);
        b.Property(p => p.TYPE).ValueGeneratedNever();
        b.Property(p => p.LBLTYPE).HasColumnType("varchar(64)");
        b.Property(p => p.DEFTYPE).HasColumnType("varchar(254)");
    }
}

public class GradeSeparatedJunctionTypeCodeListRecord
{
    public int TYPE { get; set; }
    public string LBLTYPE { get; set; }
    public string DEFTYPE { get; set; }
}

public class GradeSeparatedJunctionTypeCodeListRecordConfiguration : IEntityTypeConfiguration<GradeSeparatedJunctionTypeCodeListRecord>
{
    public const string TableName = "OngelijkgrondseKruisingCodelijstType";

    public void Configure(EntityTypeBuilder<GradeSeparatedJunctionTypeCodeListRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.PbsSchema).HasKey(p => p.TYPE);
        b.Property(p => p.TYPE).ValueGeneratedNever();
        b.Property(p => p.LBLTYPE).HasColumnType("varchar(64)");
        b.Property(p => p.DEFTYPE).HasColumnType("varchar(254)");
    }
}

public class RoadSegmentSideCodeListRecord
{
    public int KANT { get; set; }
    public string LBLKANT { get; set; }
    public string DEFKANT { get; set; }
}

public class RoadSegmentSideCodeListRecordConfiguration : IEntityTypeConfiguration<RoadSegmentSideCodeListRecord>
{
    public const string TableName = "WegsegmentCodelijstKant";

    public void Configure(EntityTypeBuilder<RoadSegmentSideCodeListRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.PbsSchema).HasKey(p => p.KANT);
        b.Property(p => p.KANT).ValueGeneratedNever();
        b.Property(p => p.LBLKANT).HasColumnType("varchar(64)");
        b.Property(p => p.DEFKANT).HasColumnType("varchar(254)");
    }
}

public class RoadSegmentMethodCodeListRecord
{
    public int METHODE { get; set; }
    public string LBLMETHODE { get; set; }
    public string DEFMETHODE { get; set; }
}

public class RoadSegmentMethodCodeListRecordConfiguration : IEntityTypeConfiguration<RoadSegmentMethodCodeListRecord>
{
    public const string TableName = "WegsegmentCodelijstMethode";

    public void Configure(EntityTypeBuilder<RoadSegmentMethodCodeListRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.PbsSchema).HasKey(p => p.METHODE);
        b.Property(p => p.METHODE).ValueGeneratedNever();
        b.Property(p => p.LBLMETHODE).HasColumnType("varchar(64)");
        b.Property(p => p.DEFMETHODE).HasColumnType("varchar(254)");
    }
}

public class RoadSegmentMorphologyCodeListRecord
{
    public int MORF { get; set; }
    public string LBLMORF { get; set; }
    public string DEFMORF { get; set; }
}

public class RoadSegmentMorphologyCodeListRecordConfiguration : IEntityTypeConfiguration<RoadSegmentMorphologyCodeListRecord>
{
    public const string TableName = "WegsegmentCodelijstMorfologie";

    public void Configure(EntityTypeBuilder<RoadSegmentMorphologyCodeListRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.PbsSchema).HasKey(p => p.MORF);
        b.Property(p => p.MORF).ValueGeneratedNever();
        b.Property(p => p.LBLMORF).HasColumnType("varchar(64)");
        b.Property(p => p.DEFMORF).HasColumnType("varchar(254)");
    }
}

public class RoadSegmentDirectionCodeListRecord
{
    public int RICHTING { get; set; }
    public string LBLRICHT { get; set; }
    public string DEFRICHT { get; set; }
}

public class RoadSegmentDirectionCodeListRecordConfiguration : IEntityTypeConfiguration<RoadSegmentDirectionCodeListRecord>
{
    public const string TableName = "WegsegmentCodelijstRichting";

    public void Configure(EntityTypeBuilder<RoadSegmentDirectionCodeListRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.PbsSchema).HasKey(p => p.RICHTING);
        b.Property(p => p.RICHTING).ValueGeneratedNever();
        b.Property(p => p.LBLRICHT).HasColumnType("varchar(64)");
        b.Property(p => p.DEFRICHT).HasColumnType("varchar(254)");
    }
}

public class RoadSegmentStatusCodeListRecord
{
    public int STATUS { get; set; }
    public string LBLSTATUS { get; set; }
    public string DEFSTATUS { get; set; }
}

public class RoadSegmentStatusCodeListRecordConfiguration : IEntityTypeConfiguration<RoadSegmentStatusCodeListRecord>
{
    public const string TableName = "WegsegmentCodelijstStatus";

    public void Configure(EntityTypeBuilder<RoadSegmentStatusCodeListRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.PbsSchema).HasKey(p => p.STATUS);
        b.Property(p => p.STATUS).ValueGeneratedNever();
        b.Property(p => p.LBLSTATUS).HasColumnType("varchar(64)");
        b.Property(p => p.DEFSTATUS).HasColumnType("varchar(254)");
    }
}

public class RoadSegmentAccessRestrictionCodeListRecord
{
    public int TOEGANG { get; set; }
    public string LBLTOEGANG { get; set; }
    public string DEFTOEGANG { get; set; }
}

public class RoadSegmentAccessRestrictionCodeListRecordConfiguration : IEntityTypeConfiguration<RoadSegmentAccessRestrictionCodeListRecord>
{
    public const string TableName = "WegsegmentCodelijstToegang";

    public void Configure(EntityTypeBuilder<RoadSegmentAccessRestrictionCodeListRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.PbsSchema).HasKey(p => p.TOEGANG);
        b.Property(p => p.TOEGANG).ValueGeneratedNever();
        b.Property(p => p.LBLTOEGANG).HasColumnType("varchar(64)");
        b.Property(p => p.DEFTOEGANG).HasColumnType("varchar(254)");
    }
}

public class RoadSegmentSurfaceTypeCodeListRecord
{
    public int VERHARDING { get; set; }
    public string LBLVERHARD { get; set; }
    public string DEFVERHARD { get; set; }
}

public class RoadSegmentSurfaceTypeCodeListRecordConfiguration : IEntityTypeConfiguration<RoadSegmentSurfaceTypeCodeListRecord>
{
    public const string TableName = "WegsegmentCodelijstVerharding";

    public void Configure(EntityTypeBuilder<RoadSegmentSurfaceTypeCodeListRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.PbsSchema).HasKey(p => p.VERHARDING);
        b.Property(p => p.VERHARDING).ValueGeneratedNever();
        b.Property(p => p.LBLVERHARD).HasColumnType("varchar(64)");
        b.Property(p => p.DEFVERHARD).HasColumnType("varchar(254)");
    }
}

public class RoadSegmentCategoryCodeListRecord
{
    public string WEGCAT { get; set; }
    public string LBLWEGCAT { get; set; }
    public string DEFWEGCAT { get; set; }
}

public class RoadSegmentCategoryCodeListRecordConfiguration : IEntityTypeConfiguration<RoadSegmentCategoryCodeListRecord>
{
    public const string TableName = "WegsegmentCodelijstWegcategorie";

    public void Configure(EntityTypeBuilder<RoadSegmentCategoryCodeListRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.PbsSchema).HasKey(p => p.WEGCAT);
        b.Property(p => p.WEGCAT).ValueGeneratedNever().HasColumnType("varchar(5)");
        b.Property(p => p.LBLWEGCAT).HasColumnType("varchar(64)");
        b.Property(p => p.DEFWEGCAT).HasColumnType("varchar(254)");
    }
}

public class RoadSegmentMaintenanceAuthorityCodeListRecord
{
    public string BEHEER { get; set; }
    public string LBLBEHEER { get; set; }
    public string OVOCODE { get; set; }
}

public class RoadSegmentMaintenanceAuthorityCodeListRecordConfiguration : IEntityTypeConfiguration<RoadSegmentMaintenanceAuthorityCodeListRecord>
{
    public const string TableName = "WegsegmentCodelijstWegbeheerder";

    public void Configure(EntityTypeBuilder<RoadSegmentMaintenanceAuthorityCodeListRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.PbsSchema).HasKey(p => p.BEHEER);
        b.Property(p => p.BEHEER).ValueGeneratedNever().HasColumnType("varchar(18)");
        b.Property(p => p.LBLBEHEER).HasColumnType("varchar(64)");
        b.Property(p => p.OVOCODE).HasColumnType("varchar(9)");
    }
}
