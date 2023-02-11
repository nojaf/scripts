#r "nuget: Humanizer.Core, 2.14.1"
#r "nuget: Fantomas.Core, 6.0.0-alpha-003"

open System.IO
open Fantomas.Core
open Fantomas.Core.SyntaxOak
open Humanizer

// Directory.EnumerateFiles(@"C:\Users\nojaf\Projects\fsharp\tests\service\data\SyntaxTree")
// |> Seq.iter File.Delete

type System.String with

    member x.Strip(content: string seq) =
        (x, content) ||> Seq.fold (fun acc toStrip -> acc.Replace(toStrip, ""))

type TestData =
    {
        Name: string
        Content: string
        Folder: string
        IsSignatureFile: bool
    }

    member x.FileName =
        let extension = if x.IsSignatureFile then ".fsi" else ".fs"

        let fileName =
            x.Name.Strip([| "`"; "."; "#"; ",Signature"; "/"; ","; "::" |]).Trim().Pascalize()

        $"%s{fileName}%s{extension}"

let testFiles =
    Directory.EnumerateFiles(@"C:\Users\nojaf\Projects\safesparrow-fsharp\tests\service\SyntaxTreeTests")
    |> Seq.toArray

let hasTestAttribute (ma: MultipleAttributeListNode option) =
    match ma with
    | None -> false
    | Some ma ->
        ma.AttributeLists
        |> List.exists (fun al ->
            al.Attributes
            |> List.exists (fun a ->
                a.TypeName.Content
                |> List.exists (
                    function
                    | IdentifierOrDot.Ident ident -> ident.Text = "Test"
                    | _ -> false
                )
            )
        )

let parseFunctions =
    set
        [|
            "getParseResults"
            "getParseResultsOfSignatureFile"
            "getCommentTrivia"
            "getDirectiveTrivia"
        |]

let (|ParseFunction|_|) (e: Expr) =
    match e with
    | Expr.Ident parseFunctionName ->
        if parseFunctions.Contains parseFunctionName.Text then
            Some(parseFunctionName.Text = "getParseResultsOfSignatureFile")
        else
            None
    | _ -> None

let (|StringConstant|_|) (e: Expr) =
    match e with
    | Expr.Constant(Constant.FromText sourceCode) ->
        let text = sourceCode.Text
        let text = if text.StartsWith("\"\"\"") then text.Substring(3) else text
        let text = if text.EndsWith("\"\"\"") then text.Substring(0, text.Length - 3) else text
        let text = if text.StartsWith("\"") then text.Substring(1) else text
        let text = if text.EndsWith("\"") then text.Substring(0, text.Length - 1) else text
        let text = text.Trim()
        Some text
    | _ -> None

let (|BoolConstant|_|) (e: Expr) =
    match e with
    | Expr.Constant(Constant.FromText sourceCode) ->
        if sourceCode.Text = "true" || sourceCode.Text = "false" then
            Some(sourceCode.Text = "true")
        else
            None
    | _ -> None

let getTestData (filePath) (bodyExpr: Expr) : bool * string =
    match bodyExpr with
    | Expr.CompExprBody compBody ->
        let sourceOpt =
            compBody.Statements
            |> List.tryPick (
                function
                | ComputationExpressionStatement.LetOrUseStatement letOrUseNode ->
                    match letOrUseNode.Binding.Expr with
                    | Expr.App appNode ->
                        match appNode.FunctionExpr, appNode.Arguments with
                        | ParseFunction isSignatureFile, [ StringConstant sourceCode ] ->
                            Some(isSignatureFile, sourceCode)
                        | ParseFunction _, [ BoolConstant isSignatureFile; StringConstant sourceCode ] ->
                            Some(isSignatureFile, sourceCode)
                        | _ -> None
                    | Expr.InfixApp infixApp ->
                        match infixApp.LeftHandSide, infixApp.RightHandSide with
                        | StringConstant sourceCode, ParseFunction isSignatureFile -> Some(isSignatureFile, sourceCode)
                        | _ -> None
                    | _ -> None
                | _ -> None
            )

        match sourceOpt with
        | None -> failwithf "Could not find test data for %A in %s" (Expr.Node bodyExpr).Range filePath
        | Some source -> source

    | _ -> failwith "Expect different AST"

let testData =
    testFiles
    |> Seq.collect (fun filePath ->
        let content = File.ReadAllText filePath

        let ast =
            CodeFormatter.ParseOakAsync(false, content)
            |> Async.RunSynchronously
            |> Array.head
            |> fst

        ast.ModulesOrNamespaces.[0].Declarations
        |> List.choose (
            function
            | ModuleDecl.TopLevelBinding binding when hasTestAttribute binding.Attributes ->
                match binding.FunctionName with
                | Choice2Of2 _ -> None
                | Choice1Of2 identifier ->
                    match identifier.Content with
                    | [ IdentifierOrDot.Ident testName ] ->
                        let isSignature, content = getTestData filePath binding.Expr

                        Some
                            {
                                Name = testName.Text
                                IsSignatureFile = isSignature
                                Content = content
                                Folder = Path.GetFileNameWithoutExtension(filePath).Replace("Tests", "")
                            }
                    | _ -> None
            | _ -> None
        )
    )
    |> Seq.toArray

for testData in testData do
    let path =
        Path.Combine(@"C:\Users\nojaf\Projects\fsharp\tests\service\data\SyntaxTree", $"{testData.FileName}.bsl")

    if File.Exists path then
        let target = Path.Combine(@"C:\Users\nojaf\Projects\fsharp\tests\service\data\SyntaxTree", testData.Folder,  $"{testData.FileName}.bsl")
        File.Move(path, target)
    

Directory.EnumerateFiles(@"C:\Users\nojaf\Projects\fsharp\tests\service\data\SyntaxTree", "*.fs?", searchOption = SearchOption.AllDirectories)
|> Seq.length