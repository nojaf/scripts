open System
open System.IO
open System.Text.RegularExpressions

let changelog = @"C:\Users\nojaf\Projects\Fable\src\fable-compiler-js\CHANGELOG.md"

let lines = File.ReadAllLines changelog

let versionMap =
    """
## 3.3.0 - 2023-11-10
## 3.0.0 - 2020-12-22
## 3.0.0-rc - 2020-11-16
## 1.3.1 - 2020-04-12
## 1.3.0 - 2020-04-10
## 1.2.4 - 2020-04-09
## 1.2.3 - 2020-03-09
## 1.2.2 - 2020-02-14
## 1.2.1 - 2019-12-23
## 1.2.0 - 2019-11-20
## 1.1.1 - 2019-09-30
## 1.1.0 - 2019-09-25
## 1.0.6 - 2019-09-25
## 1.0.5 - 2019-08-14
## 1.0.4 - 2019-06-16
## 1.0.3 - 2019-04-02
## 1.0.2 - 2019-03-05
## 1.0.0-beta-003 - 2019-03-01
## 1.0.1 - 2019-03-01
## 1.0.0 - 2019-02-28
## 1.0.0-beta-002 - 2019-02-21
## 1.0.0-beta-001 - 2019-02-19
## 1.0.0-alpha-017 - 2019-02-12
## 1.0.0-alpha-016 - 2019-02-11
## 1.0.0-alpha-015 - 2019-02-11
## 1.0.0-alpha-014 - 2019-02-07
## 1.0.0-alpha-012 - 2019-01-22
## 1.0.0-alpha-011 - 2019-01-22
## 1.0.0-alpha-010 - 2018-12-17
## 1.0.0-alpha-009 - 2018-12-17
## 1.0.0-alpha-008 - 2018-12-14
## 1.0.0-alpha-007 - 2018-12-14
## 1.0.0-alpha-006 - 2018-12-14
## 1.0.0-alpha-005 - 2018-12-13
## 1.0.0-alpha-004 - 2018-12-13
## 1.0.0-alpha-003 - 2018-12-13
## 1.0.0-alpha-002 - 2018-12-13
## 1.0.0-alpha-001 - 2018-12-12
    """
        .Split('\n', StringSplitOptions.RemoveEmptyEntries ||| StringSplitOptions.TrimEntries)
    |> Array.map (fun line ->
        let parts = line.Trim().Split " - "
        let version = parts.[0].Replace("#", "").Trim()
        version, line
    )
    |> Map.ofArray

let isDateAtEndOfLine (line: string) : bool =
    let pattern = @"\d{4}-\d{2}-\d{2}$"
    Regex.IsMatch(line, pattern)

let datelessLines =
    lines
    |> Array.filter (fun line -> line.TrimStart().StartsWith("## ") && not (isDateAtEndOfLine (line.TrimEnd())))

let updatedLines =
    lines
    |> Array.map (fun line ->
        if
            not (line.TrimStart().StartsWith("## ", StringComparison.Ordinal))
            || isDateAtEndOfLine (line.TrimEnd())
        then
            line
        else
            let version = line.TrimStart().Substring(3).TrimEnd()

            match Map.tryFind version versionMap with
            | None -> line
            | Some line -> line
    )

File.WriteAllLines(changelog, updatedLines)

let nuget =
    """
String.prototype.print = function() {
  console.log(this.toString());
};

[...document.querySelectorAll("tr:has(a):has(span[data-datetime])")]
    .map(tr => {
        const a = tr.querySelector("a").textContent.trim();
        const time = tr.querySelector("span[data-datetime]").getAttribute("data-datetime").split('T')[0];
        return `## ${a} - ${time}`
    })
    .join('\n')
    .print();
    """

let npm =
    """
String.prototype.print = function() {
  console.log(this.toString());
};

[...document.querySelectorAll("li:has(> a):has(time)")].map(li => {
   const a = li.firstChild;
    const t = li.querySelector("time").getAttribute('datetime').split('T')[0];
   return `## ${a.textContent} - ${t}`
}).join('\n').print();
"""
