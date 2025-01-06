cd lib
dotnet build -c Release
cd ..
mkdir godot-example/addons/bulk-autotile/bin
cp lib/bin/Release/net8.0/* godot-example/addons/bulk-autotile/bin/