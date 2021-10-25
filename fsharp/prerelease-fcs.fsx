// #i "nuget:https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-tools/nuget/v3/index.json"
// #r "nuget:FSharp.Compiler.Service, 40.0.1-preview.21330.7"
#r @"C:\Users\fverdonck\Projects\fsharp\artifacts\bin\FSharp.Compiler.Service\Debug\netstandard2.0\FSharp.Compiler.Service.dll"

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
        if not (Seq.isEmpty tree.Diagnostics) then
            printfn "%A" tree.Diagnostics
        return tree.ParseTree
    }
    |> Async.RunSynchronously

// getAst
//     "tmp.fsi"
//     """
// #I __SOURCE_DIRECTORY__
//     """

getAst
    "tmp.fsx"
    """
match foo with
| Bar (
    bar1,
    bar2,
    bar3,
    bar4
  ) ->
    ()
    """
|> printfn "%A"