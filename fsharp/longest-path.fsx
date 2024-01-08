#r "nuget: FSharp.Data, 6.3.0"
#r "nuget: Graphoscope, 0.6.0-preview.1"
#load "./common.fsx"

open System
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions
open FSharp.Data
open Graphoscope
open Graphoscope.Measures
open Common

// Add the printer to F# FSI
fsi.AddPrinter(fun (ts: TimeSpan) -> $"%02d{ts.Minutes}:%02d{ts.Seconds}.%03d{ts.Milliseconds}")

type LongestPathResult =
    {
        LongestPath: NodeInfo list
        Duration: float
        Graph: FGraph<int, int, TimeSpan>
        FileNames: IDictionary<int, string>
    }

    member this.SlowestFile =
        this.LongestPath
        |> List.take (this.LongestPath.Length - 1)
        |> List.filter (fun n -> n.FileName.EndsWith(".fs", StringComparison.Ordinal))
        |> List.maxBy (fun n -> n.Duration.TotalMilliseconds)

    member this.MermaidStyle =
        this.LongestPath
        |> List.map (fun n -> $"style %i{n.Idx} fill:#f64747,color:#FFF")
        |> String.concat "\n"
        |> sprintf "%%%% Longest path styling\n%s"

and NodeInfo =
    {
        Idx: int
        FileName: string
        Duration: TimeSpan
    }

/// <param name="projectDirectory">Directory where the fsproj resides.</param>
let processReportAndGraph (projectDirectory: DirectoryInfo) (timingCsv: FileInfo) (graph: FileInfo) =
    let graphLines = File.ReadAllLines(graph.FullName)

    let fileNamesMap =
        let parseInput (input: string) =
            let pattern = @"\s*(\d+)\[""(.+?)""\]"
            let m = Regex.Match(input, pattern)

            if m.Success then
                let index = int m.Groups.[1].Value
                let name = m.Groups.[2].Value
                let fullPath = Path.Combine(projectDirectory.FullName, name)
                Some(index, fullPath)
            else
                None

        graphLines |> Array.choose parseInput |> dict

    if not timingCsv.Exists then
        failwith $"No timing csv found at %s{timingCsv.FullName}"

    let timings =
        use csvFile = CsvFile.Load(timingCsv.FullName).Cache()

        let csvLines =
            csvFile.Rows
            |> Seq.choose (fun row ->
                let name = row.GetColumn "Name"

                if
                    not (name = "CheckDeclarations.CheckOneImplFile")
                    && not (name = "CheckDeclarations.CheckOneSigFile")
                then
                    None
                else

                let duration = row.GetColumn "Duration(s)" |> float |> TimeSpan.FromSeconds
                let fileName = row.GetColumn "fileName"

                fileNamesMap
                |> Seq.tryFind (fun (KeyValue(_, f)) -> f = fileName)
                |> Option.map (fun kv -> kv.Key, duration)
            )
            |> Seq.sortBy fst
            |> Seq.toArray

        assert (csvLines.Length = graphLines.Length)

        csvLines |> dict

    assert (fileNamesMap.Count = timings.Count)

    let lastIndex = timings.Keys |> Seq.max

    let allLinks =
        graphLines
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
    let startNodes =
        [| 0..lastIndex |]
        |> Array.Parallel.choose (fun idx -> if allDest.Contains idx then None else Some idx)
        |> set

    let virtualStartNode = -1

    let fGraph =
        FGraph.empty
        |> FGraph.addNode virtualStartNode virtualStartNode
        |> FGraph.addNodes (Array.init timings.Count (fun i -> i, i))
        |> FGraph.addEdges
            [
                for sn in startNodes do
                    yield (virtualStartNode, sn, timings.[sn])

                for src, dest in allLinks do
                    yield (src, dest, timings.[dest])
            ]

    let longestPath, duration =
        LongestPath.getLongestPathOfFGraph virtualStartNode (fun (t: TimeSpan) -> t.TotalMilliseconds) fGraph

    let longestPath =
        longestPath
        |> List.filter (fun idx -> idx > -1)
        |> List.map (fun idx ->
            {
                Idx = idx
                FileName = fileNamesMap.[idx]
                Duration = timings.[idx]
            }
        )

    {
        LongestPath = longestPath
        Duration = duration
        Graph = fGraph
        FileNames = fileNamesMap
    }

let processProject (fsprojFile: FileInfo) =
    // Create the rsp file
    let rspFile = Path.ChangeExtension(fsprojFile.FullName, ".rsp") |> FileInfo

    if not rspFile.Exists then
        failwith $"No rsp file was found for {fsprojFile.FullName}"

    let graph =
        match tryFindOutputFlag rspFile with
        | None -> failwith $"no --output flag found in %s{rspFile.FullName}"
        | Some outputFlag ->

        let outputPath = Path.GetFullPath(outputFlag, fsprojFile.DirectoryName)
        Path.ChangeExtension(outputPath, ".graph.md") |> FileInfo

    assert graph.Exists

    let timingCsv = Path.Combine(fsprojFile.DirectoryName, "graph.csv") |> FileInfo

    if not timingCsv.Exists then
        failwith $"No timing csv found at %s{timingCsv.FullName}"

    processReportAndGraph fsprojFile.Directory timingCsv graph

// let project =
//     processReportAndGraph
//         (DirectoryInfo(@"C:\Users\nojaf\Projects\fsac-top-level-types\src\FsAutoComplete.Core"))
//         (FileInfo(@"C:\Users\nojaf\Projects\fsac-top-level-types\src\FsAutoComplete.Core\report.csv"))
//         (FileInfo(
//             @"C:\Users\nojaf\Projects\fsac-top-level-types\src\FsAutoComplete.Core\obj\Debug\net6.0\FsAutoComplete.Core.graph.md"
//         ))

let project =
    processReportAndGraph
        (DirectoryInfo(@"C:\Users\nojaf\Projects\resharper-fsharp\ReSharper.FSharp\src\FSharp\FSharp.Common"))
        (FileInfo(@"C:\Users\nojaf\Projects\resharper-fsharp\ReSharper.FSharp\src\FSharp\FSharp.Common\report.csv"))
        (FileInfo(
            @"C:\Users\nojaf\Projects\resharper-fsharp\ReSharper.FSharp\src\FSharp\FSharp.Common\obj\Debug\net472\JetBrains.ReSharper.Plugins.FSharp.Common.graph.md"
        ))

project.LongestPath
|> List.sortByDescending (fun ni -> ni.Duration.TotalMilliseconds)
|> List.iter (fun ni -> printfn "%A | %s" ni.Duration ni.FileName)

List.last project.LongestPath

project.Duration
project.SlowestFile

printfn "%s" project.MermaidStyle

project.SlowestFile
