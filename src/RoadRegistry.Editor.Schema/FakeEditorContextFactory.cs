namespace RoadRegistry.Editor.Schema;

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;

public class FakeEditorContextFactory : IDesignTimeDbContextFactory<FakeEditorContext>
{
    public FakeEditorContext CreateDbContext(params string[] args)
    {
        var builder = new DbContextOptionsBuilder<EditorContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning));

        return new FakeEditorContext(builder.Options);
    }
}
