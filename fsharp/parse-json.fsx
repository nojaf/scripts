open System
open System.Text.Json

let json = Console.In.ReadToEnd()
let jsonDocument = JsonDocument.Parse json

let options =
    jsonDocument.RootElement
        .GetProperty("Items")
        .GetProperty("FscCommandLineArgs")
        .EnumerateArray()
    |> Seq.map (fun arg -> arg.GetProperty("Identity").GetString())
    |> Seq.toArray

for option in options do
    printfn "%s" option
