// Configuration
/// The path to the dotnet executable of the preview SDK
let dotnet = @"C:\Program Files\dotnet\dotnet.exe"

/// The path to the fsc.dll of the preview SDK
let fscPath =
    {|
        ``8.0.100-rc.1`` = @"C:\Program Files\dotnet\sdk\8.0.100-rc.1.23455.8\FSharp\fsc.dll"
        localRelease = @"C:\Users\nojaf\Projects\fsharp\artifacts\bin\fsc\Release\net8.0\win-x64\publish\fsc.dll"
        mainFSharp = @"C:\Users\nojaf\Projects\main-fsharp\artifacts\bin\fsc\Release\net8.0\win-x64\publish\fsc.dll"
    |}

#r "nuget: CliWrap, 3.6.0"
#r "System.Security.Cryptography"

open System
open System.IO
open System.Threading.Tasks
open CliWrap
open Microsoft.FSharp.Collections

// Add the printer to F# FSI
fsi.AddPrinter(fun (ts: TimeSpan) -> $"%02d{ts.Minutes}:%02d{ts.Seconds}.%03d{ts.Milliseconds}")
fsi.AddPrinter(fun (d: DateTime) -> d.ToShortTimeString())
fsi.AddPrinter(fun (fi: FileInfo) -> fi.FullName)

module Task =
    let RunSynchronously (task: Task<'a>) = task.Result

/// Calculates the SHA256 hash of the given file.
let getFileHash filename =
    use sha256 = System.Security.Cryptography.SHA256.Create()
    use stream = File.OpenRead(filename)
    let hash = sha256.ComputeHash(stream)
    BitConverter.ToString(hash).Replace("-", "")

let private ignoredCompilerSettings =
    set
        [|
            "--warnaserror"
            "--test:GraphBasedChecking"
            "--test:ParallelOptimization"
            "--test:ParallelIlxGen"
        |]

let sanitizeRspFile (rspFile: FileInfo) =
    let filteredLines =
        File.ReadAllLines rspFile.FullName
        |> Seq.filter (fun line ->
            let line = line.TrimStart()
            ignoredCompilerSettings |> Seq.exists line.StartsWith |> not
        )

    File.WriteAllLines(rspFile.FullName, filteredLines)

/// Creates a response file for the given project using a globally installed Telplin.
let createResponseFile (fsproj: FileInfo) : Task<FileInfo> =
    task {
        let rspFile = FileInfo(Path.ChangeExtension(fsproj.FullName, ".rsp"))

        if rspFile.Exists then
            printfn "Skipping generating response file"
            return rspFile
        else

        do!
            Cli
                .Wrap("telplin")
                .WithArguments($"--only-record {fsproj.Name} -- -c Release")
                .WithWorkingDirectory(fsproj.DirectoryName)
                .WithStandardErrorPipe(PipeTarget.ToDelegate(printfn "%s"))
                .ExecuteAsync()
                .Task
            :> Task

        // Clean up any unwanted settings
        sanitizeRspFile rspFile

        return rspFile
    }

/// Tries to find the output file from the response file.
let tryFindOutputFlag (rspFile: FileInfo) : string option =
    File.ReadAllLines(rspFile.FullName)
    |> Array.tryPick (fun line ->
        let line = line.Trim()

        if not (line.StartsWith("-o:")) && not (line.StartsWith("--output:")) then
            None
        else
            Some(line.Replace("--output:", "").Replace("-o:", ""))
    )

let warn = "--warn:0 --nowarn:3370 --nowarn:3520 --nowarn:3365"

/// Compile F# project using a response file.
/// <remark>Use --warn:0</remark>
let compileFSharpProjectAux dotnet fscDll (responseFile: FileInfo) (flags: string) : Task<TimeSpan> =
    task {
        let! result =
            Cli
                .Wrap(dotnet)
                .WithArguments($"\"%s{fscDll}\" \"@{responseFile.Name}\" --nologo {warn} {flags}")
                .WithWorkingDirectory(responseFile.DirectoryName)
                .WithStandardOutputPipe(PipeTarget.ToDelegate(printfn "%s"))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(printfn "%s"))
                .ExecuteAsync()
                .Task

        return result.RunTime
    }

/// Compile F# project using a response file.
/// <remark>Use --warn:0</remark>
let compileFSharpProject (responseFile: FileInfo) (flags: string) : Task<TimeSpan> =
    compileFSharpProjectAux dotnet fscPath.``8.0.100-rc.1`` responseFile flags

/// Contains all the new compiler flags.
/// --test:GraphBasedChecking, --test:ParallelOptimization, ...
let experimentalCompilerFlags =
    "--parallelreferenceresolution --test:GraphBasedChecking --test:DumpCheckingGraph --test:ParallelOptimization --test:ParallelIlxGen"
