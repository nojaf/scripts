#r "nuget: Fantomas.Client"

open System
open System.IO
open Fantomas.Client.Contracts
open Fantomas.Client.LSPFantomasService

let service = new LSPFantomasService() :> FantomasService

let file = @"C:\Users\nojaf\Projects\fsharp\src\Compiler\Checking\NicePrint.fs"
let content = File.ReadAllText file

let formatReq: FormatDocumentRequest =
    {
        SourceCode = content
        FilePath = file
        Config = None
    }

service.FormatDocumentAsync(formatReq).Result.Code

(service :> IDisposable).Dispose()
