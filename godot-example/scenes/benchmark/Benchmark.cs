using Godot;
using BulkAutoTile;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text.Json;

namespace BulkAutoTileExample;

// NOTE: This is a very simplistic benchmark, should be ran in release since 
//       debug will affect the results
public partial class Benchmark : Node
{
  const string TILE_SET_TEMPLATE_PATH = "res://resources/TileSetTemplate.tres";

  bool benchmarkRunning = false;
  Button StartBenchmarkButton = null!;

  public override void _Ready()
  {
    StartBenchmarkButton = GetNode<Button>("CanvasLayer/V/RichTextLabel");
    StartBenchmarkButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(StartBenchmark)));
  }

  private void StartBenchmark()
  {
    if (benchmarkRunning)
      return;

    benchmarkRunning = true;
    string result = "";
    string osName = OS.GetName();
    string osVersion = System.Environment.OSVersion.VersionString;
    string cpuInfo = OS.GetProcessorName();
    string gpuInfo = RenderingServer.GetRenderingDevice().GetDeviceName();

    string jsonString = File.ReadAllText("resources/BenchmarkConfig.json");
    var benchmarkConfig = JsonSerializer.Deserialize(
      jsonString, BenchmarkConfigJsonContext.Default.BenchmarkConfig)
      ?? throw new JsonException(nameof(jsonString));

    Vector2I chunkSize = new(benchmarkConfig.ChunkX, benchmarkConfig.ChunkY);
    int RepeatCount = benchmarkConfig.RepeatCount;
    int batchSize = benchmarkConfig.BatchSize;
    string autoTileConfigPath = benchmarkConfig.AutoTileConfigPath;
    string imagesDirectory = benchmarkConfig.ImagesDirectory;

    result += $"""
    Platform: {osName}, version: {osVersion}, CPU: {cpuInfo}, GPU: {gpuInfo}
    Chunk size: {chunkSize}, repeat count: {RepeatCount}, batch size for async: {batchSize}
    
    {RunGodotBenchmarkSync(RepeatCount, chunkSize)}
    {RunCustomBenchmarkSync(RepeatCount, chunkSize, autoTileConfigPath, imagesDirectory)}
    {RunCustomBenchmarkAsync(RepeatCount, chunkSize, batchSize, autoTileConfigPath, imagesDirectory)}
    """;

    File.WriteAllText(benchmarkConfig.OutputPath, result);
    GD.Print(result);
    benchmarkRunning = false;
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

    return benchmarkInstance.GetResult();
  }

  private string RunCustomBenchmarkSync(
    int repeats, Vector2I chunkSize, string AutoTileConfigPath, string imagesDirectory)
  {
    var bulkAutoTileConfig = BulkAutoTileConfig.LoadFromFile(AutoTileConfigPath);

    BulkAutoTileDrawerComposer bulkAutoTileDrawerComposer = new(
      bulkAutoTileConfig,
      imagesDirectory,
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

    return benchmarkInstance.GetResult();
  }

  private string RunCustomBenchmarkAsync(
    int repeats, Vector2I chunkSize, int bachSize, string AutoTileConfigPath, string imagesDirectory)
  {
    var bulkAutoTileConfig = BulkAutoTileConfig.LoadFromFile(AutoTileConfigPath);

    BulkAutoTileDrawerComposer bulkAutoTileDrawerComposer = new(
      bulkAutoTileConfig,
      imagesDirectory,
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

    return benchmarkInstance.GetResult();
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
