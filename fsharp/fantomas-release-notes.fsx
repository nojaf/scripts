#r "nuget: FSharp.Data, 4.0.1"

// 2021-09-07T16:06:05.2730000
// is:issue is:closed closed:>=2021-09-08
// [...document.querySelectorAll(".opened-by")].map(span => span.textContent.match(/#\d+/)[0]).map(a => parseInt(a.substring(1),0)).join(';')

open FSharp.Data
open System

let fixedIssues =
    "1970;1969;1965;1962;1961;1954;1953;1945;1944;1933;1918;1898;1874"
    |> fun issues -> issues.Split(';')
    |> Array.map int
    |> Array.toList


[<Literal>]
let SampleLink = "https://github.com/fsprojects/fantomas/issues/363"

type GithubIssuePage = HtmlProvider<SampleLink>

let getTitle issue =
    let url = $"https://github.com/fsprojects/fantomas/issues/%i{issue}"

    let page = GithubIssuePage.Load(url)

    page.Html.CssSelect(".js-issue-title")
    |> List.tryHead
    |> Option.map (fun t ->
        let title = t.InnerText().Trim()

        let dot = if title.EndsWith(".") then String.Empty else "."

        $"* Fix %s{title}%s{dot} [#%i{issue}](%s{url})"
    )

let issues = List.choose getTitle fixedIssues |> String.concat Environment.NewLine

printfn $"%s{issues}"
