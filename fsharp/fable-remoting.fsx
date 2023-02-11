#r "nuget: Fable.Remoting.Suave"

open System.Collections.Generic
open Suave
open Fable.Remoting.Server
open Fable.Remoting.Suave

type FantomasConfiguration =
    {
        /// Name of the setting, data type
        Options: Dictionary<string, string>
        /// Name of data type, allowed values
        EnumOptions: Dictionary<string, string array>
    }

type FantomasDaemon =
    // abstract member Configuration : unit -> FantomasConfiguration
    // abstract member Format : sourceCode: string -> fileName: string -> configuration: string option -> string
    // abstract member FormatSelection : sourceCode: string -> fileName: string -> range: obj -> configuration: string option -> string
    {
        Version: unit -> Async<string>
    }

// Server

let daemon: FantomasDaemon =
    {
        Version = fun () -> Async.result "4.6"
    }

let fableWebApp: WebPart =
    Remoting.createApi ()
    |> Remoting.fromValue daemon
    |> Remoting.withBinarySerialization
    |> Remoting.buildWebPart

startWebServer defaultConfig fableWebApp

// Client

#r "nuget: Fable.Remoting.DotnetClient"

open Fable.Remoting.DotnetClient

let client =
    Remoting.createApi "http://127.0.0.1:8080"
    |> Remoting.withBinarySerialization
    |> Remoting.buildProxy<FantomasDaemon>

async {
    let! version = client.Version()
    printfn "version %s" version
}
|> Async.RunSynchronously
