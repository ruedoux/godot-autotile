using GameCore;
using Godot;

namespace BulkAutoTile;

internal static class TypeMapper
{
  public static Vector2I Map(Vector2Int v)
    => new(v.X, v.Y);

  public static Vector2Int Map(Vector2I v)
    => new(v.X, v.Y);

  public static Vector2Int Map(Vector2 v)
    => new((int)v.X, (int)v.Y);
}