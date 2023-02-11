#r "nuget: Fantomas.Core, 5.2.0-alpha-012"

open Fantomas.Core
open Fantomas.Core.FormatConfig

let config =
    { FormatConfig.Default with
        MaxLineLength = 90
    }

CodeFormatter.FormatDocumentAsync(
    false,
    """
let a =  0
    """,
    config
)
|> Async.RunSynchronously
