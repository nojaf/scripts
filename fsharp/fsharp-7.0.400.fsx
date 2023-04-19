#r "nuget: FSharp.Data, 6.0.1-beta002"
#r "nuget: CliWrap, 3.6.0"

open System
open System.Text
open System.Linq
open System.Xml.Linq
open FSharp.Data
open CliWrap
// 7.0.4xx
(*
    Download https://aka.ms/dotnet/8.0.1xx/daily/productCommit-win-x64.txt
    Copy the commit hash for sdk_commit
    Open https://github.com/dotnet/sdk/blob/{SDK_COMMIT_FROM_ABOVE}/eng/Version.Details.xml
    Copy the commit hash for <Dependency Name="Microsoft.FSharp.Compiler"
    Open https://github.com/dotnet/fsharp/commit/{FSHARP_COMMIT_FROM_ABOVE}
*)

let sdkResponse =
    Http.Request("https://aka.ms/dotnet/7.0.4xx/daily/productCommit-win-x64.txt")

let sdkHash =
    match sdkResponse.Body with
    | Text _ -> failwith "Unexpected text response"
    | Binary body ->
        Encoding.UTF8
            .GetString(body)
            .Split([| ' '; '\n'; ',' |], StringSplitOptions.RemoveEmptyEntries)
        |> Array.choose (fun line ->
            if not (line.Contains("sdk:")) then
                None
            else
                line.Replace("sdk:", "").Replace("\"", "") |> Some
        )
        |> Array.head

let versionResponse =
    Http.Request(
        $"https://raw.githubusercontent.com/dotnet/sdk/{sdkHash}/eng/Version.Details.xml",
        headers = [| "Content-Disposition", "attachment; filename=\"Version.Details.xml\"" |]
    )

let fsharpCommit =
    match versionResponse.Body with
    | Binary _ -> failwith "Unexpected binary response"
    | Text body ->
        let xml = XElement.Parse(body)

        query {
            for dependency in xml.Descendants("Dependency") do
                where (dependency.Attribute("Name").Value = "Microsoft.FSharp.Compiler")
                select (dependency.Descendants("Sha").First().Value)
        }
        |> Seq.head

Cli
    .Wrap(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe")
    .WithArguments($"https://github.com/dotnet/fsharp/commit/{fsharpCommit}")
    .ExecuteAsync()
    .Task.Wait()
