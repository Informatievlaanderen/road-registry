namespace RoadRegistry.Editor.Schema;

using Microsoft.EntityFrameworkCore;

public sealed class FakeEditorContext : EditorContext
{
    public FakeEditorContext()
    {
    }

    public FakeEditorContext(DbContextOptions<EditorContext> options)
        : base(options)
    {
    }
}
