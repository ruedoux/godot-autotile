@echo off

cd lib
dotnet build -c Release
cd ..
if not exist "godot-example\addons\bulk-autotile\bin" (
    mkdir "godot-example\addons\bulk-autotile\bin"
)
xcopy /Y /E "lib\bin\Release\net8.0\*" "godot-example\addons\bulk-autotile\bin\"