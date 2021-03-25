#i "nuget:https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-tools/nuget/v3/index.json"
#r "nuget:FSharp.Compiler.Service, 39.0.0-preview.21174.2"

open FSharp.Compiler.Text
open FSharp.Compiler.Syntax
open FSharp.Compiler.CodeAnalysis

let fileName = "tmp.fsx"
let checker = FSharpChecker.Create()

let parsingOptions =
    { FSharpParsingOptions.Default with
          SourceFiles = [| fileName |] }

let source =
    """
namespace global

let s = @"x"
let x - 0
    """
    |> SourceText.ofString

let ast =
    async {
        let! tree = checker.ParseFile(fileName, source, parsingOptions)
        return tree.ParseTree
    }
    |> Async.RunSynchronously

match ast with
| ParsedInput.ImplFile (ParsedImplFileInput (modules = modules)) -> printfn "%A" modules
| _ -> ()
