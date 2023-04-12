#r "nuget: TextCopy, 6.1.0"

open System
open System.IO
open TextCopy

let targetFolder =
    @"C:\Users\nojaf\Projects\resharper-fsharp\ReSharper.FSharp\test\data\features\completion"

let (</>) a b = Path.Combine(a, b)

let mkTest
    testName
    (implContent: string)
    //(signatureContentBefore: string)
    // (signatureContentAfter: string)
    =
    let implContent = implContent.Trim()
    // let signatureContentBefore = signatureContentBefore.Trim()
    // let signatureContentAfter = signatureContentAfter
    File.WriteAllText(targetFolder </> $"{testName}.fs", implContent)
    // File.WriteAllText(targetFolder </> $"{testName}.fs.gold", implContent)
    // File.WriteAllText(targetFolder </> $"{testName}.fsi", signatureContentBefore)
    // File.WriteAllText(targetFolder </> $"{testName}.fsi.gold", signatureContentAfter)

    ClipboardService.SetText($"[<Test>] member x.``{testName}`` () = x.DoNamedTest()")

mkTest
    "NamedUnionCaseFieldsPat - 05"
    """// ${COMPLETE_ITEM:banana}
// ${COMPLETE_ITEM:citrus}
module Foo

type Foo =
    | Meh of int * string
    | Bar of apple:int * banana: string * citrus: float

let a (b: Foo) =
    match b with
    | Bar(a = apple; {caret}) ->
"""

Directory.EnumerateFiles(targetFolder)
|> Seq.iter (fun path ->
    if path.Contains("NamedUnionCaseFieldsPat") then
        let content = File.ReadAllText(path)

        if content.[content.Length - 1] <> '\n' then
            File.WriteAllText(path, String.Concat(content, '\n'))
)

Directory.EnumerateFiles(targetFolder, "*.tmp")
|> Seq.iter (fun path ->
    let gold = Path.ChangeExtension(path, ".gold")
    File.Move(path, gold, true)
)
