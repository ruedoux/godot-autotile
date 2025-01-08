using Godot;
using BulkAutoTile;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Diagnostics.Metrics;
using System.Threading.Tasks.Dataflow;

namespace BulkAutoTileExample;

public partial class Benchmark : Node
{
  const string TILE_SET_TEMPLATE_PATH = "res://resources/TileSetTemplate.tres";

  const string IMAGES_DIRECTORY = "resources/images";
  const string AUTO_TILE_CONFIG_PATH = "resources/AutoTileConfig.json";
  const string RESULT_FILE = "benchmark-result.txt";

  // NOTE: This is a very simplistic benchmark, should be ran in release since 
  //       debug will affect the results
  public override void _Ready()
  {
    string result = "";
    Vector2I chunkSize64 = new(64, 64);
    Vector2I chunkSize128 = new(128, 128);
    Vector2I chunkSize256 = new(256, 256);
    Vector2I chunkSize512 = new(512, 512);

    string osName = OS.GetName();
    string osVersion = System.Environment.OSVersion.VersionString;
    string cpuInfo = OS.GetProcessorName();
    string gpuInfo = RenderingServer.GetRenderingDevice().GetDeviceName();

    result += $"Platform: {osName}, version: {osVersion}, CPU: {cpuInfo}, GPU: {gpuInfo}" + System.Environment.NewLine;
    result += "# ONE THREAD" + System.Environment.NewLine;
    result += RunGodotBenchmarkSync(10, chunkSize64) + System.Environment.NewLine;
    result += RunGodotBenchmarkSync(10, chunkSize128) + System.Environment.NewLine;

    result += RunCustomBenchmarkSync(10, chunkSize64) + System.Environment.NewLine;
    result += RunCustomBenchmarkSync(10, chunkSize128) + System.Environment.NewLine;

    result += "# ASYNC" + System.Environment.NewLine;
    result += RunCustomBenchmarkAsync(10, chunkSize128, 32) + System.Environment.NewLine;
    result += RunCustomBenchmarkAsync(10, chunkSize256, 32) + System.Environment.NewLine;
    result += RunCustomBenchmarkAsync(10, chunkSize512, 32) + System.Environment.NewLine;

    result += RunCustomBenchmarkAsync(10, chunkSize128, 64) + System.Environment.NewLine;
    result += RunCustomBenchmarkAsync(10, chunkSize256, 64) + System.Environment.NewLine;
    result += RunCustomBenchmarkAsync(10, chunkSize512, 64) + System.Environment.NewLine;

    File.WriteAllText(RESULT_FILE, result);
  }

  private string RunGodotBenchmarkSync(int repeats, Vector2I chunkSize)
  {
    var tileSet = ResourceLoader.Load<TileSet>(TILE_SET_TEMPLATE_PATH);
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

    RemoveChild(tileMapLayer);
    tileMapLayer.QueueFree();

    return benchmarkInstance.GetResult($"Godot for chunk size {chunkSize.X}x{chunkSize.Y}");
  }

  private string RunCustomBenchmarkSync(int repeats, Vector2I chunkSize)
  {
    var bulkAutoTileConfig = BulkAutoTileConfig.LoadFromFile(AUTO_TILE_CONFIG_PATH);

    BulkAutoTileDrawerComposer bulkAutoTileDrawerComposer = new(
      bulkAutoTileConfig,
      IMAGES_DIRECTORY,
      bulkAutoTileConfig.GetRandomTileIdsToTileNames()
    );

    var bulkAutoTileDrawer = bulkAutoTileDrawerComposer.GetBulkAutoTileDrawer();
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

    RemoveChild(bulkAutoTileDrawer);
    bulkAutoTileDrawer.QueueFree();

    return benchmarkInstance.GetResult($"Custom for chunk size {chunkSize.X}x{chunkSize.Y}");
  }

  private string RunCustomBenchmarkAsync(int repeats, Vector2I chunkSize, int bachSize)
  {
    var bulkAutoTileConfig = BulkAutoTileConfig.LoadFromFile(AUTO_TILE_CONFIG_PATH);

    BulkAutoTileDrawerComposer bulkAutoTileDrawerComposer = new(
      bulkAutoTileConfig,
      IMAGES_DIRECTORY,
      bulkAutoTileConfig.GetRandomTileIdsToTileNames()
    );

    var bulkAutoTileDrawer = bulkAutoTileDrawerComposer.GetBulkAutoTileDrawer();
    AddChild(bulkAutoTileDrawer);

    List<KeyValuePair<Vector2I, int>> tilesToDraw = new();
    for (int x = 0; x < chunkSize.X; x++)
      for (int y = 0; y < chunkSize.Y; y++)
        tilesToDraw.Add(new(new(x, y), 0));

    BenchmarkInstance benchmarkInstance = new(() =>
      {
        // Due to async support it's possible to simply batch the tiles to draw
        foreach (var partition in PartitionList(tilesToDraw, bachSize))
          bulkAutoTileDrawer.DrawTilesAsync(0, partition.ToArray());
        bulkAutoTileDrawer.Wait(); // Wait for all batches to finish
      },
      bulkAutoTileDrawer.Clear,
      repeats);

    benchmarkInstance.Run();

    RemoveChild(bulkAutoTileDrawer);
    bulkAutoTileDrawer.QueueFree();

    return benchmarkInstance.GetResult($"Custom for chunk size {chunkSize.X}x{chunkSize.Y} and batch size {bachSize}");
  }


  private static List<List<T>> PartitionList<T>(List<T> sourceList, int partitionSize)
  {
    var partitions = new List<List<T>>();
    for (int i = 0; i < sourceList.Count; i += partitionSize)
    {
      List<T> partition = sourceList.GetRange(i, Math.Min(partitionSize, sourceList.Count - i));
      partitions.Add(partition);
    }
    return partitions;
  }
}
