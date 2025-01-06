using BulkAutoTile;
using Godot;
using System.Collections.Generic;

namespace BulkAutoTileExample;

public partial class ExampleLoad : Node2D
{
  private readonly string autotileImagesDirectory = "./resources/autotile-set-images";
  private readonly string autotileBitmaskName = "ExampleBitmaskName";
  private readonly string autotileTileName = "ExampleAutoTiled";

  public override void _Ready()
  {
    // This is how its expected to be loaded in production (from premade json).
    var bulkAutoTileConfig = BulkAutoTileConfig.LoadFromFile("./resources/AutoTileConfig.json");

    // In production rather that doing this its better to assign the tiles in some presistent way
    Dictionary<string, int> tileNamesToIds = bulkAutoTileConfig.GetRandomTileIdsToTileNames();
    // The rest is exactly the same as in simple example
    BulkAutoTileDrawerComposer bulkAutoTileDrawerComposer = new(
      bulkAutoTileConfig,
      autotileImagesDirectory,
      tileNamesToIds
    );

    var bulkAutoTileDrawer = bulkAutoTileDrawerComposer.GetBulkAutoTileDrawer();
    AddChild(bulkAutoTileDrawer);

    List<KeyValuePair<Vector2I, int>> tilesToDraw = new()
    {
      new(new(0, 0), tileNamesToIds[autotileTileName]),
      new(new(1, 0), tileNamesToIds[autotileTileName])
    };

    bulkAutoTileDrawer.DrawTiles(0, tilesToDraw.ToArray());
  }

}
