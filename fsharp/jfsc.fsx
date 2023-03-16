#r "nuget: CliWrap, 3.6.0"

open System
open System.IO
open System.Xml.Linq
open CliWrap
open CliWrap.Buffered

let (</>) a b = Path.Combine(a, b) |> Path.GetFullPath

let editXmlValue (path: string) (node: string) (value: string) =
    let xdoc = XDocument.Load(path)

    xdoc.Root.Descendants(XName.op_Implicit node)
    |> Seq.tryExactlyOne
    |> Option.iter (fun element ->
        element.Value <- value
        xdoc.Save(path)
    )

[<RequireQualifiedAccess>]
module Settings =
    let projects = @"C:\Users\nojaf\Projects"
    let jfcsRoot = @"C:\Users\nojaf\Projects\jfcs"

    let resharperFSharpRoot =
        @"C:\Users\nojaf\Projects\resharper-fsharp\ReSharper.FSharp"

    let branchName = "net231"
    let fcsFsproj = jfcsRoot </> "src/Compiler/FSharp.Compiler.Service.fsproj"
    let nuspec = jfcsRoot </> "src/Compiler/JetBrains.FSharp.Compiler.Service.nuspec"
    let artifacts = jfcsRoot </> "artifacts"
    let publishedPackages = artifacts </> "packages/Debug/Shipping"
    let newVersion = "2023.1.77"
    let newVersionDev = $"{newVersion}-dev"

    let directoryBuildProps = resharperFSharpRoot </> "Directory.Build.props"

// Clone JetBrains/fsharp
if not (Path.Exists(Settings.jfcsRoot)) then
    Cli
        .Wrap("gh")
        .WithWorkingDirectory(Settings.projects)
        .WithArguments($"repo clone JetBrains/fsharp jfcs -- --single-branch --branch {Settings.branchName}")
        .ExecuteAsync()
        .Task.Wait()

// Tweak the fsproj and nuspec files to use a new version
editXmlValue Settings.fcsFsproj "VersionPrefix" Settings.newVersion
editXmlValue Settings.fcsFsproj "NuspecFile" "JetBrains.FSharp.Compiler.Service.nuspec"
editXmlValue Settings.nuspec "version" Settings.newVersion

if Path.Exists Settings.artifacts then
    Directory.Delete(Settings.jfcsRoot </> "artifacts")

// Create the Nuget packages
Cli
    .Wrap("Build.cmd")
    .WithWorkingDirectory(Settings.jfcsRoot)
    .WithArguments("-noVisualStudio -pack -c Debug")
    .ExecuteAsync()
    .Task.Wait()

// Clean existing NuGet caches
let nugetCaches =
    Cli
        .Wrap("dotnet")
        .WithArguments("nuget locals all -l")
        .ExecuteBufferedAsync()
        .Task.Result.StandardOutput.Split(Environment.NewLine)
    |> Array.choose (fun line ->
        let parts = line.Split(": ")
        if parts.Length <> 2 then None else Some(parts.[1].Trim())
    )

for source in nugetCaches do
    let localVersion =
        source </> "jetbrains.fsharp.compiler.service" </> Settings.newVersionDev

    if Path.Exists localVersion then
        Directory.Delete(localVersion, true)

// Add local nuget source
Cli
    .Wrap("dotnet")
    .WithWorkingDirectory(Settings.resharperFSharpRoot)
    .WithArguments($"nuget add source --name jfcs --configfile ./nuget.config \"{Settings.publishedPackages}\"")
    .ExecuteAsync()
    .Task.Wait()

// Update Directory.Build.props
editXmlValue Settings.directoryBuildProps "FSharpCompilerServiceVersion" Settings.newVersionDev
