#r "nuget: Fantomas.FCS, 5.0.0-alpha-003"

open Fantomas.FCS

let sourceCode =
    """
type T =
    abstract P: Task<'t when 't :> INumber>
"""

Parse.parseFile true (FSharp.Compiler.Text.SourceText.ofString sourceCode) []
|> snd
|> printfn "%A"
