using System.Collections.Generic;
using AutoTile;
using GameCore;
using Godot;

namespace BulkAutoTile;

internal record GodotTileMap(
  TileMapLayer[] TileMapLayers,
  int[] TileIdToSourceId);

internal static class TileMapLoader
{
  public static GodotTileMap Load(TileLoader tileLoader, Vector2Int tileSize)
  {
    var tiles = tileLoader.LoadTiles();
    var pathToTileIds = AssignPathsToTileIds(tiles);
    int biggestLayer = GetBiggestLayer(tiles);

    Dictionary<int, HashSet<Vector2Int>> tileIdtoAtlasPositions = new();
    foreach (var (tileIdentificator, tileResource) in tiles)
      tileIdtoAtlasPositions[tileIdentificator.TileId] = new(tileResource.BitmaskMap.Keys);

    TileSet tileSet = new();
    int[] tileIdToSourceId = new int[tiles.Count];
    foreach (var kv in pathToTileIds)
    {
      var sourceId = AddSource(tileSet, kv.Key, tileSize);
      foreach (var tileId in kv.Value)
      {
        tileIdToSourceId[tileId] = sourceId;
        AssignTilesToSource(tileSet, sourceId, tileIdtoAtlasPositions[tileId]);
      }
    }

    TileMapLayer[] tileMapLayers = new TileMapLayer[biggestLayer + 1];
    for (int i = 0; i <= biggestLayer; i++)
    {
      tileMapLayers[i] = new()
      {
        TileSet = tileSet,
        TextureFilter = CanvasItem.TextureFilterEnum.Nearest
      };
    }

    return new(tileMapLayers, tileIdToSourceId);
  }

  private static Dictionary<string, HashSet<int>> AssignPathsToTileIds(
    Dictionary<TileIdentificator, TileResource> tiles)
  {
    Dictionary<string, HashSet<int>> pathToTileIds = new();
    foreach (var kv in tiles)
    {
      var imagePath = kv.Value.ImagePath;
      if (!pathToTileIds.TryGetValue(imagePath, out HashSet<int>? ids))
      {
        pathToTileIds[imagePath] = new();
        ids = pathToTileIds[imagePath];
      }

      ids.Add(kv.Key.TileId);
    }

    return pathToTileIds;
  }

  private static int GetBiggestLayer(Dictionary<TileIdentificator, TileResource> tiles)
  {
    int biggestLayer = 0;
    foreach (var kv in tiles)
      if (kv.Value.Layer > biggestLayer)
        biggestLayer = kv.Value.Layer;

    return biggestLayer;
  }

  private static int AddSource(TileSet tileSet, string sourceImagePath, Vector2Int tileSize)
  {
    var texture = Image.LoadFromFile(sourceImagePath);

    TileSetAtlasSource source = new()
    {
      TextureRegionSize = TypeMapper.Map(tileSize),
      Texture = ImageTexture.CreateFromImage(texture)
    };

    return tileSet.AddSource(source);
  }

  private static void AssignTilesToSource(
    TileSet tileSet, int sourceId, HashSet<Vector2Int> atlasPositions)
  {
    var source = (TileSetAtlasSource)tileSet.GetSource(sourceId);
    foreach (var position in atlasPositions)
      source.CreateTile(TypeMapper.Map(position));
  }
}