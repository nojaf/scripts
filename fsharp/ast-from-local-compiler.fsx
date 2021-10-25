#r @"C:\Users\fverdonck\Projects\fsharp\artifacts\bin\FSharp.Core\Debug\netstandard2.0\FSharp.Core.dll"
#r @"C:\Users\fverdonck\Projects\fsharp\artifacts\bin\FSharp.Compiler.Service\Debug\netstandard2.0\FSharp.Compiler.Service.dll"

open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Syntax

let fileName = "/tmp.fsx"
let checker = FSharpChecker.Create()

let parsingOptions =
    { FSharpParsingOptions.Default with SourceFiles = [| fileName |] }

let source =
    """
let a = list.[..^0]   // 1,2,3,4,5
let b = list.[..^1]   // 1,2,3,4
let c = list.[0..^1]  // 1,2,3,4
let d = list.[^1..]   // 4,5
let e = list.[^0..]   // 5
let f = list.[^2..^1] // 3,4
"""

let ast =
    checker.ParseFile(fileName, FSharp.Compiler.Text.SourceText.ofString source, parsingOptions)
    |> Async.RunSynchronously
    |> fun result ->
        printfn "%A" result.Diagnostics
        result.ParseTree

printf "%A" ast
