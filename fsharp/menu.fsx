#r "nuget: Suave"
#r "nuget: Thoth.Json.Net, 8.0.0"
#r "nuget: Fable.React, 8.0.1"

open System.Net
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Thoth.Json.Net

type Item = {
    Title: string
    Category: string
    CategoryIndex: int
    Index: int
    Link: string
}

let decodeStringAsInt = Decode.string |> Decode.map (int)

let decodeItem: Decoder<Item> =
    Decode.object (fun get -> {
        Category = get.Required.Field "category" Decode.string
        CategoryIndex = get.Required.Field "categoryIndex" decodeStringAsInt
        Index = get.Required.Field "index" decodeStringAsInt
        Link = get.Required.Field "link" Decode.string
        Title = get.Required.Field "title" Decode.string
    })

open Fable.React
open Fable.React.Props

let view (items: Item array) : string =
    let groups = Array.groupBy (fun i -> i.CategoryIndex) items

    let children =
        groups
        |> Array.map (fun (_, groupItems) ->
            let groupTitle = groupItems[0].Category

            let id = $"menu-{groupTitle}-collapse".Replace(" ", "-").Trim().ToLower()

            let groupItems =
                groupItems
                |> Array.map (fun (gi: Item) ->
                    li [] [ a [ Href gi.Link; ClassName "ms-4 my-2 d-block" ] [ str gi.Title ] ]
                )

            li [ ClassName "mb-1" ] [
                button [
                    ClassName "btn align-items-center rounded"
                    Data("bs-toggle", "collapse")
                    Data("bs-target", $"#{id}")
                    AriaExpanded true
                ] [ str groupTitle ]
                div [ ClassName "collapse show"; Id id ] [
                    ul [ ClassName "list-unstyled fw-normal pb-1 small" ] groupItems
                ]
            ]
        )

    let element = fragment [] children
    Fable.ReactServer.renderToString (element)

let menuPart =
    POST
    >=> Filters.path "/menu"
    >=> (fun (ctx: HttpContext) -> async {
        let json = System.Text.Encoding.UTF8.GetString(ctx.request.rawForm)
        printfn "received: %s" json

        match Decode.fromString (Decode.array decodeItem) json with
        | Error err -> return! OK $"<div>Failed to decode, {err}</div>" ctx
        | Ok items ->
            let html = view items
            printfn "html:\n%s" html
            return! OK html ctx
    })

let port = 8906us

startWebServer
    { defaultConfig with
        bindings = [ HttpBinding.create HTTP IPAddress.Loopback port ]
    }
    menuPart
