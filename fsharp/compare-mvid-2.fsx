#r "nuget: CliWrap, 3.6.0"
#r "nuget: System.Reflection.Metadata"

open System
open System.IO
open System.Reflection.Metadata
open System.Reflection.PortableExecutable

let getMvid refDll =
    use embeddedReader = new PEReader(File.OpenRead refDll)
    let sourceReader = embeddedReader.GetMetadataReader()
    let loc = sourceReader.GetModuleDefinition().Mvid
    let mvid = sourceReader.GetGuid(loc)
    // printfn "%s at %s" (mvid.ToString()) (DateTime.Now.ToString())
    mvid

open CliWrap

let project = @"C:\Users\nojaf\Projects\graph-sample\GraphSample.fsproj"

let output =
    @"C:\Users\nojaf\Projects\graph-sample\obj\Debug\net7.0\GraphSample.dll"

let fsc =
    @"C:\Users\nojaf\Projects\safesparrow-fsharp\artifacts\bin\fsc\Release\net7.0\win-x64\publish\fsc.dll"

let runs =
    [ 0..50 ]
    |> List.map (fun idx ->
        Cli
            .Wrap("dotnet")
            .WithArguments(
                // --test:ParallelIlxGen
                $"build {project} /p:Deterministic=True /p:DotnetFscCompilerPath=\"{fsc}\" /p:OtherFlags=\"--test:GraphBasedChecking --test:DumpCheckingGraph\" --no-incremental"
            )
            .ExecuteAsync()
            .Task.Wait()

        printfn "Done with %i" idx

        getMvid output
    )

printfn "%A" runs
