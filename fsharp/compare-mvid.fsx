#r "nuget: CliWrap, 3.6.0"
#r "nuget: System.Reflection.Metadata"

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

let argsFile =
    // FileInfo(@"C:\Users\nojaf\Projects\main-fantomas\src\Fantomas.Core.Tests\Fantomas.Core.Tests.args.txt")
    // FileInfo(@"C:\Users\nojaf\Projects\fsharp\src\Compiler\FSharp.Compiler.Service.args.txt")
    // FileInfo(@"C:\Users\nojaf\Projects\fsharp\src\FSharp.Core\FSharp.Core.args.txt")
    FileInfo(@"C:\Users\nojaf\Projects\graph-sample\GraphSample.args.txt")

let total = 50

[<RequireQualifiedAccess>]
type MvidResult<'mivd when 'mivd: equality> =
    | None
    | Stable of mvid: 'mivd * times: int
    | Unstable of initial: 'mivd * times: int * variant: 'mivd

for dll in
    Directory.EnumerateFiles(
        // @"C:\Users\nojaf\Projects\fsharp\artifacts\obj\FSharp.Core\Debug\netstandard2.0",
        // @"C:\Users\nojaf\Projects\fsharp\artifacts\obj\FSharp.Compiler.Service\Debug\netstandard2.0",
        @"C:\Users\nojaf\Projects\graph-sample\obj\Debug\net7.0",
        "*.dll"
    ) do
    File.Delete(dll)

let runs =
    (MvidResult.None, [ 1..total ])
    ||> List.fold (fun prevMvid idx ->
        match prevMvid with
        | MvidResult.Unstable _ -> prevMvid
        | _ ->

        try
            let args = $"@{argsFile.Name}"

            Cli
                .Wrap(
                    @"C:\Users\nojaf\Projects\safesparrow-fsharp\artifacts\bin\fsc\Release\net7.0\win-x64\publish\fsc.exe"
                )
                .WithWorkingDirectory(argsFile.DirectoryName)
                .WithArguments($"\"{args}\" --test:GraphBasedChecking --test:DumpCheckingGraph") // --debug-
                .ExecuteAsync()
                .Task.Wait()

            let binaryPath =
                let binaryPath = File.ReadAllLines(argsFile.FullName).[0].Replace("-o:", "")

                if File.Exists binaryPath then
                    binaryPath
                else
                    Path.Combine(argsFile.DirectoryName, binaryPath)

            let binary = FileInfo(binaryPath)
            let mvid = getMvid binary.FullName
            printfn "Compiled %02i, write date %A, mvid: %s" idx binary.LastWriteTime (mvid.ToString("N"))

            let differentPath =
                Path.Combine(binary.Directory.FullName, $"{Path.GetFileNameWithoutExtension(binary.Name)}-{idx}.dll")

            File.Move(binaryPath, differentPath)

            match prevMvid with
            | MvidResult.Unstable _
            | MvidResult.None _ -> MvidResult.Stable(mvid, 1)
            | MvidResult.Stable(prevMvid, times) ->
                if prevMvid <> mvid then
                    MvidResult.Unstable(prevMvid, times, mvid)
                else
                    MvidResult.Stable(prevMvid, times + 1)
        with ex ->
            printfn "%s" ex.Message
            prevMvid
    )

printfn "%A" runs
