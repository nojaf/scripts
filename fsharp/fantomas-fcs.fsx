#r "nuget: Fantomas.FCS, 5.2.0-alpha-009"
open Fantomas.FCS

let sourceCode =
    """
let a = $":^) {if true then "y" else "n"} d"
"""

Parse.parseFile false (FSharp.Compiler.Text.SourceText.ofString sourceCode) []
|> fst
|> printfn "%A"
