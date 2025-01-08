using BulkAutoTile;
using Godot;
using System;
using System.Collections.Generic;

namespace BulkAutoTileExample;

public partial class Example : Node2D
{
  const string IMAGES_DIRECTORY = "./resources/images";
  const string AUTO_TILE_NAME = "ExampleAutoTiled";
  const string AUTO_TILE_NAME2 = "ExampleAutoTiled2";
  const string AUTO_TILE_CONFIG_PATH = "./resources/AutoTileConfig.json";

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
    int tileId1 = tileNamesToIds[AUTO_TILE_NAME];
    int tileId2 = tileNamesToIds[AUTO_TILE_NAME2];

    for (int i = 0; i < 128; i++)
    {
      tilesToDraw.Add(new(new(random.Next(0, 16), random.Next(0, 16)), tileId1));
      tilesToDraw.Add(new(new(random.Next(0, 16), random.Next(0, 16)), tileId2));
    }

    bulkAutoTileDrawer.DrawTiles(0, tilesToDraw.ToArray());
  }
}
