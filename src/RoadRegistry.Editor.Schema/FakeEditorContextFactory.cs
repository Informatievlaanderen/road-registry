namespace RoadRegistry.Editor.Schema;

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class FakeEditorContextFactory : IDesignTimeDbContextFactory<FakeEditorContext>
{
    public FakeEditorContext CreateDbContext(params string[] args)
    {
        var builder = new DbContextOptionsBuilder<EditorContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
        return new FakeEditorContext(builder.Options);
    }
}
