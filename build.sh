#!/bin/sh
rm -rf artifacts/*.*
rm -rf **/bin
rm -rf **/obj
mkdir tools
mkdir tools/nuget
curl -o ./tools/nuget/nuget.exe -k https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
mono tools/nuget/nuget.exe update -self
mono tools/nuget/nuget.exe install Cake -OutputDirectory tools -ExcludeVersion

mono tools/Cake/Cake.exe build.cake
