#r "nuget: FSharp.Data.Adaptive, 1.2.13"

open System
open System.IO
open System.Text
open System.Threading
open System.Xml.Linq
open FSharp.Data.Adaptive

let file = @"C:\Users\nojaf\Projects\emerald-sword\server.fsproj"

let getFilesAux fsprojContent =
    let xdoc = XDocument.Parse fsprojContent

    let getAttributeValue (element: XElement) name =
        element.Attribute(XName.Get(name))
        |> Option.ofObj
        |> Option.map (fun attr -> attr.Value)

    let itemGroups = xdoc.Root.Elements(XName.Get("ItemGroup"))

    itemGroups
    |> Seq.tryPick (fun (itemGroup: XElement) ->
        getAttributeValue itemGroup "Label"
        |> Option.bind (fun label ->
            if label <> "FSI" then
                Some Array.empty
            else
                itemGroup.Elements(XName.Get("Compile"))
                |> Seq.choose (fun compile ->
                    getAttributeValue compile "Include"
                    |> Option.map (fun file -> Path.Combine(@"C:\Users\nojaf\Projects\emerald-sword", file))
                )
                |> Seq.toArray
                |> Some
        )
    )
    |> Option.defaultValue Array.empty

let getFiles fsprojContent =
    let files = getFilesAux fsprojContent

    files
    |> Array.map AdaptiveFile.GetLastWriteTime
    |> ASet.ofArray
    |> ASet.flattenA
    |> ASet.tryMax
    |> AVal.map (fun lastModified -> lastModified, files)

let aFile = AdaptiveFile.ReadAllText(file) |> AVal.bind getFiles

let disposable = aFile.AddCallback(fun files -> printfn "Lines %A" files)

disposable.Dispose()

// $"#load @\"%s{absolutePath}\""
FileInfo(@"C:\Users\nojaf\Projects\scripts\fsharp\App.fs").LastAccessTime
