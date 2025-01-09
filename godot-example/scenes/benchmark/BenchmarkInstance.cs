using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using Godot;

namespace BulkAutoTileExample;


[JsonSourceGenerationOptions(
  DefaultIgnoreCondition = JsonIgnoreCondition.Never,
  IncludeFields = true)]
[JsonSerializable(typeof(BenchmarkConfig))]
public partial class BenchmarkConfigJsonContext : JsonSerializerContext { }

public record BenchmarkConfig(
  string OutputPath,
  string ImagesDirectory,
  string AutoTileConfigPath,
  int ChunkX,
  int ChunkY,
  int BatchSize,
  int RepeatCount);

public partial class BenchmarkInstance : Node2D
{
  public long AverageTimeMs { private set; get; }
  public long MedianTimeMs { private set; get; }
  public long MaxTimeMs { private set; get; }
  public long MinTimeMs { private set; get; }

  public readonly int Repeats;
  private readonly Action benchmarkFunction;
  private readonly Action afterFunction;

  public BenchmarkInstance(Action benchmarkFunction, Action afterFunction, int repeats)
  {
    this.benchmarkFunction = benchmarkFunction;
    this.afterFunction = afterFunction;
    this.Repeats = repeats;
  }

  public void Run()
  {
    List<long> results = new();
    for (int i = 0; i < Repeats; i++)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      benchmarkFunction();
      stopwatch.Stop();
      results.Add(stopwatch.ElapsedMilliseconds);
      afterFunction();
    }

    if (results.Count > 0)
    {
      AverageTimeMs = (long)results.Average();
      MaxTimeMs = results.Max();
      MinTimeMs = results.Min();

      results.Sort();
      if (results.Count % 2 == 0)
        MedianTimeMs = (results[results.Count / 2 - 1] + results[results.Count / 2]) / 2;
      MedianTimeMs = results[results.Count / 2];
    }
  }

  public string GetResult()
  {
    return $"""
    [RUN RESULT] {benchmarkFunction.Method.Name}
    -> Average time: {AverageTimeMs}ms
    -> Median time: {MedianTimeMs}ms
    -> Max time: {MaxTimeMs}ms
    -> Min time: {MinTimeMs}ms
    """;
  }
}