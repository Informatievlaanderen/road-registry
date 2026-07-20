namespace RoadRegistry.WmsWfsV2.Schema.Records;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentMorphologyAttributeRecord
{
    public int MO_OIDN { get; set; }
    public int WS_OIDN { get; set; }
    public int? MORF { get; set; }
    public string? LBLMORF { get; set; }
    public double VANPOS { get; set; }
    public double TOTPOS { get; set; }
}

public class RoadSegmentMorphologyAttributeRecordConfiguration : IEntityTypeConfiguration<RoadSegmentMorphologyAttributeRecord>
{
    public const string TableName = "WegsegmentMorfologieAttributen";

    public void Configure(EntityTypeBuilder<RoadSegmentMorphologyAttributeRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsWfsV2Schema).HasKey(p => p.MO_OIDN);
        b.Property(p => p.MO_OIDN).ValueGeneratedOnAdd();
        b.Property(p => p.LBLMORF).HasColumnType("varchar(64)");
        b.HasIndex(p => p.WS_OIDN);
    }
}

public class RoadSegmentStreetNameAttributeRecord
{
    public int SN_OIDN { get; set; }
    public int WS_OIDN { get; set; }
    public int? STRTNMID { get; set; }
    public int? KANT { get; set; }
    public string? LBLKANT { get; set; }
    public double VANPOS { get; set; }
    public double TOTPOS { get; set; }
}

public class RoadSegmentStreetNameAttributeRecordConfiguration : IEntityTypeConfiguration<RoadSegmentStreetNameAttributeRecord>
{
    public const string TableName = "WegsegmentStraatnaamAttributen";

    public void Configure(EntityTypeBuilder<RoadSegmentStreetNameAttributeRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsWfsV2Schema).HasKey(p => p.SN_OIDN);
        b.Property(p => p.SN_OIDN).ValueGeneratedOnAdd();
        b.Property(p => p.LBLKANT).HasColumnType("varchar(64)");
        b.HasIndex(p => p.WS_OIDN);
    }
}

public class RoadSegmentAccessRestrictionAttributeRecord
{
    public int TO_OIDN { get; set; }
    public int WS_OIDN { get; set; }
    public int? TOEGANG { get; set; }
    public string? LBLTOEGANG { get; set; }
    public double VANPOS { get; set; }
    public double TOTPOS { get; set; }
}

public class RoadSegmentAccessRestrictionAttributeRecordConfiguration : IEntityTypeConfiguration<RoadSegmentAccessRestrictionAttributeRecord>
{
    public const string TableName = "WegsegmentToegangAttributen";

    public void Configure(EntityTypeBuilder<RoadSegmentAccessRestrictionAttributeRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsWfsV2Schema).HasKey(p => p.TO_OIDN);
        b.Property(p => p.TO_OIDN).ValueGeneratedOnAdd();
        b.Property(p => p.LBLTOEGANG).HasColumnType("varchar(64)");
        b.HasIndex(p => p.WS_OIDN);
    }
}

public class RoadSegmentCarTrafficDirectionAttributeRecord
{
    public int AU_OIDN { get; set; }
    public int WS_OIDN { get; set; }
    public int? RICHTING { get; set; }
    public string? LBLRICHT { get; set; }
    public double VANPOS { get; set; }
    public double TOTPOS { get; set; }
}

public class RoadSegmentCarTrafficDirectionAttributeRecordConfiguration : IEntityTypeConfiguration<RoadSegmentCarTrafficDirectionAttributeRecord>
{
    public const string TableName = "WegsegmentVerkeerstypeAutoAttributen";

    public void Configure(EntityTypeBuilder<RoadSegmentCarTrafficDirectionAttributeRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsWfsV2Schema).HasKey(p => p.AU_OIDN);
        b.Property(p => p.AU_OIDN).ValueGeneratedOnAdd();
        b.Property(p => p.LBLRICHT).HasColumnType("varchar(64)");
        b.HasIndex(p => p.WS_OIDN);
    }
}

public class RoadSegmentBikeTrafficDirectionAttributeRecord
{
    public int FI_OIDN { get; set; }
    public int WS_OIDN { get; set; }
    public int? RICHTING { get; set; }
    public string? LBLRICHT { get; set; }
    public double VANPOS { get; set; }
    public double TOTPOS { get; set; }
}

public class RoadSegmentBikeTrafficDirectionAttributeRecordConfiguration : IEntityTypeConfiguration<RoadSegmentBikeTrafficDirectionAttributeRecord>
{
    public const string TableName = "WegsegmentVerkeerstypeFietsAttributen";

    public void Configure(EntityTypeBuilder<RoadSegmentBikeTrafficDirectionAttributeRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsWfsV2Schema).HasKey(p => p.FI_OIDN);
        b.Property(p => p.FI_OIDN).ValueGeneratedOnAdd();
        b.Property(p => p.LBLRICHT).HasColumnType("varchar(64)");
        b.HasIndex(p => p.WS_OIDN);
    }
}

public class RoadSegmentPedestrianTrafficDirectionAttributeRecord
{
    public int VO_OIDN { get; set; }
    public int WS_OIDN { get; set; }
    public int? RICHTING { get; set; }
    public string? LBLRICHT { get; set; }
    public double VANPOS { get; set; }
    public double TOTPOS { get; set; }
}

public class RoadSegmentPedestrianTrafficDirectionAttributeRecordConfiguration : IEntityTypeConfiguration<RoadSegmentPedestrianTrafficDirectionAttributeRecord>
{
    public const string TableName = "WegsegmentVerkeerstypeVoetgangerAttributen";

    public void Configure(EntityTypeBuilder<RoadSegmentPedestrianTrafficDirectionAttributeRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsWfsV2Schema).HasKey(p => p.VO_OIDN);
        b.Property(p => p.VO_OIDN).ValueGeneratedOnAdd();
        b.Property(p => p.LBLRICHT).HasColumnType("varchar(64)");
        b.HasIndex(p => p.WS_OIDN);
    }
}

public class RoadSegmentMaintenanceAuthorityAttributeRecord
{
    public int WB_OIDN { get; set; }
    public int WS_OIDN { get; set; }
    public string? BEHEER { get; set; }
    public int? KANT { get; set; }
    public string? LBLKANT { get; set; }
    public double VANPOS { get; set; }
    public double TOTPOS { get; set; }
}

public class RoadSegmentMaintenanceAuthorityAttributeRecordConfiguration : IEntityTypeConfiguration<RoadSegmentMaintenanceAuthorityAttributeRecord>
{
    public const string TableName = "WegsegmentWegbeheerderAttributen";

    public void Configure(EntityTypeBuilder<RoadSegmentMaintenanceAuthorityAttributeRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsWfsV2Schema).HasKey(p => p.WB_OIDN);
        b.Property(p => p.WB_OIDN).ValueGeneratedOnAdd();
        b.Property(p => p.BEHEER).HasColumnType("varchar(18)");
        b.Property(p => p.LBLKANT).HasColumnType("varchar(64)");
        b.HasIndex(p => p.WS_OIDN);
    }
}

public class RoadSegmentCategoryAttributeRecord
{
    public int WC_OIDN { get; set; }
    public int WS_OIDN { get; set; }
    public string? WEGCAT { get; set; }
    public string? LBLWEGCAT { get; set; }
    public double VANPOS { get; set; }
    public double TOTPOS { get; set; }
}

public class RoadSegmentCategoryAttributeRecordConfiguration : IEntityTypeConfiguration<RoadSegmentCategoryAttributeRecord>
{
    public const string TableName = "WegsegmentWegcategorieAttributen";

    public void Configure(EntityTypeBuilder<RoadSegmentCategoryAttributeRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsWfsV2Schema).HasKey(p => p.WC_OIDN);
        b.Property(p => p.WC_OIDN).ValueGeneratedOnAdd();
        b.Property(p => p.WEGCAT).HasColumnType("varchar(5)");
        b.Property(p => p.LBLWEGCAT).HasColumnType("varchar(64)");
        b.HasIndex(p => p.WS_OIDN);
    }
}

public class RoadSegmentSurfaceTypeAttributeRecord
{
    public int WV_OIDN { get; set; }
    public int WS_OIDN { get; set; }
    public int? VERHARDING { get; set; }
    public string? LBLVERHARD { get; set; }
    public double VANPOS { get; set; }
    public double TOTPOS { get; set; }
}

public class RoadSegmentSurfaceTypeAttributeRecordConfiguration : IEntityTypeConfiguration<RoadSegmentSurfaceTypeAttributeRecord>
{
    public const string TableName = "WegsegmentWegverhardingAttributen";

    public void Configure(EntityTypeBuilder<RoadSegmentSurfaceTypeAttributeRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsWfsV2Schema).HasKey(p => p.WV_OIDN);
        b.Property(p => p.WV_OIDN).ValueGeneratedOnAdd();
        b.Property(p => p.LBLVERHARD).HasColumnType("varchar(64)");
        b.HasIndex(p => p.WS_OIDN);
    }
}
