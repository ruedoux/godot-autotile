# BulkAutoTile

Godot implementation of an autotile algorithm for tilemaps with JSON configuration.

## Why?

Godot's terrain implementation works well for static maps that do not change during gameplay. However, it is _extremely_ slow ([see benchmark results](#benchmarks)) for use cases like rendering chunks at runtime or generating large portions of the tilemap.

This library is designed to solve this issue for projects that require runtime autotile generation.

## Features

- **Bulk AutoTiling**
- **Ready-to-use Godot implementation**
- **Much faster than Godot's terrain implementation**
- **Fully async compatible**
- **Tile configuration** (includes bitmasks and connections)

## Benchmarks

Speed comparison between Godot's built-in terrain feature was performed using a simple benchmark (source code in the godot-example project).

Results for a 256x256 autotiled chunk (over 25 repeats):

- **Godot:**

  - Average time: 3355ms
  - Median time: 3389ms
  - Max time: 3444ms
  - Min time: 3254ms

- **BulkAutoTile:**
  - Average time: 98ms
  - Median time: 79ms
  - Max time: 181ms
  - Min time: 68ms

_NOTE:_ The above results were collected using a non-callable/async implementation of the algorithm. If used with async, it would be even faster!

## Installation

As the project is fairly bare-bones, it opts for hand compilation of DLLs and moving the `godot-example/addons/bulk-autotile` folder into your project.

Compile and copy the DLLs:

```bash
# For Linux
./compile.sh

# For Windows
./compile.bat
```

After doing that you can copy the `godot-example/addons/bulk-autotile` folder from this repo into your project.

Add .csproj reference so the Godot project sees the .dll's:

```xml
<ItemGroup>
  <Reference Include="BulkAutoTile">
    <HintPath>addons/bulk-autotile/bin/BulkAutoTile.dll</HintPath>
  </Reference>
</ItemGroup>
```
