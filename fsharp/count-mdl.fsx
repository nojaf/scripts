#r "nuget: Fantomas, 4.5.0-alpha-001"

open FSharp.Compiler.SourceCodeServices
open FSharp.Compiler.SyntaxTree
open Fantomas
open System.IO

let checker = FSharpChecker.Create()

let countInFile (path: string) =
    let source = File.ReadAllText(path) |> SourceOrigin.SourceString

    let ext =
        if (Path.GetExtension(path) = ".fsi") then
            ".fsi"
        else
            ".fsx"

    let fileName = $"{System.Guid.NewGuid()}{ext}"

    let parsingOptions =
        { FSharpParsingOptions.Default with
            SourceFiles = [| fileName |] }

    let ast =
        async {
            let! trees = CodeFormatter.ParseAsync(fileName, source, parsingOptions, checker)
            return (Array.map fst >> Array.head) trees
        }
        |> Async.RunSynchronously

    match ast with
    | ParsedInput.ImplFile(ParsedImplFileInput(modules = modules)) ->
        modules
        |> List.sumBy (
            function
            | SynModuleOrNamespace(decls = decls) -> List.length decls
        )
    | _ -> 0

let countInFolder (path: string) : unit =
    let files =
        seq {
            yield! Directory.EnumerateFiles(path, "*.fs", SearchOption.AllDirectories)
            yield! Directory.EnumerateFiles(path, "*.fsi", SearchOption.AllDirectories)
            yield! Directory.EnumerateFiles(path, "*.fsx", SearchOption.AllDirectories)
        }
        |> Seq.filter (fun path -> not (path.Contains("obj/") || path.Contains("tests/")))

    files
    |> Seq.map (fun path -> path, countInFile path)
    |> Seq.sortByDescending snd
    |> Seq.iter (fun (p, i) -> printfn "%s : %i" p i)

countInFolder @"C:\Users\fverdonck\Projects\fantomas"
