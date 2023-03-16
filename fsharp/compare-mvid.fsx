#r "nuget: CliWrap, 3.6.0"
#r "nuget: System.Reflection.Metadata"
#r "System.Security.Cryptography"

open System
open System.IO
open System.Reflection.Metadata
open System.Reflection.PortableExecutable

let getMvid refDll =
    use embeddedReader = new PEReader(File.OpenRead refDll)
    let sourceReader = embeddedReader.GetMetadataReader()
    let loc = sourceReader.GetModuleDefinition().Mvid
    let mvid = sourceReader.GetGuid(loc)
    mvid

// Source: https://stackoverflow.com/questions/64393919/load-resource-from-assembly-without-loading-the-assembly
let getResourceFromBinary (startWith: string) dll =
    use embeddedReader = new PEReader(File.OpenRead dll)
    let sourceReader = embeddedReader.GetMetadataReader()

    sourceReader.ManifestResources
    |> Seq.tryPick (fun resHandle ->
        let resource = sourceReader.GetManifestResource(resHandle)

        if resource.Name.IsNil then
            None
        else

        let name = sourceReader.GetString(resource.Name)

        if not (name.StartsWith(startWith)) then
            None
        else

        let resourceDirectory =
            embeddedReader.GetSectionData(embeddedReader.PEHeaders.CorHeader.ResourcesDirectory.RelativeVirtualAddress)

        let reader =
            resourceDirectory.GetReader(int resource.Offset, resourceDirectory.Length - int resource.Offset)

        let size = reader.ReadUInt32()
        let resourceBytes = reader.ReadBytes(int size)
        Some resourceBytes
    )

let getFSCompResource = getResourceFromBinary "FSComp."

let geFSharpOptimizationDataResource =
    getResourceFromBinary "FSharpOptimizationData."

let getFSharpSignatureDataResource = getResourceFromBinary "FSharpSignatureData."
let getFSIStringsResource = getResourceFromBinary "FSIstrings."
let getFSStringsResource = getResourceFromBinary "FSStrings."
let getUtilsStringsResource = getResourceFromBinary "UtilsStrings."

let getFileHash filename =
    use sha256 = System.Security.Cryptography.SHA256.Create()
    use stream = File.OpenRead(filename)
    let hash = sha256.ComputeHash(stream)
    BitConverter.ToString(hash).Replace("-", "")

open CliWrap

let argsFile =
    // FileInfo(@"C:\Users\nojaf\Projects\main-fantomas\src\Fantomas.Core\Fantomas.Core.args.txt")
    // FileInfo(@"C:\Users\nojaf\Projects\fsharp\src\Compiler\FSharp.Compiler.Service.args.txt")
    // FileInfo(@"C:\Users\nojaf\Projects\fsharp\src\FSharp.Core\FSharp.Core.args.txt")
    // FileInfo(@"C:\Users\nojaf\Projects\graph-sample\GraphSample.args.txt")
    FileInfo(@"C:\Users\nojaf\Projects\main-fsharp\src\Compiler\FSharp.Compiler.Service.args.txt")
// FileInfo(@"C:\Users\nojaf\Projects\DeterminismSample\DeterminismSample.args.txt")

let total = 10

type CompilationResultInfo =
    {
        Mvid: Guid
        BinaryFileHash: string
        PdbFileHash: string option
        SignatureDataHash: string option
    }

    override x.ToString() =
        let mvid = x.Mvid.ToString("N")

        let pdb =
            match x.PdbFileHash with
            | None -> ""
            | Some pdb -> $", pdb: {pdb}"

        let signatureData =
            match x.SignatureDataHash with
            | None -> ""
            | Some signatureData -> $", signature json: {signatureData}"

        $"mvid: {mvid}, binary: {x.BinaryFileHash}{pdb}{signatureData}"

[<RequireQualifiedAccess>]
type CompilationResult<'TResult when 'TResult: equality> =
    | None
    | Stable of result: 'TResult * times: int
    | Unstable of initial: 'TResult * times: int * variant: 'TResult

let oldFiles (argsFile: FileInfo) =
    let outputFile =
        File.ReadAllLines(argsFile.FullName)
        |> Array.tryPick (fun line ->
            if not (line.StartsWith("-o:")) then
                None
            else
                let objPath = line.Replace("-o:", "")

                let objPath =
                    if File.Exists objPath then
                        objPath
                    else
                        Path.Combine(argsFile.Directory.FullName, objPath)

                FileInfo(objPath) |> Some
        )

    match outputFile with
    | None -> Seq.empty
    | Some outFile ->
        seq {
            yield! Directory.EnumerateFiles(outFile.Directory.FullName, "*.dll")
            yield! Directory.EnumerateFiles(outFile.Directory.FullName, "*.pdb")
            yield! Directory.EnumerateFiles(outFile.Directory.FullName, "*.signature-data*.json")
        }

let cleanUp argsFile =
    for file in oldFiles argsFile do
        File.Delete(file)

let compile (argsFile: FileInfo) (additionalArguments: string) (suffix: string) =
    let args = $"@{argsFile.Name}"

    Cli
        .Wrap(@"C:\Users\nojaf\Projects\fsharp\artifacts\bin\fsc\Release\net7.0\win-x64\publish\fsc.exe")
        .WithWorkingDirectory(argsFile.DirectoryName)
        .WithArguments($"\"{args}\" %s{additionalArguments}") // --debug-
        .ExecuteAsync()
        .Task.Wait()

    let binaryPath =
        let binaryPath = File.ReadAllLines(argsFile.FullName).[0].Replace("-o:", "")

        if File.Exists binaryPath then
            binaryPath
        else
            Path.Combine(argsFile.DirectoryName, binaryPath)

    let binary = FileInfo(binaryPath)
    let binaryHash = getFileHash binary.FullName
    let mvid = getMvid binary.FullName

    let pdbFile: FileInfo option =
        let path = Path.ChangeExtension(binary.FullName, ".pdb")

        if not (File.Exists path) then
            None
        else
            Some(FileInfo(path))

    let signatureData: FileInfo option =
        let path = Path.ChangeExtension(binary.FullName, ".signature-data.json")

        if not (File.Exists path) then
            None
        else
            Some(FileInfo(path))

    let result =
        {
            Mvid = mvid
            BinaryFileHash = binaryHash
            PdbFileHash = pdbFile |> Option.map (fun fi -> getFileHash fi.FullName)
            SignatureDataHash = signatureData |> Option.map (fun fi -> getFileHash fi.FullName)
        }

    printfn $"Compiled %s{suffix}, write date %A{binary.LastWriteTime}, result: \n    {result}"

    let renameToRun (file: FileInfo) =
        let differentPath =
            Path.Combine(
                file.Directory.FullName,
                $"{Path.GetFileNameWithoutExtension(file.Name)}-%s{suffix}{file.Extension}"
            )

        File.Move(file.FullName, differentPath)

    renameToRun binary
    Option.iter renameToRun pdbFile
    Option.iter renameToRun signatureData

    result

let runs argsFile =
    cleanUp argsFile

    (CompilationResult.None, [ 1..total ])
    ||> List.fold (fun (prevResult: CompilationResult<CompilationResultInfo>) idx ->
        match prevResult with
        | CompilationResult.Unstable _ -> prevResult
        | _ ->

        try
            let result =
                compile
                    argsFile
                    "--test:GraphBasedChecking --test:DumpCheckingGraph --debug:portable --test:DumpSignatureData"
                    $"%02i{idx}"

            match prevResult with
            | CompilationResult.Unstable _
            | CompilationResult.None _ -> CompilationResult.Stable(result, 1)
            | CompilationResult.Stable(prevResult, times) ->
                if prevResult <> result then
                    CompilationResult.Unstable(prevResult, times, result)
                else
                    CompilationResult.Stable(prevResult, times + 1)
        with ex ->
            printfn "%s" ex.Message
            prevResult
    )

// printfn "%A" (runs argsFile)

let compileTwice (argsFile: FileInfo) =
    try
        cleanUp argsFile

        let defaultResult =
            compile argsFile "--debug:portable --test:DumpSignatureData" "default"

        let graphResult =
            compile
                argsFile
                "--test:GraphBasedChecking --test:DumpCheckingGraph --debug:portable --test:DumpSignatureData"
                "graphhh"

        printfn "Are binaries equal: %s" (if defaultResult = graphResult then "yes" else "no")
        printfn "%A" (defaultResult, graphResult)
    with ex ->
        printfn "%s" ex.Message

compileTwice argsFile
