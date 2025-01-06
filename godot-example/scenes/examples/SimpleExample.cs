using BulkAutoTile;
using Godot;
using System.Collections.Generic;

namespace BulkAutoTileExample;

public partial class SimpleExample : Node2D
{
  private readonly string autotileImagesDirectory = "./resources/autotile-set-images";
  private readonly string autotileBitmaskName = "ExampleBitmaskName";
  private readonly string autotileTileName = "ExampleAutoTiled";

  public override void _Ready()
  {
    // Explanation of configuration is in `GetBulkAutoTileConfig` function
    BulkAutoTileConfig bulkAutoTileConfig = GetBulkAutoTileConfig();

    // In this example random tile ids are assigned to tile names.
    // You could assign your own ids for each of the tile names if you wanted
    Dictionary<string, int> tileNamesToIds = bulkAutoTileConfig.GetRandomTileIdsToTileNames();

    // In order to get the autotile drawer, it first needs to be made from configuration
    BulkAutoTileDrawerComposer bulkAutoTileDrawerComposer = new(
      bulkAutoTileConfig,
      autotileImagesDirectory, // The root directory where all images for autotile reside
      tileNamesToIds
    );

    // The drawer node is a godot abstraction layer on top of the autotile algorithm.
    // It can be treated exactly like a regular TileMap, just add it to a scene.
    // NOTE: QueueFree() it like a regular node to avoid memory leak.
    var bulkAutoTileDrawer = bulkAutoTileDrawerComposer.GetBulkAutoTileDrawer();
    AddChild(bulkAutoTileDrawer);

    // In order to optimize bulk placement of tiles you have to create a list of
    // positions paired with tile ids
    List<KeyValuePair<Vector2I, int>> tilesToDraw = new()
    {
      new(new(0, 0), tileNamesToIds[autotileTileName]), // Place tile autotileTileName on position=(0,0)
      new(new(1, 0), tileNamesToIds[autotileTileName])  // Place tile autotileTileName on position=(1,0)
    };

    // This is straight forward, draw the tiles on layer 0
    bulkAutoTileDrawer.DrawTiles(0, tilesToDraw.ToArray());
  }

  // This only should be used for testing, rather than doing this in code prepare
  // a proper json configuration like in ExampleLoad.cs
  private BulkAutoTileConfig GetBulkAutoTileConfig()
  {
    // For this config instead of handcrafting the bitmask we can use godot abstraction
    // to make the job easier. Simply create a terrain in TileSet and load it here.
    int terrain = 0;
    int terrainSet = 0;
    var tileSetTemplate = ResourceLoader.Load<TileSet>("resources/TileSetTemplate.tres");

    // Builder is used for crafting the config
    AutoTileConfigBuilder autotileConfigBuilder = new();
    autotileConfigBuilder.SetTileSize(16); // Aka 16x16 tile

    // Pull bitmask from TileSet and assign name to it
    autotileConfigBuilder.AddBitmaskSetFromTileSet(
      autotileBitmaskName,
      tileSetTemplate,
      terrainSet,
      terrain);

    // Tile definition is basically creating a single tile from scratch
    autotileConfigBuilder.AddTileDefinition(
      autotileTileName, // The name of the tile (so it can be referenced later)
      Layer: 0, // Layer, works exactly like in regulat TileMapLayer
      ImageFileName: autotileTileName, // The name of the image, without the extension
      BitmaskName: autotileBitmaskName, // The name of the previously defined bitmask
      PositionInSet: new(0, 0), // Where the set starts on the image (Its possible to put multiple sets in one image)
      AutoTileGroup: -1 // Defines the tile group. Tile groups autotile with each other
      );

    return autotileConfigBuilder.BuildBulkAutoTileConfig();
  }
}
