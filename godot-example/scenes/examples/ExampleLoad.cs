using BulkAutoTile;
using Godot;
using System;
using System.Collections.Generic;

namespace BulkAutoTileExample;

public partial class ExampleLoad : Node2D
{
  const string IMAGES_DIRECTORY = "./resources/images";
  const string AUTO_TILE_NAME = "ExampleAutoTiled";
  const string AUTO_TILE_CONFIG_PATH = "./resources/AutoTileConfig.json";

  public override void _Ready()
  {
    // This is how its expected to be loaded in production (from premade json).
    var bulkAutoTileConfig = BulkAutoTileConfig.LoadFromFile(AUTO_TILE_CONFIG_PATH);

    // In production rather that doing this its better to assign the tiles in some presistent way
    Dictionary<string, int> tileNamesToIds = bulkAutoTileConfig.GetRandomTileIdsToTileNames();
    // The rest is exactly the same as in simple example
    BulkAutoTileDrawerComposer bulkAutoTileDrawerComposer = new(
      bulkAutoTileConfig,
      IMAGES_DIRECTORY,
      tileNamesToIds
    );

    var bulkAutoTileDrawer = bulkAutoTileDrawerComposer.GetBulkAutoTileDrawer();
    AddChild(bulkAutoTileDrawer);

    Random random = new();
    List<KeyValuePair<Vector2I, int>> tilesToDraw = new();
    int tileId1 = tileNamesToIds[AUTO_TILE_NAME];
    for (int i = 0; i < 128; i++)
      tilesToDraw.Add(new(new(random.Next(0, 16), random.Next(0, 16)), tileId1));

    bulkAutoTileDrawer.DrawTiles(0, tilesToDraw.ToArray());
  }

}
