#r "nuget: TextCopy, 6.1.0"

open System
open System.IO
open TextCopy

let targetFolder =
    @"C:\Users\nojaf\Projects\resharper-fsharp\ReSharper.FSharp\test\data\features\intentions\generateSignatureFile"

let (</>) a b = Path.Combine(a, b)

let mkTest
    testName
    (implContent: string)
    //(signatureContentBefore: string)
    (signatureContentAfter: string)
    =
    let implContent = implContent.Trim()
    // let signatureContentBefore = signatureContentBefore.Trim()
    let signatureContentAfter = signatureContentAfter
    File.WriteAllText(targetFolder </> $"{testName}.fs", implContent)
    File.WriteAllText(targetFolder </> $"{testName}.fs.gold", implContent)
    // File.WriteAllText(targetFolder </> $"{testName}.fsi", signatureContentBefore)
    File.WriteAllText(targetFolder </> $"{testName}.fsi.gold", signatureContentAfter)

    ClipboardService.SetText($"[<Test>] member x.``{testName}`` () = x.DoNamedTestWithSignature()")

mkTest
    "ModuleStructure - 01"
    """module Foo

open System
{caret}
let a = 0
"""
    """module Test

open System
"""

Directory.EnumerateFiles(targetFolder)
|> Seq.iter (fun path ->
    let content = File.ReadAllText(path)

    if content.[content.Length - 1] <> '\n' then
        File.WriteAllText(path, String.Concat(content, '\n'))
)
