#r "nuget: TextCopy, 6.1.0"

open System
open System.IO
open TextCopy

let targetFolder =
    @"C:\Users\nojaf\Projects\resharper-fsharp\ReSharper.FSharp\test\data\features\generate\signatureFiles"

let (</>) a b = Path.Combine(a, b)

let mkTest
    testName
    (implContent: string)
    =
    File.WriteAllText(targetFolder </> $"{testName}.fs", implContent)
    ClipboardService.SetText($"[<Test>] member x.``{testName}`` () = x.DoNamedTest()")

mkTest
    "Nested module 01"
    """// ${KIND:SignatureFile}
// ${SELECT0:Generate signature file title}
module Foo

    module Bar =
        open System
{caret}
"""

Directory.EnumerateFiles(targetFolder, "*.tmp")
|> Seq.iter (fun path ->
    let gold = Path.ChangeExtension(path, ".gold")
    File.Move(path, gold, true)
)