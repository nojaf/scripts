open System
open System.IO

let countLines file =
    File.ReadAllText file
    |> fun v -> v.Split([| "\n" |], StringSplitOptions.None)
    |> Seq.length

Directory.EnumerateFiles(@"C:\Users\fverdonck\Projects\fsharp\src\fsharp", "*.fs", SearchOption.AllDirectories)
|> Seq.map (fun file -> file, countLines file)
|> Seq.sortByDescending snd
|> Seq.take 10
|> Seq.map (fun (file, lines) -> sprintf "%s: %i lines" (file.Replace(@"C:\Users\fverdonck\Projects\", "")) lines)
|> Seq.toArray
|> printfn "%A"
