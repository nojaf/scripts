#r "nuget: TextCopy, 6.1.0"

open System
open System.IO
open TextCopy

let targetFolder =
    @"C:\Users\nojaf\Projects\resharper-fsharp\ReSharper.FSharp\test\data\features\intentions\addFunctionToSignature"

let (</>) a b = Path.Combine(a, b)

let mkTest testName (implContent: string) (signatureContentBefore: string) (signatureContentAfter: string) =
    let implContent = implContent.Trim()
    let signatureContentBefore = signatureContentBefore.Trim()
    let signatureContentAfter = signatureContentAfter.Trim()
    File.WriteAllText(targetFolder </> $"{testName}.fs", implContent)
    File.WriteAllText(targetFolder </> $"{testName}.fs.gold", implContent)
    File.WriteAllText(targetFolder </> $"{testName}.fsi", signatureContentBefore)
    File.WriteAllText(targetFolder </> $"{testName}.fsi.gold", signatureContentAfter)

    ClipboardService.SetText($"[<Test>] member x.``{testName}`` () = x.DoNamedTestWithSignature()")

mkTest
    "Generic constraints - 01"
    """module Test

let memoizeBy{caret} (g: 'a -> 'c) (f: 'a -> 'b) =
    let cache =
        System.Collections.Concurrent.ConcurrentDictionary<_, _>(HashIdentity.Structural)

    fun x -> cache.GetOrAdd(Some(g x), lazy (f x)).Force()
"""
    """module Test
"""
    """module Test
"""

Directory.EnumerateFiles(targetFolder)
|> Seq.iter (fun path ->
    let content = File.ReadAllText(path)

    if content.[content.Length - 1] <> '\n' then
        File.WriteAllText(path, String.Concat(content, '\n'))
)

open System.Diagnostics.CodeAnalysis

let x ([<NotNull>] y: int, [<SuppressMessage "Some message">] z: string) = y + 0

let memoizeBy (g: 'a -> 'c) (f: 'a -> 'b) =
    let cache =
        System.Collections.Concurrent.ConcurrentDictionary<_, _>(HashIdentity.Structural)

    fun x -> cache.GetOrAdd(Some(g x), lazy (f x)).Force()
