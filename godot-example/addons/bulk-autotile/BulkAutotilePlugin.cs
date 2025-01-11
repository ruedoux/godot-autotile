#if TOOLS
using Godot;

namespace BulkAutoTilePlugin;

[Tool]
public partial class BulkAutoTilePlugin : EditorPlugin
{
  public override void _EnterTree()
  {
    GD.Print("For now there is no point in activating GodotAutoTile since the dll's are referenced via .csproj.");
    GD.Print("In the future the activation via plugin might be added.");
  }
}
#endif
