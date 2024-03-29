﻿#load "./common.fsx"
#r "nuget: FSharp.Stats, 0.4.12-preview.2"

open System
open System.Collections.Generic
open System.IO
open System.Threading.Tasks
open FSharp.Stats
open FSharp.Collections
open Common

type CompilationMode =
    | Regular = 0
    | Experimental = 1
    | ExperimentalSignatures = 2

type BenchmarkResult =
    {
        RegularAverage: TimeSpan
        ExperimentalAverage: TimeSpan
        ExperimentalSignaturesAverage: TimeSpan
        BinaryHash: string
    }

let getBinaryHash (rspFile: FileInfo) (outputFlag: string) =
    let outputFile = Path.Combine(rspFile.DirectoryName, outputFlag) |> FileInfo
    assert outputFile.Exists
    getFileHash outputFile.FullName, outputFile.LastWriteTime

let benchmarkProject (numberOfRuns: int) (rspFile: FileInfo) : Task<BenchmarkResult> =
    task {
        assert rspFile.Exists
        let extraFile = Path.Combine(rspFile.DirectoryName, "Extra.rsp") |> FileInfo
        assert extraFile.Exists

        let totalRunCount = 3 * numberOfRuns
        let results = ResizeArray<CompilationMode * TimeSpan> totalRunCount
        let mutable binaryHash = None

        let setBinaryHash hash =
            match binaryHash with
            | None -> binaryHash <- Some hash
            | Some currentHash ->
                if currentHash <> hash then
                    failwith "Binary hash mismatch"

        let binaryWriteTimes = HashSet<DateTime> totalRunCount

        let addBinaryHash () =
            tryFindOutputFlag rspFile
            |> Option.iter (fun outputFlag ->
                let binaryHash, lastWriteTime = getBinaryHash rspFile outputFlag
                setBinaryHash binaryHash
                binaryWriteTimes.Add lastWriteTime |> ignore
            )

        for run in [ 1..numberOfRuns ] do
            let! time = compileFSharpProject rspFile ""
            addBinaryHash ()
            printfn $"Regular run %i{run} in %A{time}"
            results.Add(CompilationMode.Regular, time)

        for run in [ 1..numberOfRuns ] do
            let! time = compileFSharpProject rspFile experimentalCompilerFlags
            addBinaryHash ()
            printfn $"Experimental run %i{run} in %A{time}"
            results.Add(CompilationMode.Experimental, time)

        // Don't assert the file hash of the binary, since it's not guaranteed to be the same
        for run in [ 1..numberOfRuns ] do
            let! time = compileFSharpProject extraFile experimentalCompilerFlags
            printfn $"ExperimentalSignatures run %i{run} in %A{time}"
            results.Add(CompilationMode.ExperimentalSignatures, time)

        let averageBy compilationMode =
            let timings =
                results
                |> Seq.choose (fun (mode, timing) ->
                    if mode = compilationMode then
                        Some timing.TotalMilliseconds
                    else
                        None
                )
                |> Seq.toArray

            let interval = Signal.Outliers.tukey 1.5 timings
            printfn "%A" interval

            timings
            |> Seq.filter (fun t -> Intervals.liesInInterval t interval)
            |> Seq.average
            |> TimeSpan.FromMilliseconds

        // Each compilation should have written a binary
        assert (binaryWriteTimes.Count = totalRunCount)

        return
            {
                RegularAverage = averageBy CompilationMode.Regular
                ExperimentalAverage = averageBy CompilationMode.Experimental
                ExperimentalSignaturesAverage = averageBy CompilationMode.ExperimentalSignatures
                BinaryHash = binaryHash.Value
            }
    }

let benchmarkProjectTypeChecking fscDll (numberOfRuns: int) (rspFile: FileInfo) : Task<TimeSpan> =
    task {
        assert rspFile.Exists
        let extraFile = Path.Combine(rspFile.DirectoryName, "Extra.rsp") |> FileInfo
        assert extraFile.Exists

        let totalRunCount = 3 * numberOfRuns
        let results = ResizeArray<TimeSpan> totalRunCount

        let csv = Path.Combine(rspFile.DirectoryName, "report.csv") |> FileInfo

        for idx in [ 1..numberOfRuns ] do
            if csv.Exists then
                csv.Delete()

            let! _ = compileFSharpProjectAux dotnet fscDll rspFile experimentalCompilerFlags

            let! lines = File.ReadAllLinesAsync csv.FullName

            let typeCheckTime =
                lines
                |> Array.pick (fun line ->
                    if not (line.Trim().StartsWith("Typecheck")) then
                        None
                    else

                    let columns = line.Split(',')
                    let duration = columns.[3] |> float |> TimeSpan.FromSeconds
                    Some duration
                )

            printfn $"Type-check %i{idx} in %A{typeCheckTime}"
            results.Add(typeCheckTime)

        let average =
            let timings = results |> Seq.map (fun t -> t.TotalMilliseconds) |> Seq.toArray
            let interval = Signal.Outliers.tukey 1.5 timings

            timings
            |> Seq.filter (fun t -> Intervals.liesInInterval t interval)
            |> Seq.average
            |> TimeSpan.FromMilliseconds

        return average
    }

let compilerProjRsp =
    @"C:\Users\nojaf\Projects\fantomas\src\Fantomas.Core\Fantomas.Core.rsp"
    |> FileInfo

benchmarkProjectTypeChecking fscPath.mainFSharp 10 compilerProjRsp
|> Task.RunSynchronously
|> printfn "MAIN: %A"

benchmarkProjectTypeChecking fscPath.localRelease 10 compilerProjRsp
|> Task.RunSynchronously
|> printfn "LOCAL: %A"

// FileInfo(@"C:\Users\nojaf\Projects\main-fantomas\src\Fantomas.Core\Fantomas.Core.rsp")
// |> benchmarkProject 5
// |> Task.RunSynchronously

(*
MAIN: 00:00:04.5423599
LOCAL: 00:00:04.5436700
*)
