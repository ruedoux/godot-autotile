using BulkAutoTile;
using Godot;
using System;
using System.Collections.Generic;

namespace BulkAutoTileExample;

public partial class ExamplePretty : Node2D
{
  const string IMAGES_DIRECTORY = "./resources/grass-tileset";
  const string GRASS_TILE_NAME = "GrassHills";
  const string WATER_TILE_NAME = "Water";
  const string AUTO_TILE_CONFIG_PATH = "./resources/PrettyAutoTileConfig.json";

  public override void _Ready()
  {
    var bulkAutoTileConfig = BulkAutoTileConfig.LoadFromFile(AUTO_TILE_CONFIG_PATH);
    Dictionary<string, int> tileNamesToIds = bulkAutoTileConfig.GetRandomTileIdsToTileNames();

    BulkAutoTileDrawerComposer bulkAutoTileDrawerComposer = new(
      bulkAutoTileConfig,
      IMAGES_DIRECTORY,
      tileNamesToIds
    );

    var bulkAutoTileDrawer = bulkAutoTileDrawerComposer.GetBulkAutoTileDrawer();
    AddChild(bulkAutoTileDrawer);

    Random random = new();
    List<KeyValuePair<Vector2I, int>> tilesToDraw = new();
    int grassTileId = tileNamesToIds[GRASS_TILE_NAME];
    int waterTileId = tileNamesToIds[WATER_TILE_NAME];

    for (int x = -1; x < 40; x++)
      for (int y = -1; y < 40; y++)
        tilesToDraw.Add(new(new(x, y), waterTileId));

    for (int x = 0; x < 39; x++)
      for (int y = 0; y < 39; y++)
        if (random.Next(0, 10) > 2)
          tilesToDraw.Add(new(new(x, y), grassTileId));

    bulkAutoTileDrawer.DrawTiles(0, tilesToDraw.ToArray());
  }

}
