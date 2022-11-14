#r "nuget: FSharp.Compiler.Service, 39.0.0"
#r "nuget: Fantomas, 4.5.0-beta-001"

open Fantomas
open System.IO
open FSharp.Compiler.SyntaxTree
open FSharp.Compiler.Text
open FSharp.Compiler.SourceCodeServices

let checker = FSharpChecker.Create()

let config = { FormatConfig.FormatConfig.Default with StrictMode = true }

type ASTFragment = Fragment of ParsedInput * Range

let updateModuleInImpl (ast: ParsedInput) (mdl: SynModuleOrNamespace) : ParsedInput =
    match ast with
    | ParsedInput.SigFile _ -> ast
    | ParsedInput.ImplFile(ParsedImplFileInput(fileName,
                                               isScript,
                                               qualifiedNameOfFile,
                                               scopedPragmas,
                                               hashDirectives,
                                               _,
                                               isLastAndCompiled)) ->

    ParsedImplFileInput(
        fileName,
        isScript,
        qualifiedNameOfFile,
        scopedPragmas,
        hashDirectives,
        [ mdl ],
        isLastAndCompiled
    )
    |> ParsedInput.ImplFile

let updateModuleInSig (ast: ParsedInput) (mdl: SynModuleOrNamespaceSig) : ParsedInput =
    match ast with
    | ParsedInput.ImplFile _ -> ast
    | ParsedInput.SigFile(ParsedSigFileInput(fileName, qualifiedNameOfFile, scopedPragmas, hashDirectives, _)) ->

    ParsedSigFileInput(fileName, qualifiedNameOfFile, scopedPragmas, hashDirectives, [ mdl ])
    |> ParsedInput.SigFile

let splitModule (ast: ParsedInput) (mn: SynModuleOrNamespace) : ASTFragment list =
    match mn with
    | SynModuleOrNamespace.SynModuleOrNamespace(lid, isRec, kind, decls, xmlDoc, attribs, ao, range) ->

    decls
    |> List.map (fun d ->
        let parsedInput =
            SynModuleOrNamespace(lid, isRec, kind, [ d ], xmlDoc, attribs, ao, range)
            |> updateModuleInImpl ast

        ASTFragment.Fragment(parsedInput, d.Range)
    )

let splitModuleSig (ast: ParsedInput) (mn: SynModuleOrNamespaceSig) : ASTFragment list =
    match mn with
    | SynModuleOrNamespaceSig.SynModuleOrNamespaceSig(lid, isRec, kind, decls, xmlDoc, attribs, ao, range) ->

    decls
    |> List.map (fun d ->
        let parsedInput =
            SynModuleOrNamespaceSig(lid, isRec, kind, [ d ], xmlDoc, attribs, ao, range)
            |> updateModuleInSig ast

        ASTFragment.Fragment(parsedInput, d.Range)
    )

let splitParsedInput (ast: ParsedInput) : ASTFragment list =
    match ast with
    | ParsedInput.ImplFile(ParsedImplFileInput.ParsedImplFileInput(modules = modules)) ->
        modules |> List.collect (splitModule ast)
    | ParsedInput.SigFile(ParsedSigFileInput.ParsedSigFileInput(modules = modules)) ->
        modules |> List.collect (splitModuleSig ast)

let formatFragment (defines: string list) (Fragment(ast, range)) (fileName: string) : Async<unit> =
    async {
        try
            let! formatted = CodeFormatter.FormatASTAsync(ast, fileName, defines, None, config)
            File.WriteAllText(fileName, formatted)
        with ex ->
            let errorFile = Path.GetFileNameWithoutExtension(fileName) |> sprintf "%s_error.txt"

            let errorLog =
                $"""Unable to format %s{fileName}
    Original range: %A{range}
    Exception: %A{ex}
    """

            File.WriteAllText(errorFile, errorLog)
    }

let formatFragments (filePath: string) : unit =
    let extension = Path.GetExtension(filePath)

    let fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath)

    let sourceOrigin = File.ReadAllText filePath |> SourceOrigin.SourceString

    let parsingOptions =
        { FSharpParsingOptions.Default with SourceFiles = [| filePath |] }

    let ast =
        CodeFormatter.ParseAsync(filePath, sourceOrigin, parsingOptions, checker)
        |> Async.RunSynchronously

    let fragments: (string list * ASTFragment list) list =
        ast
        |> Seq.map (fun (ast, defines) -> defines, splitParsedInput ast)
        |> Seq.toList

    let fragmentFolder = Path.GetFullPath filePath |> Path.GetFileNameWithoutExtension

    if not (Directory.Exists(fragmentFolder)) then
        Directory.CreateDirectory(fragmentFolder) |> ignore

    match fragments with
    | [] -> ()
    | [ [], fragments ] ->
        let fragmentFileName idx =
            $"%s{fileNameWithoutExtension}_%i{idx}%s{extension}"

        fragments
        |> List.mapi (fun idx fragment -> formatFragment [] fragment (fragmentFileName idx))
        |> Async.Parallel
        |> Async.Ignore
        |> Async.RunSynchronously
    | fragmentsWithDefines -> ()
