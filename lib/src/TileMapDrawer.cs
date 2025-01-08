using System;
using System.Collections.Generic;
using AutoTile;
using GameCore;
using Godot;

namespace BulkAutoTile;

internal class TileMapDrawer : ITileMapDrawer
{
  private readonly GodotTileMap godotTileMap;

  public TileMapDrawer(GodotTileMap godotTileMap)
  {
    this.godotTileMap = godotTileMap;
  }

  public void Clear()
  {
    Callable.From(() =>
    {
      foreach (var tileMapLayer in godotTileMap.TileMapLayers)
        tileMapLayer.Clear();
    }).CallDeferred();
  }

  public void DrawTiles(int tileLayer, KeyValuePair<Vector2Int, AutoTile.TileData?>[] positionsToTileData)
  {
    if (godotTileMap.TileMapLayers.Length < tileLayer - 1 || tileLayer < 0)
      throw new ArgumentException($"AutoTileDrawer does not contain tile layer: {tileLayer}");

    Callable.From(() =>
    {
      foreach (var (position, tileData) in positionsToTileData)
        if (tileData is not null)
          godotTileMap.TileMapLayers[tileLayer].SetCell(
            TypeMapper.Map(position),
            godotTileMap.TileIdToSourceId[tileData.TileId],
            TypeMapper.Map(tileData.AtlasCoords));
        else
          godotTileMap.TileMapLayers[tileLayer].SetCell(TypeMapper.Map(position));
    }).CallDeferred();
  }
}