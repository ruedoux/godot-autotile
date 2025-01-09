using BulkAutoTile;
using Godot;
using System;
using System.Collections.Generic;

namespace BulkAutoTileExample;

public partial class SimpleExample : Node2D
{
  const string IMAGES_DIRECTORY = "./resources/images";
  const string AUTO_TILE_FILE = "ExampleAutoTiled.png";
  const string AUTO_TILE_BITMASK = "ExampleBitmaskName";
  const string AUTO_TILE_NAME = "ExampleAutoTiled";
  const string TILE_SET_TEMPLATE_PATH = "resources/TileSetTemplate.tres";


  public override void _Ready()
  {
    // Explanation of configuration is in `GetBulkAutoTileConfig` function
    BulkAutoTileConfig bulkAutoTileConfig = GetBulkAutoTileConfig();

    // In this example random tile ids are assigned to tile names.
    // You could assign your own ids for each of the tile names if you wanted
    Dictionary<string, int> tileNamesToIds = bulkAutoTileConfig.GetRandomTileIdsToTileNames();

    // In order to get the autoTile drawer, it first needs to be made from configuration
    BulkAutoTileDrawerComposer bulkAutoTileDrawerComposer = new(
      bulkAutoTileConfig,
      IMAGES_DIRECTORY, // The root directory where all images for autoTile reside
      tileNamesToIds
    );

    // The drawer node is a godot abstraction layer on top of the autoTile algorithm.
    // It can be treated exactly like a regular TileMap, just add it to a scene.
    // NOTE: QueueFree() it like a regular node to avoid memory leak.
    var bulkAutoTileDrawer = bulkAutoTileDrawerComposer.GetBulkAutoTileDrawer();
    AddChild(bulkAutoTileDrawer);

    // In order to optimize bulk placement of tiles you have to create a list of
    // positions paired with tile ids
    Random random = new();
    List<KeyValuePair<Vector2I, int>> tilesToDraw = new();
    int tileId1 = tileNamesToIds[AUTO_TILE_NAME];
    for (int i = 0; i < 128; i++)
      tilesToDraw.Add(new(new(random.Next(0, 16), random.Next(0, 16)), tileId1));

    // This is straight forward, draw the tiles on layer 0
    bulkAutoTileDrawer.DrawTiles(0, tilesToDraw.ToArray());
  }

  // This only should be used for testing, rather than doing this in code prepare
  // a proper json configuration like in ExampleLoad.cs
  private static BulkAutoTileConfig GetBulkAutoTileConfig()
  {
    // For this config instead of handcrafting the bitmask we can use godot abstraction
    // to make the job easier. Simply create a terrain in TileSet and load it here.
    int terrain = 0;
    int terrainSet = 0;
    var tileSetTemplate = ResourceLoader.Load<TileSet>(TILE_SET_TEMPLATE_PATH);

    // Builder is used for crafting the config
    AutoTileConfigBuilder autoTileConfigBuilder = new();
    autoTileConfigBuilder.SetTileSize(16); // Aka 16x16 tile

    // Pull bitmask from TileSet and assign name to it
    autoTileConfigBuilder.AddBitmaskSetFromTileSet(
      AUTO_TILE_BITMASK,
      tileSetTemplate,
      terrainSet,
      terrain);

    // Tile definition is basically creating a single tile from scratch
    autoTileConfigBuilder.AddTileDefinition(
      AUTO_TILE_NAME, // The name of the tile (so it can be referenced later)
      Layer: 0, // Layer, works exactly like in regulat TileMapLayer
      ImageFileName: AUTO_TILE_FILE, // The name of the image, without the extension
      BitmaskName: AUTO_TILE_BITMASK, // The name of the previously defined bitmask
      PositionInSet: new(0, 0), // Where the set starts on the image (Its possible to put multiple sets in one image)
      AutoTileGroup: -1 // Defines the tile group. Tile groups autoTile with each other
      );

    return autoTileConfigBuilder.BuildBulkAutoTileConfig();
  }
}
