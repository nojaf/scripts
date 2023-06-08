﻿// Configuration
let dotnet = @"C:\Program Files\dotnet\dotnet.exe"
let fscDll = @"C:\Program Files\dotnet\sdk\7.0.400-preview.23272.51\FSharp\fsc.dll"

#r "nuget: CliWrap, 3.6.0"
#r "System.Security.Cryptography"

open System
open System.IO
open System.Threading.Tasks
open CliWrap
open CliWrap.Buffered
open Microsoft.FSharp.Collections

// Add the printer to F# FSI
fsi.AddPrinter(fun (ts: TimeSpan) -> $"%02d{ts.Minutes}:%02d{ts.Seconds}.%03d{ts.Milliseconds}")
fsi.AddPrinter(fun (d: DateTime) -> d.ToShortTimeString())
fsi.AddPrinter(fun (fi: FileInfo) -> fi.FullName)

module Task =
    let RunSynchronously (task: Task<'a>) = task.Result

type ProjectFileInfo =
    {
        Idx: int
        FileName: string
        RegularTypeCheckDuration: TimeSpan
        GraphTypeCheckDuration: TimeSpan
        /// Own deps
        DependencyCount: int
        /// Being depended on
        DependentCount: int
        LineCount: int
    }

[<RequireQualifiedAccess>]
[<CustomEquality; NoComparison>]
type BinaryHashResult =
    | OutputFileNotFound
    | Hash of value: string * lastWriteTime: DateTime

    override x.Equals y =
        match x, y with
        | Hash(value = xv), (:? BinaryHashResult as y) ->
            match y with
            | Hash(value = yv) -> xv = yv
            | _ -> false
        | _ -> false

    override x.GetHashCode() =
        match x with
        | OutputFileNotFound -> -1
        | Hash(value = value) -> hash value

type ProjectInfo =
    {
        FileInfo: FileInfo
        Files: ProjectFileInfo array
        RegularCompilationDuration: TimeSpan
        RegularTypeCheckDuration: TimeSpan
        GraphCompilationDuration: TimeSpan
        GraphTypeCheckDuration: TimeSpan
        RegularBinaryHash: BinaryHashResult
        GraphTypeCheckBinaryHash: BinaryHashResult
    }

type ReportRow =
    {
        Name: string
        StartTime: DateTime
        EndTime: DateTime
        Duration: TimeSpan
        FileName: string option
    }

let mkReportMap (csvFile: FileInfo) =
    File.ReadAllLines(csvFile.FullName)
    |> Array.skip 1
    |> Array.map (fun line ->
        try
            let columns = line.Split(',')
            let name = columns.[0]
            let duration = columns.[3] |> float |> TimeSpan.FromSeconds

            let fileName =
                if columns.Length < 8 then
                    None
                elif String.IsNullOrWhiteSpace columns.[7] then
                    None
                else
                    let name = columns.[7]
                    // "obj\Debug\net472\.NETFramework,Version=v4.7.2.AssemblyAttributes.fs" has a comma in the name, but is wrapped in quotes.
                    if not (name.StartsWith('"')) then
                        Some name
                    else
                        Some($"%s{columns.[7]},%s{columns.[8]}".Trim('"'))

            let parseDate str =
                DateTime.ParseExact(str, "HH-mm-ss.ffff", System.Globalization.CultureInfo.InvariantCulture)

            {
                Name = name
                StartTime = parseDate columns.[1]
                EndTime = parseDate columns.[2]
                Duration = duration
                FileName = fileName
            }
        with ex ->
            printfn $"Could not process:\n%s{line}"
            raise ex
    )

let getDuration (row: ReportRow) = row.Duration

let findTotalTime (records: ReportRow array) =
    // This isn't always present for some reason...
    let fscCompliation =
        records |> Array.tryFind (fun { Name = name } -> name = "FSC compilation")

    match fscCompliation with
    | Some record -> record.Duration
    | None ->
        let minTime = records |> Array.map (fun { StartTime = time } -> time) |> Array.min
        let maxTime = records |> Array.map (fun { EndTime = time } -> time) |> Array.max
        maxTime - minTime

let findTypeCheck (csvFile: FileInfo) (records: ReportRow array) =
    let record = records |> Array.tryFind (fun { Name = name } -> name = "Typecheck")

    match record with
    | Some record -> getDuration record
    | None ->
        printfn $"Did not find any 'TypeCheck' record in %A{csvFile}"
        TimeSpan.Zero

let findByNameAndFile (names: Set<string>) fileName (records: ReportRow array) =
    let record =
        records
        |> Array.tryFind (fun record ->
            match record with
            | {
                  Name = name
                  FileName = Some fileName'
              } -> names.Contains name && fileName = fileName'
            | _ -> false
        )

    match record with
    | Some record -> getDuration record
    | None ->
        printfn $"Did not find any %A{names} duration for %s{fileName}"
        TimeSpan.Zero

let getFileHash filename =
    use sha256 = System.Security.Cryptography.SHA256.Create()
    use stream = File.OpenRead(filename)
    let hash = sha256.ComputeHash(stream)
    BitConverter.ToString(hash).Replace("-", "")

let processReports
    (outputFlag: string option)
    (regularBinaryHash: BinaryHashResult)
    (graphBasedBinaryHash: BinaryHashResult)
    (project: FileInfo)
    =
    let regularCsvFile = Path.Combine(project.DirectoryName, "regular.csv") |> FileInfo
    let regularMap = mkReportMap regularCsvFile

    let regularCompilationDuration, regularTypeCheckDuration =
        findTotalTime regularMap, findTypeCheck regularCsvFile regularMap

    let fileIndexes =
        regularMap
        |> Array.filter (fun r -> r.Name = "ParseAndCheckInputs.CheckOneInput")
        |> Array.mapi (fun idx { FileName = fileName } -> idx, fileName.Value)

    let graphCsvFile = Path.Combine(project.DirectoryName, "graph.csv") |> FileInfo
    let graphMap = mkReportMap graphCsvFile

    let graphCompilationDuration, graphTypeCheckDuration =
        findTotalTime graphMap, findTypeCheck graphCsvFile graphMap

    let graphMarkdown =
        match outputFlag with
        | None ->
            printfn "No -o flag found for %A" project
            None
        | Some outputFlag ->
            let outputPath = Path.GetFullPath(outputFlag, project.DirectoryName)
            let graphPath = Path.ChangeExtension(outputPath, ".graph.md") |> FileInfo
            if graphPath.Exists then Some graphPath else None

    if Option.isNone graphMarkdown then
        printfn "%A did not produce a graph" project

    let edges =
        match graphMarkdown with
        | None -> Array.empty
        | Some graphMarkdown ->
            File.ReadAllLines(graphMarkdown.FullName)
            |> Array.choose (fun line ->
                if not (line.Contains("-->")) then
                    None
                else
                    let parts = line.Split("-->")

                    if parts.Length <> 2 then
                        None
                    else
                        Some(int parts.[0], int parts.[1])
            )

    let regularTypeCheckTimings =
        fileIndexes
        |> Array.map (fun (idx, fileName) ->
            idx, findByNameAndFile (Set.singleton "ParseAndCheckInputs.CheckOneInput") fileName regularMap
        )
        |> dict

    let dependencies =
        fileIndexes
        |> Array.map (fun (idx, _) ->
            idx, edges |> Array.choose (fun (from, dep) -> if from = idx then Some dep else None)
        )
        |> dict

    let files =
        fileIndexes
        |> Array.map (fun (idx, fileName) ->
            let ownDependencies: int array = dependencies.[idx]

            {
                Idx = idx
                FileName = fileName
                RegularTypeCheckDuration = regularTypeCheckTimings.[idx]
                GraphTypeCheckDuration =
                    findByNameAndFile
                        (set [| "CheckDeclarations.CheckOneImplFile"; "CheckDeclarations.CheckOneSigFile" |])
                        fileName
                        graphMap
                DependencyCount = ownDependencies.Length
                DependentCount = edges |> Seq.filter (fun (_, depOf) -> depOf = idx) |> Seq.length
                LineCount = File.ReadLines(fileName) |> Seq.length
            }
        )

    {
        FileInfo = project
        Files = files
        RegularCompilationDuration = regularCompilationDuration
        RegularTypeCheckDuration = regularTypeCheckDuration
        GraphCompilationDuration = graphCompilationDuration
        GraphTypeCheckDuration = graphTypeCheckDuration
        RegularBinaryHash = regularBinaryHash
        GraphTypeCheckBinaryHash = graphBasedBinaryHash
    }

let exportProject (project: ProjectInfo) =
    let combinedPath = Path.Combine(project.FileInfo.DirectoryName, "combined.csv")

    let lines =
        [|
            yield
                "Idx, FileName, RegularTypeCheckDuration, GraphTypeCheckDuration, DependencyCount, DependentCount, LineCount, WorldClockTime, AllDependencies"
            yield!
                project.Files
                |> Array.map (fun projectFileInfo ->
                    let fileName =
                        if not (projectFileInfo.FileName.Contains(',')) then
                            projectFileInfo.FileName
                        else
                            $"\"{projectFileInfo.FileName}\""

                    let timeSpan (ts: TimeSpan) =
                        ts.TotalSeconds.ToString("000.0000", System.Globalization.CultureInfo.InvariantCulture)

                    sprintf
                        "%d,%s,%s,%s,%d,%d,%d"
                        projectFileInfo.Idx
                        fileName
                        (timeSpan projectFileInfo.RegularTypeCheckDuration)
                        (timeSpan projectFileInfo.GraphTypeCheckDuration)
                        projectFileInfo.DependencyCount
                        projectFileInfo.DependentCount
                        projectFileInfo.LineCount
                )
        |]

    File.WriteAllLines(combinedPath, lines)

let sanitizeRspFile (rspFile: FileInfo) =
    let filteredLines =
        File.ReadAllLines rspFile.FullName
        |> Seq.filter (fun line -> not (line.TrimStart().StartsWith("--warnaserror")))

    File.WriteAllLines(rspFile.FullName, filteredLines)

let getProject (project: FileInfo) : Task<ProjectInfo> =
    task {
        printfn $"Processing %s{project.FullName}"
        let rspFile = FileInfo(Path.ChangeExtension(project.FullName, ".rsp"))

        if rspFile.Exists then
            printfn "Skipping generating response file"
        else
            do!
                Cli
                    .Wrap("telplin")
                    .WithArguments($"--only-record {project.Name} -- -c Release")
                    .WithWorkingDirectory(project.DirectoryName)
                    .WithStandardErrorPipe(PipeTarget.ToDelegate(printfn "%s"))
                    .ExecuteAsync()
                    .Task
                :> Task
            // Clean up any unwanted settings
            sanitizeRspFile rspFile
        // Clean up old files
        for csvFile in project.Directory.EnumerateFiles("*.csv") do
            csvFile.Delete()

        let rspFileName = rspFile.Name
        // The graph will be located next to the --output file
        let outputFlag =
            File.ReadAllLines(rspFile.FullName)
            |> Array.tryPick (fun line ->
                let line = line.Trim()

                if not (line.StartsWith("-o:")) && not (line.StartsWith("--output:")) then
                    None
                else
                    Some(line.Replace("--output:", "").Replace("-o:", ""))
            )

        let getBinaryHash () =
            match outputFlag with
            | None -> BinaryHashResult.OutputFileNotFound
            | Some output ->
                let outputFile = Path.Combine(rspFile.DirectoryName, output) |> FileInfo

                if not outputFile.Exists then
                    BinaryHashResult.OutputFileNotFound
                else
                    BinaryHashResult.Hash(getFileHash outputFile.FullName, outputFile.LastWriteTime)

        do!
            Cli
                .Wrap(dotnet)
                .WithArguments($"\"{fscDll}\" \"@{rspFileName}\" --times:regular.csv")
                .WithWorkingDirectory(project.DirectoryName)
                .WithStandardOutputPipe(PipeTarget.ToDelegate(printfn "%s"))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(printfn "%s"))
                .ExecuteAsync()
                .Task
            :> Task

        let regularHash = getBinaryHash ()

        do!
            Cli
                .Wrap(dotnet)
                .WithArguments(
                    $"\"{fscDll}\" \"@{rspFileName}\" --parallelreferenceresolution --test:GraphBasedChecking --test:DumpCheckingGraph --test:ParallelOptimization --test:ParallelIlxGen --times:graph.csv"
                )
                .WithWorkingDirectory(project.DirectoryName)
                .ExecuteAsync()
                .Task
            :> Task

        let graphBasedHash = getBinaryHash ()
        let projectInfo = processReports outputFlag regularHash graphBasedHash project
        exportProject projectInfo
        return projectInfo
    }

let getProjectsInSolution (slnFile: FileInfo) =
    task {
        let! result =
            Cli
                .Wrap(dotnet)
                .WithArguments($"sln {slnFile.FullName} list")
                .WithWorkingDirectory(slnFile.Directory.FullName)
                .ExecuteBufferedAsync()
                .Task

        let projects =
            result.StandardOutput.Split('\n')
            |> Array.choose (fun project ->
                let project = project.Trim()

                if not (project.EndsWith(".fsproj")) then
                    None
                else
                    Path.Combine(slnFile.Directory.FullName, project) |> FileInfo |> Some
            )

        return projects
    }

let getProjects (slnFile: FileInfo) =
    task {
        let! projects = getProjectsInSolution slnFile
        let projectInfos = Array.zeroCreate<ProjectInfo> projects.Length

        for pIdx = 0 to (projects.Length - 1) do
            printfn $"Processing project %i{pIdx + 1}/%i{projects.Length}"
            let project = projects.[pIdx]
            let! project = getProject project
            projectInfos.[pIdx] <- project

        return projectInfos
    }

let solutionDelta projects =
    let compareBy predicateName regularPredicate graphPredicate =
        let regularTime = projects |> Array.sumBy regularPredicate
        let graphTime = projects |> Array.sumBy graphPredicate
        printfn $"Delta by %s{predicateName}: %3f{(regularTime - graphTime) / regularTime * 100.0}%%"

    compareBy
        "compilation"
        (fun p -> p.RegularCompilationDuration.TotalMilliseconds)
        (fun p -> p.GraphCompilationDuration.TotalMilliseconds)

    compareBy
        "type-checking"
        (fun (p: ProjectInfo) -> p.RegularTypeCheckDuration.TotalMilliseconds)
        (fun p -> p.GraphTypeCheckDuration.TotalMilliseconds)

let processSolution (slnFile: FileInfo) =
    task {
        let! projects = getProjects slnFile
        solutionDelta projects
        return projects
    }

// let fantomasSln = FileInfo(@"C:\Users\nojaf\Projects\main-fantomas\fantomas.sln")
// let fantomasProjects = processSolution fantomasSln |> Task.RunSynchronously
// Array.iter exportProject fantomasProjects

// @"C:\Users\nojaf\Projects\FsLexYacc\src\FsYacc\fsyacc.fsproj"
@"C:\Users\nojaf\Projects\FsAutoComplete\src\FsAutoComplete.Core\FsAutoComplete.Core.fsproj"
|> FileInfo
|> getProject
|> Task.RunSynchronously
|> Array.singleton
|> solutionDelta
