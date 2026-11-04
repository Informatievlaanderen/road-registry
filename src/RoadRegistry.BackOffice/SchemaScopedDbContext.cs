namespace RoadRegistry.BackOffice;

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

// A read model that can be mapped to more than one schema by the same DbContext type: the production one, and the
// shadow copy a rebuild fills while the live one keeps serving.
//
// The entity configurations name their schema, so a context wanting another one rewrites it after the model is built.
// That only works if the two models can coexist: EF caches one model per context type, so a context type serving two
// schemas has to say so - which is what SchemaAwareModelCacheKeyFactory below is for. Miss it and the first schema to
// build the model is the one every later instance writes to, whatever it asked for.
public interface ISchemaScopedDbContext
{
    string Schema { get; }
}

public static class SchemaScopedDbContextExtensions
{
    // Applied at the end of OnModelCreating, so it overrides whatever the entity configurations named. Only tables:
    // neither read model owns a sequence - every generated key is an identity column, which lives with its table.
    public static void MapToSchema(this ModelBuilder modelBuilder, string schema)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.GetTableName() is not null)
            {
                entityType.SetSchema(schema);
            }
        }
    }

    // The schema travels on the options rather than as a constructor parameter. It has to: EF's DbContextFactory
    // builds an activator for the context type and refuses a type with more than one constructor it could use, so a
    // context that keeps its single (DbContextOptions) constructor is the only kind AddDbContextFactory can serve.
    public static DbContextOptionsBuilder<TContext> UseSchema<TContext>(this DbContextOptionsBuilder<TContext> optionsBuilder, string schema)
        where TContext : DbContext
    {
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new SchemaOptionsExtension(schema));

        return optionsBuilder;
    }

    // The schema these options were built for, or nothing when they name none - which is every context but a shadow.
    public static string? FindSchema(this DbContextOptions options)
    {
        return options.FindExtension<SchemaOptionsExtension>()?.Schema;
    }

    // Registered by the context itself in OnConfiguring rather than by whoever builds the options, so a context that
    // can be schema-scoped is never handed a model that was cached for another schema.
    public static DbContextOptionsBuilder UseSchemaAwareModelCache(this DbContextOptionsBuilder optionsBuilder)
    {
        return optionsBuilder.ReplaceService<IModelCacheKeyFactory, SchemaAwareModelCacheKeyFactory>();
    }
}

public sealed class SchemaAwareModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        return (context.GetType(), (context as ISchemaScopedDbContext)?.Schema, designTime);
    }
}

public sealed class SchemaOptionsExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;

    public SchemaOptionsExtension(string schema)
    {
        Schema = schema;
    }

    public string Schema { get; }

    public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services)
    {
    }

    public void Validate(IDbContextOptions options)
    {
    }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        public ExtensionInfo(SchemaOptionsExtension extension)
            : base(extension)
        {
        }

        private new SchemaOptionsExtension Extension => (SchemaOptionsExtension)base.Extension;

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => $"Schema={Extension.Schema} ";

        // Two schemas are two models, so they do not share EF's internal service provider - which is where the model
        // cache lives.
        public override int GetServiceProviderHashCode()
        {
            return Extension.Schema.GetHashCode();
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        {
            return other is ExtensionInfo info && info.Extension.Schema == Extension.Schema;
        }

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["Schema"] = Extension.Schema;
        }
    }
}
