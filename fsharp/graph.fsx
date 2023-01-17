#r "nuget: Thoth.Json.Net, 9.0.0"
#r "nuget: TextCopy, 6.2.0"

open System.IO
open System.Text
open Thoth.Json.Net
open TextCopy

// let input = @"C:\Users\nojaf\Projects\safesparrow-fsharp\artifacts\bin\ParallelTypeCheckingTests\Debug\net7.0\Fantomas.Core.fsproj.deps.json"
// let input = @"C:\Users\nojaf\Downloads\fantomas-core-typed-tree.json"
let input =
    @"C:\Users\nojaf\Projects\main-fantomas\src\Fantomas.Core\Fantomas.Core.dll.deps.json"

let json = File.ReadAllText(input)

let decoder: Decoder<(string * string array) list> =
    Decode.keyValuePairs (Decode.array Decode.string)

let fileName (path: string) = Path.GetFileName(path)

let graph =
    let sb = StringBuilder()
    sb.AppendLine("graph") |> ignore

    match Decode.fromString decoder json with
    | Error err -> failwithf "could not decode, got %A" err
    | Ok result ->
        let indexes = result |> Seq.mapi (fun idx (key, _) -> fileName key, idx) |> dict

        for (key, deps) in result do
            let name = fileName key
            let idx = indexes.[name]
            sb.AppendLine($"    {idx}[\"{name}\"]") |> ignore

            for dep in deps do
                let depIdx = indexes.[fileName dep]
                sb.AppendLine($"    {idx} --> {depIdx}") |> ignore

    sb.ToString()

printfn "%s" graph
ClipboardService.SetText(graph)
// https://mermaid.live/
