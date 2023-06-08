// Configuration

open System.Collections.Concurrent
open System.Collections.Generic

let dotnet = @"C:\Program Files\dotnet\dotnet.exe"
let fscDll = @"C:\Program Files\dotnet\sdk\7.0.400-preview.23260.7\FSharp\fsc.dll"

#r "nuget: System.Collections.Immutable, 7.0.0"

open System
open System.Collections.Immutable
open System.IO

// Add the printer to F# FSI
fsi.AddPrinter(fun (ts: TimeSpan) -> $"%02d{ts.Minutes}:%02d{ts.Seconds}.%03d{ts.Milliseconds}")

module Seq =
    let inline toImmutableArray (values: 'a seq) : ImmutableArray<'a> = ImmutableArray.CreateRange(values)

type Path = ImmutableHashSet<int>
type PathWithTimings = ImmutableHashSet<int * TimeSpan>

let outputFlag (rspFile: FileInfo) =
    File.ReadAllLines(rspFile.FullName)
    |> Array.tryPick (fun line ->
        let line = line.Trim()

        if not (line.StartsWith("-o:")) && not (line.StartsWith("--output:")) then
            None
        else
            Some(line.Replace("--output:", "").Replace("-o:", ""))
    )

let memoize f =
    let dict = Dictionary<_, _>()

    fun c ->
        let exist, value = dict.TryGetValue c

        match exist with
        | true -> value
        | _ ->
            let value = f c
            dict.Add(c, value)
            value

let processProject (fsprojFile: FileInfo) : ImmutableArray<PathWithTimings> =
    // Create the rsp file
    let rspFile = Path.ChangeExtension(fsprojFile.FullName, ".rsp") |> FileInfo

    if not rspFile.Exists then
        failwith $"No rsp file was found for {fsprojFile.FullName}"

    let timingCsv = Path.Combine(fsprojFile.DirectoryName, "regular.csv") |> FileInfo

    if not timingCsv.Exists then
        failwith "No regular.csv file was found"

    let graph =
        match outputFlag rspFile with
        | None -> failwith $"no --output flag found in %s{rspFile.FullName}"
        | Some outputFlag ->

        let outputPath = Path.GetFullPath(outputFlag, fsprojFile.DirectoryName)
        Path.ChangeExtension(outputPath, ".graph.md") |> FileInfo

    assert graph.Exists

    let timings =
        File.ReadAllLines(timingCsv.FullName)
        |> Array.choose (fun line ->
            if not (line.StartsWith("ParseAndCheckInputs.CheckOneInput")) then
                None
            else

            let columns = line.Split(',')
            let duration = columns.[3] |> float |> TimeSpan.FromSeconds
            Some duration
        )
        |> Array.indexed
        |> dict

    let lastIndex = timings.Keys |> Seq.max

    let allLinks =
        File.ReadAllLines(graph.FullName)
        |> Array.choose (fun line ->
            if not (line.Contains("-->")) then
                None
            else

            let columns = line.Split("-->")
            let dest = columns.[0].Trim() |> int
            let src = columns.[1].Trim() |> int
            Some(src, dest)
        )

    let allDest = allLinks |> Array.map snd |> set

    // Start with dep free nodes
    let depFreeNodes =
        [| 0..lastIndex |]
        |> Array.Parallel.choose (fun idx -> if allDest.Contains idx then None else Some idx)
        |> set

    printfn "Start nodes: %A" depFreeNodes
    let pathCache = ConcurrentDictionary<PathWithTimings, PathWithTimings array>()

    let rec traverse (path: PathWithTimings) (src: int) : PathWithTimings array =
        let currentPath =
            let timing = timings.[src]
            let segment = src, timing
            path.Add segment

        pathCache.GetOrAdd(
            currentPath,
            fun path ->
                let destinations =
                    allLinks |> Array.choose (fun (s, d) -> if src <> s then None else Some d)

                if Array.isEmpty destinations then
                    Array.singleton path
                else
                    destinations |> Array.collect (traverse path)
        )

    depFreeNodes
    |> Seq.collect (traverse PathWithTimings.Empty)
    |> fun allPaths -> allPaths.ToImmutableArray()

let totalPathTime (path: PathWithTimings) =
    (TimeSpan.Zero, path) ||> Seq.fold (fun acc (_, duration) -> acc + duration)

let printPath (path: PathWithTimings) : unit =
    let sum = totalPathTime path

    let path =
        path
        |> Seq.sortByDescending snd
        |> Seq.map (fun (idx, timing) ->
            let percentage = timing.TotalMilliseconds / sum.TotalMilliseconds * 100.
            $"%i{idx} (%.2f{Math.Round(percentage, 2)}%%)"
        )
        |> String.concat ", "

    printfn $"{sum}: %s{path}"

// This doesn't check if the signatureCandidates already is a sig file.
let whatIf (signatureCandidates: int seq) (path: PathWithTimings) : PathWithTimings =
    let signatureCandidates = set signatureCandidates

    path
    |> Seq.map (fun (idx, timing) ->
        if signatureCandidates.Contains idx then
            idx, TimeSpan.Zero
        else
            idx, timing
    )
    |> ImmutableHashSet.CreateRange

let FsAutoComplete =
    processProject (
        FileInfo @"C:\Users\nojaf\Projects\FsAutoComplete\src\FsAutoComplete.Core\FsAutoComplete.Core.fsproj"
    )

FsAutoComplete
|> Seq.map (whatIf [ 35; 19 ]) //; 5; 19; 33; 27 ])
|> Seq.sortByDescending totalPathTime
|> Seq.take 10
|> Seq.iter printPath

FsAutoComplete.Length
