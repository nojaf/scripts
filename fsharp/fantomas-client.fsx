#r "nuget: Fantomas.Client"

open System
open System.IO
open Fantomas.Client.Contracts
open Fantomas.Client.LSPFantomasService

let service = new LSPFantomasService() :> FantomasService

let file = @"C:\Users\fverdonck\Projects\HelloWorld\Math.fs"
let content = File.ReadAllText file

let response =
    service.VersionAsync file
    |> Async.AwaitTask
    |> Async.RunSynchronously

printfn "%A" response

(service :> IDisposable).Dispose()
