#r "nuget: TextCopy, 6.1.0"

open TextCopy

let template caseDescription problemDescription implementationSample generatedSignature =
    $"""
When generating a signature file from {caseDescription}, the generated code {problemDescription}.

**Repro steps**

```fsharp{implementationSample}```

leads to

```fsharp{generatedSignature}```

**Expected behaviour**

The generated signature should be considered equivalent to the backing source file.

**Actual behaviour**

The generated code isn't valid:

// TODO: editor image

**Known workarounds**

Edit signature file by hand.
    """

template
    "a function with statically resolved type parameters"
    "requires an additional space"
    """
module MyApp.GoodStuff

val inline toString:
  p: System.Threading.Tasks.Task<^revision> -> string
    when ^revision: (static member GetCommitHash: ^revision -> string)
"""
    """
module MyApp.GoodStuff

let inline toString< ^revision when ^revision: (static member GetCommitHash: ^revision -> string)>
    (p: System.Threading.Tasks.Task< ^revision >)
    : string =
    ""
"""
|> fun output ->
    printfn "%s" output
    ClipboardService.SetText(output)

// Title
// Signature file generation does not handle
