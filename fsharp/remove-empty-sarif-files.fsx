// A sarif could actually be empty, the following code cleans up the result folder.
#r "nuget: Thoth.Json.Net, 11.0.0"

open System.IO
open Thoth.Json.Net

let anyDecoder = Decode.object (fun _ -> true)

let sarifDecoder =
    Decode.object (fun get ->
        let runs =
            get.Required.Field
                "runs"
                (Decode.array (Decode.object (fun get -> get.Required.Field "results" (Decode.array anyDecoder))))

        (Array.concat runs).Length
    )

let removeEmptySarifFiles (analysisResultFolder: string) =
    Directory.EnumerateFiles(analysisResultFolder, "*.sarif")
    |> Seq.iter (fun file ->
        let json = File.ReadAllText file

        match Decode.fromString sarifDecoder json with
        | Error err -> printfn "%s" err
        | Ok count ->
            if count = 0 then
                printfn $"Removing %s{file}"
                File.Delete file
    )

removeEmptySarifFiles @"C:\Users\nojaf\Projects\Fable\src\reports"
