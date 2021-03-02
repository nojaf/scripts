#r "nuget: Fantomas, 4.5.0-alpha-001"

open FSharp.Compiler.SourceCodeServices
open FSharp.Compiler.SyntaxTree
open Fantomas

let source =
    System.IO.File.ReadAllText(System.IO.Path.Combine(__SOURCE_DIRECTORY__, __SOURCE_FILE__))
    |> SourceOrigin.SourceString

let fileName = "tmp.fsx"

let parsingOptions =
    { FSharpParsingOptions.Default with
          SourceFiles = [| fileName |] }

let checker = FSharpChecker.Create()

let ast =
    async {
        let! trees = CodeFormatter.ParseAsync(fileName, source, parsingOptions, checker)
        return (Array.map fst >> Array.head) trees
    }
    |> Async.RunSynchronously

match ast with
| ParsedInput.ImplFile (ParsedImplFileInput (modules = modules)) ->
    modules
    |> List.sumBy
        (function
        | SynModuleOrNamespace (decls = decls) -> List.length decls)
    |> printfn "Found %i declarations"
| _ -> printfn "No declarations found"
