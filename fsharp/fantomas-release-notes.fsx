#r "nuget: FSharp.Data, 4.0.1"

// 2021-02-26T08:19:07.2400000
// is:issue is:closed closed:>=2021-04-24
// [...document.querySelectorAll(".opened-by")].map(span => span.textContent.match(/#\d+/)[0]).map(a => parseInt(a.substring(1),0)).join(';')

open FSharp.Data
open System

let fixedIssues =
    "1759;1757;1473;1468;1272;1189;1161;1028;973;815;814;639;364;308;305"
    |> fun issues -> issues.Split(';')
    |> Array.map int
    |> Array.toList


[<Literal>]
let SampleLink =
    "https://github.com/fsprojects/fantomas/issues/363"

type GithubIssuePage = HtmlProvider<SampleLink>

let getTitle issue =
    let url =
        $"https://github.com/fsprojects/fantomas/issues/%i{issue}"

    let page = GithubIssuePage.Load(url)

    page.Html.CssSelect(".js-issue-title")
    |> List.tryHead
    |> Option.map (fun t ->
        let title = t.InnerText().Trim()

        let dot =
            if title.EndsWith(".") then
                String.Empty
            else
                "."

        $"* Fix %s{title}%s{dot} [#%i{issue}](%s{url})"
    )

let issues =
    List.choose getTitle fixedIssues
    |> String.concat Environment.NewLine

printfn $"%s{issues}"
