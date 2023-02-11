#r "nuget: Fantomas.Core, 5.0.4"
// #r "nuget: Fantomas.Core, 5.2.0-beta-001"

open Fantomas.Core
open Fantomas.Core.FormatConfig

printfn "Version: %s" (CodeFormatter.GetVersion())

let config =
    { FormatConfig.Default with
        MultiLineLambdaClosingNewline = true
    }

"""

"""
