#r "nuget: CliWrap, 3.6.4"
#r "nuget: Argu, 6.1.1"

open System
open System.IO
open CliWrap
open Argu

let (</>) a b = Path.Combine(a, b)

let aliases =
    [|
        "express", [ "Glutinum.Express" ]
        "react", [ "Fable.React"; "Feliz.CompilerPlugins" ]
        "promise", [ "Fable.Promise" ]
        "fetch", [ "Fable.Fetch" ]
        "dom", [ "Fable.Browser.Dom" ]
        "thoth", [ "Thoth.Json" ]
        "elmish", [ "Feliz.UseElmish" ]
    |]
    |> Map.ofArray

type Arguments =
    | [<Unique; AltCommandLine "-n">] Name of string
    | Packages of string list
    | [<Unique; AltCommandLine "-o">] Output of string
    | [<Unique>] Debug

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Name _ -> "Name of the fsproj file"
            | Packages _ ->
                let aliasNames = aliases.Keys |> String.concat ", "
                $"Potential NuGet packages to add. Could also be an alias (%s{aliasNames})"
            | Output _ -> "Output directory, defaults to pwd"
            | Debug -> "Print debug messages"

let errorHandler =
    ProcessExiter(
        colorizer =
            function
            | ErrorCode.HelpText -> None
            | _ -> Some ConsoleColor.Red
    )

let parser =
    ArgumentParser.Create<Arguments>(programName = "dotnet fsi scaffold-fable.fsx", errorHandler = errorHandler)

let arguments =
    fsi.CommandLineArgs
    |> Array.filter (fun a -> not (a.EndsWith(".fsx", StringComparison.InvariantCulture)))

let results = parser.ParseCommandLine arguments

let outputDirectory =
    results.TryGetResult <@ Output @>
    |> Option.defaultValue (Directory.GetCurrentDirectory())
    |> DirectoryInfo

outputDirectory.Create()

let projectName = results.TryGetResult <@ Name @> |> Option.defaultValue "App"
let fsprojFile = outputDirectory.FullName </> $"%s{projectName}.fsproj"

let fsprojContents =
    """
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="App.fs" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Update="FSharp.Core" Version="7.0.400" />
  </ItemGroup>

</Project>
"""

File.WriteAllText(outputDirectory.FullName </> "App.fsproj", fsprojContents)

let appFile = outputDirectory.FullName </> "App.fs"

File.WriteAllText(
    appFile,
    """module App

open Fable.Core

JS.console.log "Works!"
"""
)

let addPackage packageName =
    Cli
        .Wrap("dotnet")
        .WithWorkingDirectory(outputDirectory.FullName)
        .WithArguments($"add package %s{packageName}")
        .ExecuteAsync()
        .Task.Result
    |> ignore

addPackage "Fable.Core"

let packages = results.TryGetResult <@ Packages @> |> Option.defaultValue []

for package in packages do
    match Map.tryFind package aliases with
    | Some aliasPackages -> List.iter addPackage aliasPackages
    | None -> addPackage package

Cli
    .Wrap("dotnet")
    .WithWorkingDirectory(outputDirectory.FullName)
    .WithArguments("restore")

printfn $"New Fable project generated at %s{fsprojFile}"
