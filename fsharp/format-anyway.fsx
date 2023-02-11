//#r "nuget: Fantomas, 4.5.0-alpha-005"
#r @"C:\Users\fverdonck\Projects\fantomas\src\Fantomas.CoreGlobalTool\bin\Release\netcoreapp3.1\FSharp.Compiler.Service.dll"
#r @"C:\Users\fverdonck\Projects\fantomas\src\Fantomas.CoreGlobalTool\bin\Release\netcoreapp3.1\Fantomas.dll"

open Fantomas
open FSharp.Compiler.CodeAnalysis
open System.IO

let fileName =
    @"C:\Users\fverdonck\Projects\fantomas\src\Fantomas\CodeFormatterImpl.fs"

let source = Path.Combine(__SOURCE_DIRECTORY__, "..", fileName) |> File.ReadAllText

let parsingOptions =
    { FSharpParsingOptions.Default with
        SourceFiles = [| fileName |]
    }

let checker = FSharpChecker.Create()

let formatted =
    CodeFormatter.FormatDocumentAsync(
        fileName,
        SourceOrigin.SourceString source,
        FormatConfig.FormatConfig.Default,
        parsingOptions,
        checker
    )
    |> Async.RunSynchronously

Path.Combine(__SOURCE_DIRECTORY__, "..", "Formatted.fs")
|> fun path -> File.WriteAllText(path, formatted)
