#r "nuget: Mono.Cecil, 0.11.4"

open Mono.Cecil

let printAssemblyContent (path: string) =
    let assembly = AssemblyDefinition.ReadAssembly(path)
    printfn "%s" assembly.FullName
    let m = Seq.head assembly.Modules
    let t = m.Types |> Seq.filter (fun t -> not (Seq.isEmpty t.Methods)) |> Seq.head
    let method = Seq.head t.Methods
    printfn "%A" method.Body.Instructions

printAssemblyContent
    @"C:\Users\nojaf\Projects\fsharp\artifacts\obj\FSharp.Compiler.Service\Debug\netstandard2.0\FSharp.Compiler.Service-1.dll"

printAssemblyContent
    @"C:\Users\nojaf\Projects\fsharp\artifacts\obj\FSharp.Compiler.Service\Debug\netstandard2.0\FSharp.Compiler.Service-3.dll"
