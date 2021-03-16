#r "nuget: Fantomas, 4.4.0"

open Fantomas
open FSharp.Compiler.SourceCodeServices
open System.IO

let fileName = "CheckExpressions.fs"
let source = Path.Combine(__SOURCE_DIRECTORY__, "..", fileName) |> File.ReadAllText
let parsingOptions =
    { FSharpParsingOptions.Default with
          SourceFiles = [| fileName |] }
let checker = FSharpChecker.Create()

let formatted =
    CodeFormatter.FormatDocumentAsync(fileName, SourceOrigin.SourceString source, FormatConfig.FormatConfig.Default, parsingOptions, checker)
    |> Async.RunSynchronously

Path.Combine(__SOURCE_DIRECTORY__, "..", "Formatted.fs")
|> fun path -> File.WriteAllText(path, formatted)