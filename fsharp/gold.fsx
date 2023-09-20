#r "nuget: TextCopy, 6.1.0"

open System
open System.IO
open TextCopy

let targetFolder =
    @"C:\Users\nojaf\Projects\resharper-fsharp\ReSharper.FSharp\test\data\features\quickFixes\updateCompiledNameInSignatureFix"

DirectoryInfo(targetFolder).Create()

let (</>) a b = Path.Combine(a, b)

let mkTest testName (implContent: string) (signatureContent: string) =
    File.WriteAllText(targetFolder </> $"{testName}.fs", implContent)
    File.WriteAllText(targetFolder </> $"{testName}.fs.gold", implContent)
    File.WriteAllText(targetFolder </> $"{testName}.fsi", signatureContent)
    ClipboardService.SetText($"[<Test>] member x.``{testName}`` () = x.DoNamedTestWithSignature()")

mkTest
    "Attribute in implementation - 03"
    "namespace A

module B =

    [<CompiledName(\"X\")>]
    let x{caret} (a:int) (b:int) = a + 1
"
    "namespace A
    
module B =

    val x: a:int -> b:int -> int
"

Directory.EnumerateFiles(targetFolder, "*.tmp")
|> Seq.iter (fun path ->
    let gold = Path.ChangeExtension(path, ".gold")
    File.Move(path, gold, true)
)
