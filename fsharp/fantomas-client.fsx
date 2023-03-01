#r "nuget: Fantomas.Client, 0.8.0"

open System
open System.IO
open Fantomas.Client.Contracts
open Fantomas.Client.LSPFantomasService

let service: FantomasService = new LSPFantomasService()
// this path needs to be absolute and exist
let filePath = Path.Combine(__SOURCE_DIRECTORY__, "gold.fsx")

service.VersionAsync(filePath).Result |> printfn "%A"




// let file = @"C:\Users\nojaf\Projects\fsharp\src\Compiler\Checking\NicePrint.fs"
// let content = File.ReadAllText file
//
// let formatReq: FormatDocumentRequest =
//     {
//         SourceCode = content
//         FilePath = file
//         Config = None
//     }
//
// service.FormatDocumentAsync(formatReq).Result.Code
//
// (service :> IDisposable).Dispose()
