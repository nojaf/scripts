dotnet new classlib -n App -o . -lang F#
dotnet add package Nojaf.Fable.React
dotnet restore
dotnet new tool-manifest
dotnet tool install fable
dotnet tool install fantomas
"module Library" | Out-File -FilePath "./Library.fs"
Set-Content -Path ".editorconfig" -Value "[*.fs]`nfsharp_experimental_elmish = true"