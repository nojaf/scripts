#r "nuget: Fantomas.Core, 6.0.0-alpha-005"

open FSharp.Compiler.Syntax

let visit (e: SynExpr) =
    match e with
    | SynExpr.ObjExpr(objType = objType; argOptions = args; withKeyword = mWith) -> ()
    | _ -> failwith "todo"
