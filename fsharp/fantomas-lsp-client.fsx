#r "nuget: CliWrap, 3.6.0"
#r "nuget: Nerdbank.Streams, 2.9.112"
#r "nuget: Thoth.Json.Net, 11.0.0"

open System.IO
open System.Text
open Thoth.Json.Net
open CliWrap
open Nerdbank.Streams

let versionContent =
    Encode.object
        [
            "jsonrpc", Encode.string "2.0"
            "id", Encode.string "3"
            "method", Encode.string "fantomas/version"
        ]
    |> Encode.toString 4

let contentLength = Encoding.UTF8.GetByteCount(versionContent)

let versionMessage = $"""Content-Length: {contentLength}\r\n\r\n{versionContent}"""

let stdIn = new SimplexStream()
let stdInWriter = new StreamWriter(stdIn)
let stdOut = StringBuilder()

Cli
    .Wrap("dotnet")
    .WithArguments("fantomas --daemon")
    .WithStandardInputPipe(PipeSource.FromStream(stdIn))
    .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOut))
    .ExecuteAsync()

stdInWriter.Write(versionMessage)

stdOut.ToString()
