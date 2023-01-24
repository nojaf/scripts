#i "nuget:https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-tools/nuget/v3/index.json"
#r "nuget:FSharp.Compiler.Service, 40.0.1-preview.21418.3"

open FSharp.Compiler.Text
open FSharp.Compiler.CodeAnalysis

let fileName = "tmp.fsi"
let checker = FSharpChecker.Create()

let parsingOptions =
    { FSharpParsingOptions.Default with
          SourceFiles = [| fileName |] }

let getAst fileName source =
    async {
        let! tree = checker.ParseFile(fileName, source |> SourceText.ofString, parsingOptions)
        if tree.Diagnostics.Length > 0 then printfn "%A" tree.Diagnostics
        return tree.ParseTree
    }
    |> Async.RunSynchronously

getAst
    "tmp.fsx"
    """
let u = ""

match!
    match! u with
    | null -> ""
    | s -> s
    with
| "" -> x
| _ -> failwith ""
    """
