#r @"C:\Users\nojaf\Projects\fsharp\artifacts\bin\FSharp.Core\Debug\netstandard2.0\FSharp.Core.dll"
#r @"C:\Users\nojaf\Projects\fsharp\artifacts\bin\FSharp.Compiler.Service\Debug\netstandard2.0\FSharp.Compiler.Service.dll"

//#r @"C:\Users\nojaf\.nuget\packages\fsharp.compiler.service\38.0.2\lib\netstandard2.0\FSharp.Compiler.Service.dll"

open FSharp.Compiler.CodeAnalysis



let fileName = "/tmp.fsx"
let checker = FSharpChecker.Create()

let parsingOptions =
    { FSharpParsingOptions.Default with
          SourceFiles = [| fileName |] }

let source =
    "
type Foo =
    | One = 1

and Bar =
    | Two = 2
"

let ast =
    checker.ParseFile(fileName, FSharp.Compiler.Text.SourceText.ofString source, parsingOptions)
    |> Async.RunSynchronously
    |> fun result -> result.ParseTree

printfn "%A" ast
