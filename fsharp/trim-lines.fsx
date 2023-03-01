open System.IO

let trimLines (path: string) =
    let lines = File.ReadAllLines(path)
    let trimmed = lines |> Array.map (fun line -> line.TrimEnd())
    File.WriteAllLines(path, trimmed)

trimLines
    @"C:\Users\nojaf\Projects\resharper-fsharp\ReSharper.FSharp\src\FSharp.Psi.Intentions\src\Intentions\GenerateSignatureFileAction.fs"
