#load "./common.fsx"

open System
open System.IO
open System.Threading.Tasks
open CliWrap
open CliWrap.Buffered
open Common

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

type ProjectInfo =
    {
        FileInfo: FileInfo
        Files: ProjectFileInfo array
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

let processReports (outputFlag: string option) (project: FileInfo) =
    let regularCsvFile = Path.Combine(project.DirectoryName, "regular.csv") |> FileInfo
    let regularMap = mkReportMap regularCsvFile

    let fileIndexes =
        regularMap
        |> Array.filter (fun r ->
            r.Name = "CheckDeclarations.CheckOneSigFile"
            || r.Name = "CheckDeclarations.CheckOneImplFile"
        )
        |> Array.mapi (fun idx { FileName = fileName } -> idx, fileName.Value)

    let graphCsvFile = Path.Combine(project.DirectoryName, "graph.csv") |> FileInfo
    let graphMap = mkReportMap graphCsvFile

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
            idx,
            findByNameAndFile
                (set [| "CheckDeclarations.CheckOneImplFile"; "CheckDeclarations.CheckOneSigFile" |])
                fileName
                regularMap
        )
        |> dict

    let graphTypeCheckTimings =
        fileIndexes
        |> Array.map (fun (idx, fileName) ->
            idx,
            findByNameAndFile
                (set [| "CheckDeclarations.CheckOneImplFile"; "CheckDeclarations.CheckOneSigFile" |])
                fileName
                graphMap
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
                GraphTypeCheckDuration = graphTypeCheckTimings.[idx]
                DependencyCount = ownDependencies.Length
                DependentCount = edges |> Seq.filter (fun (_, depOf) -> depOf = idx) |> Seq.length
                LineCount = File.ReadLines(fileName) |> Seq.length
            }
        )

    { FileInfo = project; Files = files }

let exportProject (project: ProjectInfo) =
    let combinedPath = Path.Combine(project.FileInfo.DirectoryName, "combined.csv")

    let lines =
        [|
            yield
                "Idx, FileName, RegularTypeCheckDuration, GraphTypeCheckDuration, Delta (Graph - Regular), DependencyCount, DependentCount, LineCount"
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

                    let delta =
                        projectFileInfo.GraphTypeCheckDuration.Subtract(projectFileInfo.RegularTypeCheckDuration)

                    sprintf
                        "%d,%s,%s,%s,%s,%d,%d,%d"
                        projectFileInfo.Idx
                        fileName
                        (timeSpan projectFileInfo.RegularTypeCheckDuration)
                        (timeSpan projectFileInfo.GraphTypeCheckDuration)
                        (timeSpan delta)
                        projectFileInfo.DependencyCount
                        projectFileInfo.DependentCount
                        projectFileInfo.LineCount
                )
        |]

    File.WriteAllLines(combinedPath, lines)

let getProject (project: FileInfo) : Task<ProjectInfo> =
    task {
        printfn $"Processing %s{project.FullName}"
        let! rspFile = createResponseFile project

        // Create a copy to later add new signatures to.
        let extraFile = Path.Combine(project.DirectoryName, "Extra.rsp") |> FileInfo

        if not extraFile.Exists then
            File.Copy(rspFile.FullName, extraFile.FullName, true)

        // Clean up old files
        for csvFile in project.Directory.EnumerateFiles("*.csv") do
            csvFile.Delete()

        // The graph will be located next to the --output file
        let outputFlag = tryFindOutputFlag rspFile
        let! _ = compileFSharpProject rspFile "--times:regular.csv"
        let! _ = compileFSharpProject rspFile $"{experimentalCompilerFlags} --times:graph.csv"
        let projectInfo = processReports outputFlag project
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

// @"C:\Users\nojaf\Projects\FsLexYacc\src\FsYacc\fsyacc.fsproj"
let fantomasCore =
    @"C:\Users\nojaf\Projects\main-fantomas\src\Fantomas.Core\Fantomas.Core.fsproj"
    |> FileInfo
    |> getProject
    |> Task.RunSynchronously

fantomasCore.Files


@"C:\Users\nojaf\Projects\Graphoscope\src\Graphoscope\Graphoscope.fsproj"
|> FileInfo
|> getProject
|> Task.RunSynchronously
