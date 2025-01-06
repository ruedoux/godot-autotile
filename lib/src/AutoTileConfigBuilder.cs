using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoTile;
using GameCore;
using Godot;
using static AutoTile.Bitmask.SurroundingDirection;
using static Godot.TileSet.CellNeighbor;

namespace BulkAutoTile;


public class BulkAutoTileConfig
{
  private readonly AutoTileConfig autoTileConfig;

  internal BulkAutoTileConfig(AutoTileConfig autoTileConfig)
  {
    this.autoTileConfig = autoTileConfig;
  }

  public Dictionary<string, int> GetRandomTileIdsToTileNames()
  {
    Dictionary<string, int> tileIdsToTileNames = new();

    int tileId = 0;
    foreach (var (tileName, tileDefinition) in autoTileConfig.TileDefinitions)
      tileIdsToTileNames[tileName] = tileId++;

    return tileIdsToTileNames;
  }

  public static BulkAutoTileConfig LoadFromFile(string path)
    => new(AutoTileConfig.LoadFromFile(path));

  public void SaveToFile(string path)
    => File.WriteAllText(path, autoTileConfig.ToJsonString());

  internal AutoTileConfig GetAutoTileConfig()
    => autoTileConfig;
}


public class AutoTileConfigBuilder
{
  private int tileSize = 0;
  private readonly Dictionary<string, TileDefinition> tileDefinitions = new();
  private readonly Dictionary<string, Dictionary<Vector2Int, byte>> bitmaskSets = new();

  public void SetTileSize(int tileSize)
    => this.tileSize = tileSize;

  public void AddTileDefinition(
    string tileName,
    int Layer,
    string ImageFileName,
    string BitmaskName,
    Vector2I PositionInSet,
    int AutoTileGroup)
    => tileDefinitions[tileName] = new(
        Layer: Layer,
        ImageFileName: ImageFileName,
        BitmaskName: BitmaskName,
        PositionInSet: TypeMapper.Map(PositionInSet),
        AutoTileGroup: AutoTileGroup);

  public void AddBitmaskSet(string bitmaskName, Dictionary<Vector2Int, byte> bitmaskSet)
    => bitmaskSets[bitmaskName] = bitmaskSet;

  public void AddBitmaskSetFromTileSet(
    string bitmaskName,
    TileSet tileSet,
    int terrainSet,
    int terrain,
    int altTileId = 0)
    => bitmaskSets[bitmaskName] = TileSetBitmaskConverter.GetBitmaskSetFromTileSet(
        tileSet,
        terrainSet,
        terrain,
        altTileId);

  public BulkAutoTileConfig BuildBulkAutoTileConfig()
    => new(AutoTileConfig.Construct(tileSize, tileDefinitions, bitmaskSets));
}

internal static class TileSetBitmaskConverter
{
  public static Dictionary<Vector2Int, byte> GetBitmaskSetFromTileSet(
    TileSet tileSet, int terrainSet, int terrain, int altTileId)
  {
    Dictionary<Vector2Int, byte> bitmaskSet = new();
    var sources = GetIdsMappedToSources(tileSet);
    foreach (var (sourceId, source) in sources)
      foreach (var (position, tileData) in GetAtlasPositionsMappedToTileData(source, altTileId))
        if (terrain == tileData.Terrain && terrainSet == tileData.TerrainSet)
          bitmaskSet[TypeMapper.Map(position)] = CreateBitmaskFrom(tileData);

    return bitmaskSet;
  }

  private static Dictionary<int, TileSetAtlasSource> GetIdsMappedToSources(TileSet tileSet)
      => Enumerable.Range(0, tileSet.GetSourceCount()).ToDictionary(
        tileSet.GetSourceId, index => (TileSetAtlasSource)tileSet.GetSource(tileSet.GetSourceId(index)));

  private static Dictionary<Vector2I, Godot.TileData> GetAtlasPositionsMappedToTileData(TileSetAtlasSource source, int altTileId)
    => Enumerable.Range(0, source.GetTilesCount()).ToDictionary(
      source.GetTileId, tileIndex => source.GetTileData(source.GetTileId(tileIndex), altTileId));

  private static byte CreateBitmaskFrom(Godot.TileData tileData)
  {
    byte bitmask = Bitmask.DEFAULT;

    bitmask = Bitmask.UpdateBitmask(bitmask, TOP_LEFT, TileDataHasBitmask(tileData, TopLeftCorner));
    bitmask = Bitmask.UpdateBitmask(bitmask, TOP, TileDataHasBitmask(tileData, TopSide));
    bitmask = Bitmask.UpdateBitmask(bitmask, TOP_RIGHT, TileDataHasBitmask(tileData, TopRightCorner));
    bitmask = Bitmask.UpdateBitmask(bitmask, RIGHT, TileDataHasBitmask(tileData, RightSide));
    bitmask = Bitmask.UpdateBitmask(bitmask, BOTTOM_RIGHT, TileDataHasBitmask(tileData, BottomRightCorner));
    bitmask = Bitmask.UpdateBitmask(bitmask, BOTTOM, TileDataHasBitmask(tileData, BottomSide));
    bitmask = Bitmask.UpdateBitmask(bitmask, BOTTOM_LEFT, TileDataHasBitmask(tileData, BottomLeftCorner));
    bitmask = Bitmask.UpdateBitmask(bitmask, LEFT, TileDataHasBitmask(tileData, LeftSide));

    return bitmask;
  }

  private static bool TileDataHasBitmask(Godot.TileData tileData, TileSet.CellNeighbor cellNeighbor)
    => tileData.GetTerrainPeeringBit(cellNeighbor) > -1;
}