#i "nuget:https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-tools/nuget/v3/index.json"
#r "nuget:FSharp.Compiler.Service, 40.0.1-preview.21330.7"

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
        return tree.ParseTree
    }
    |> Async.RunSynchronously

getAst
    "tmp.fsi"
    """
#I __SOURCE_DIRECTORY__
    """

getAst
    "tmp.fsx"
    """
module foo

// bar
// baz
    """
