using Godot;
using BulkAutoTile;
using System.Collections.Generic;

namespace BulkAutoTileExample;

public partial class Benchmark : Node
{
  // NOTE: This is a very simplistic benchmark ran in debug (or release if you want)
  // NOTE: If async and deferred drawing was used this would be even faster for BulkAutoTile!
  public override void _Ready()
  {
    Vector2I chunkSize = new(256, 256);

    var godotBenchmark = RunGodotBenchmark(10, chunkSize);
    PrintBenchmarkResult(
      godotBenchmark, $"Godot built in auto-tile for {chunkSize.X}x{chunkSize.Y}");

    var customBenchmark = RunCustomBenchmark(10, chunkSize);
    PrintBenchmarkResult(
      customBenchmark, $"BulkAutoTile auto-tile for {chunkSize.X}x{chunkSize.Y}");
  }

  private BenchmarkInstance RunGodotBenchmark(int repeats, Vector2I chunkSize)
  {
    var tileSet = ResourceLoader.Load<TileSet>("resources/BenchmarkTileSet.tres");
    var tileMapLayer = new TileMapLayer()
    {
      TileSet = tileSet
    };
    AddChild(tileMapLayer);

    Godot.Collections.Array<Vector2I> positions = new();
    for (int x = 0; x < chunkSize.X; x++)
      for (int y = 0; y < chunkSize.Y; y++)
        positions.Add(new(x, y));

    BenchmarkInstance benchmarkInstance = new(
      () => tileMapLayer.SetCellsTerrainConnect(positions, 0, 0),
      tileMapLayer.Clear,
      repeats);

    benchmarkInstance.Run();

    tileMapLayer.QueueFree();

    return benchmarkInstance;
  }

  private BenchmarkInstance RunCustomBenchmark(int repeats, Vector2I chunkSize)
  {
    var bulkAutoTileConfig = BulkAutoTileConfig.LoadFromFile("./resources/BenchmarkAutoTileConfig.json");

    BulkAutoTileDrawerComposer bulkAutoTileDrawerComposer = new(
      bulkAutoTileConfig,
      "./resources/autotile-set-images",
      bulkAutoTileConfig.GetRandomTileIdsToTileNames()
    );

    // Using not deferred version so its fair for godot built in.
    var bulkAutoTileDrawer = bulkAutoTileDrawerComposer.GetBulkAutoTileDrawerNotDeferred();
    AddChild(bulkAutoTileDrawer);

    List<KeyValuePair<Vector2I, int>> tilesToDraw = new();
    for (int x = 0; x < chunkSize.X; x++)
      for (int y = 0; y < chunkSize.Y; y++)
        tilesToDraw.Add(new(new(x, y), 0));

    BenchmarkInstance benchmarkInstance = new(
      () => bulkAutoTileDrawer.DrawTiles(0, tilesToDraw.ToArray()),
      bulkAutoTileDrawer.Clear,
      repeats);

    benchmarkInstance.Run();

    bulkAutoTileDrawer.QueueFree();
    return benchmarkInstance;
  }

  private static void PrintBenchmarkResult(BenchmarkInstance benchmarkInstance, string info)
  {
    GD.Print($"""
    -------------
    Result for benchmark:
    [] {info}
    Repeat count: {benchmarkInstance.Repeats}
    -> Average time: {benchmarkInstance.AverageTimeMs}ms
    -> Median time: {benchmarkInstance.MedianTimeMs}ms
    -> Max time: {benchmarkInstance.MaxTimeMs}ms
    -> Min time: {benchmarkInstance.MinTimeMs}ms
    -------------
    """);
  }
}
