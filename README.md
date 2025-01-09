# BulkAutoTile

Godot implementation of an autotile algorithm for tilemaps with JSON configuration.

## Why?

Godot's terrain implementation works well for static maps that do not change during gameplay. However, it is [_extremely_](#benchmarks) slow for use cases like rendering chunks at runtime or generating large portions of the tilemap.

This library is designed to solve this issue for projects that require runtime autotile generation.

## Features

- **Ready-to-use Godot implementation**
- **Bulk AutoTiling**
- **Fully async compatible**
- **Multiple tile types can autotile with each other**
- **Much faster than Godot's terrain implementation** (see [benchmarks](#benchmarks))
- **Tile configuration** (includes bitmasks and connections)

## Benchmarks

Speed comparison between Godot's built-in terrain feature and this autotile implementation was performed using a simple benchmark (source code in the godot-example project directory).

[Result for 128x128 chunk](https://github.com/ruedoux/godot-autotile/tree/main/godot-example/benchmark-result-128.txt)

[Result for 256x256 chunk](https://github.com/ruedoux/godot-autotile/tree/main/godot-example/benchmark-result-256.txt)

## Installation

# Manual

Compile and copy the DLLs to `godot-example/addons/bulk-autotile/bin`:

```bash
# For Linux
./compile.sh

# For Windows
./compile.bat
```

After doing that you can copy the `godot-example/addons/bulk-autotile` folder from this repo into your project.

Add .csproj reference so Godot project sees the .dll's:

```xml
<ItemGroup>
  <Reference Include="BulkAutoTile">
    <HintPath>addons/bulk-autotile/bin/BulkAutoTile.dll</HintPath>
  </Reference>
</ItemGroup>
```
