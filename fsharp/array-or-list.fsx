#r "nuget: FSharp.Compiler.Service"

open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text.Range
open FSharp.Compiler.Text.Position

let fileName = "/tmp.fsx"
let checker = FSharpChecker.Create()

let parsingOptions =
    { FSharpParsingOptions.Default with SourceFiles = [| fileName |] }

let source =
    """
[    1   ]
"""

let ast =
    checker.ParseFile(fileName, FSharp.Compiler.Text.SourceText.ofString source, parsingOptions)
    |> Async.RunSynchronously
    |> fun result -> result.ParseTree

printf "%A" ast

let arrayNode =
    match ast with
    | ParsedInput.ImplFile (ParsedImplFileInput(modules = [ SynModuleOrNamespace(decls = [ SynModuleDecl.DoExpr (expr = array) ]) ])) ->
        match array with
        | SynExpr.ArrayOrList _
        | SynExpr.ArrayOrListOfSeqExpr _ -> array
        | _ -> failwith "should have array"
    | _ -> failwith "should have array"

let lpr =
    mkRange
        arrayNode.Range.FileName
        (mkPos arrayNode.Range.StartLine arrayNode.Range.StartColumn)
        (mkPos arrayNode.Range.StartLine (arrayNode.Range.StartColumn + 1))

let rpr =
    mkRange
        arrayNode.Range.FileName
        (mkPos arrayNode.Range.EndLine (arrayNode.Range.EndColumn - 1))
        (mkPos arrayNode.Range.EndLine arrayNode.Range.EndColumn)
