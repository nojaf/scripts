#r "nuget: CliWrap, 3.6.0"

open CliWrap

let pwd = @"C:\Users\nojaf\Projects\safesparrow-fsharp\src\FSharp.Core"

let targets = [| "Rebuild" |] |> String.concat ";"

let properties =
    [|
        "DesignTimeBuild", "true"
        "BuildingProject", "false"
        "BuildProjectReferences", "false"
        "SkipCompilerExecution", "true"
        "DisableRarCache", "true"
        "AutoGenerateBindingRedirects", "false"
        "CopyBuildOutputToOutputDirectory", "false"
        "CopyOutputSymbolsToOutputDirectory", "false"
        "CopyDocumentationFileToOutputDirectory", "false"
        "ComputeNETCoreBuildOutputFiles", "false"
        "SkipCopyBuildProduct", "true"
        "AddModules", "false"
        "UseCommonOutputDirectory", "true"
        "GeneratePackageOnBuild", "false"
    |]
    |> Array.map (fun (k, v) -> $"{k}={v}")
    |> String.concat ";"


Cli
    .Wrap("dotnet")
    .WithArguments($"msbuild \"-t:{targets}\" \"-p:{properties}\" -v:n -bl")
    .WithWorkingDirectory(pwd)
    .WithValidation(CommandResultValidation.None)
    .ExecuteAsync()
    .Task.Wait()
