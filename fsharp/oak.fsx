﻿#r "nuget: Fantomas.Core,6.0.0-alpha-006"

open Fantomas.Core
open Fantomas.Core.SyntaxOak

let zeroRange = FSharp.Compiler.Text.Range.Zero

let binding =
    BindingNode(
        xmlDoc = None,
        attributes = None,
        leadingKeyword = MultipleTextsNode([ SingleTextNode("let", zeroRange) ], zeroRange),
        isMutable = true,
        inlineNode = None,
        accessibility = None,
        functionName = Choice1Of2(IdentListNode([ IdentifierOrDot.Ident(SingleTextNode("a", zeroRange)) ], zeroRange)),
        genericTypeParameters = None,
        parameters = [],
        returnType = None,
        equals = SingleTextNode("=", zeroRange),
        expr = Expr.Constant(Constant.FromText(SingleTextNode("7", zeroRange))),
        range = zeroRange
    )

let file =
    Oak(
        [],
        [
            ModuleOrNamespaceNode(None, [ ModuleDecl.TopLevelBinding binding ], zeroRange)
        ],
        zeroRange
    )

CodeFormatter.FormatOakAsync(file) |> Async.RunSynchronously
