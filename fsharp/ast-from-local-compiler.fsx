#r @"C:\Users\fverdonck\Projects\fsharp\artifacts\bin\FSharp.Core\Debug\netstandard2.0\FSharp.Core.dll"
#r @"C:\Users\fverdonck\Projects\fsharp\artifacts\bin\FSharp.Compiler.Service\Debug\netstandard2.0\FSharp.Compiler.Service.dll"

open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Syntax

let fileName = "/tmp.fsx"
let checker = FSharpChecker.Create()

let parsingOptions =
    { FSharpParsingOptions.Default with
        SourceFiles = [| fileName |]
        LangVersionText = "preview" }

let source =
    """
match () with
| x
| _ -> ()
"""

let ast =
    checker.ParseFile(fileName, FSharp.Compiler.Text.SourceText.ofString source, parsingOptions)
    |> Async.RunSynchronously
    |> fun result ->
        printfn "%A" result.Diagnostics
        result.ParseTree

printf "%A" ast

// #r @"C:\Users\fverdonck\Projects\fsharp\artifacts\bin\FSharp.Core\Debug\netstandard2.0\FSharp.Core.dll"
// #r @"C:\Users\fverdonck\Projects\fsharp\artifacts\bin\FSharp.Compiler.Service\Debug\netstandard2.0\FSharp.Compiler.Service.dll"

// FSharp.Compiler.Syntax.PrettyNaming.AddBackticksToIdentifierIfNeeded "mod"
// |> printfn "FSharp.Compiler.Syntax.PrettyNaming.AddBackticksToIdentifierIfNeeded :  %s"
