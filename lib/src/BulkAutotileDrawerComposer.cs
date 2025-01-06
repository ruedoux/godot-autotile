using System.Collections.Generic;
using System.Linq;
using AutoTile;
using GameCore;
using Godot;

namespace BulkAutoTile;

public class BulkAutoTileDrawer : Node2D
{
  private readonly GodotTileMap godotTileMap;
  private readonly AutoTileDrawer autoTileDrawer;

  internal BulkAutoTileDrawer(AutoTileDrawer autoTileDrawer, GodotTileMap godotTileMap)
  {
    this.autoTileDrawer = autoTileDrawer;
    this.godotTileMap = godotTileMap;
    foreach (var tileMapLayer in godotTileMap.TileMapLayers)
      AddChild(tileMapLayer);
  }

  public void Clear()
    => autoTileDrawer.Clear();

  public void Wait()
    => autoTileDrawer.Wait();

  public void DrawTilesAsync(int layer, KeyValuePair<Vector2I, int>[] positionToTileIds)
  {
    var positionToTileIdsConverted = positionToTileIds
      .Select(kvp => new KeyValuePair<Vector2Int, int>(
          TypeMapper.Map(kvp.Key),
          kvp.Value))
      .ToArray();
    autoTileDrawer.DrawTilesAsync(layer, positionToTileIdsConverted);
  }

  public void DrawTiles(int layer, KeyValuePair<Vector2I, int>[] positionToTileIds)
  {
    var positionToTileIdsConverted = positionToTileIds
      .Select(kvp => new KeyValuePair<Vector2Int, int>(
          TypeMapper.Map(kvp.Key),
          kvp.Value))
      .ToArray();
    autoTileDrawer.DrawTiles(layer, positionToTileIdsConverted);
  }

  public void UpdateTiles(int tileLayer, Vector2I[] positions)
  {
    var positionsConverted = positions.Select(TypeMapper.Map).ToArray();
    autoTileDrawer.UpdateTiles(tileLayer, positionsConverted);
  }

  public new void QueueFree()
  {
    foreach (var tileMapLayer in godotTileMap.TileMapLayers)
      tileMapLayer.QueueFree();
    base.QueueFree();
  }
}

public class BulkAutoTileDrawerComposer
{
  private readonly AutoTileConfig autoTileConfig;
  private readonly AutoTilerComposer autoTilerComposer;

  public BulkAutoTileDrawerComposer(
    BulkAutoTileConfig BulkAutoTileConfig,
    string autoTileImageDirectory,
    Dictionary<string, int> tileNameToIds)
  {
    autoTileConfig = BulkAutoTileConfig.GetAutoTileConfig();
    autoTilerComposer = new(
      autoTileImageDirectory,
      autoTileConfig,
      tileNameToIds);
  }

  /// <summary>
  /// Uses deferred call for TileMapLayer, async compatible.
  /// </summary>
  public BulkAutoTileDrawer GetBulkAutoTileDrawer()
  {
    var tileSize = new Vector2Int(autoTileConfig.TileSize, autoTileConfig.TileSize);
    var godotTileMap = TileMapLoader.Load(autoTilerComposer.TileLoader, tileSize);
    TileMapDrawer tileMapDrawer = new(godotTileMap);
    return new(new(tileMapDrawer, autoTilerComposer.GetAutoTiler()), godotTileMap);
  }

  /// <summary>
  /// Does not use deferred call for TileMapLayer, not async compatible.
  /// </summary>
  public BulkAutoTileDrawer GetBulkAutoTileDrawerNotDeferred()
  {
    var tileSize = new Vector2Int(autoTileConfig.TileSize, autoTileConfig.TileSize);
    var godotTileMap = TileMapLoader.Load(autoTilerComposer.TileLoader, tileSize);
    TileMapDrawerNotDeferred tileMapDrawerNotDeferred = new(godotTileMap);
    return new(new(tileMapDrawerNotDeferred, autoTilerComposer.GetAutoTiler()), godotTileMap);
  }
}