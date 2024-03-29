#r @"C:\Users\nojaf\Projects\fsharp\artifacts\bin\FSharp.Core\Debug\netstandard2.0\FSharp.Core.dll"
#r @"C:\Users\nojaf\Projects\fsharp\artifacts\bin\FSharp.Compiler.Service\Debug\netstandard2.0\FSharp.Compiler.Service.dll"

open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Syntax

let fileName = "/tmp.fsx"
let checker = FSharpChecker.Create()

let parsingOptions =
    { FSharpParsingOptions.Default with
        SourceFiles = [| fileName |]
    }

let source =
    "
#if FOO
    \"\"\"
    #if BAR
                printfn \"FOO\"
    #endif
    \"\"\"
#else
                ()
#endif
"

let ast =
    checker.ParseFile(fileName, FSharp.Compiler.Text.SourceText.ofString source, parsingOptions)
    |> Async.RunSynchronously
    |> fun result -> result.ParseTree

printfn "%A" ast
