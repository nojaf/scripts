#r "nuget: FSharp.Compiler.Service"

open FSharp.Compiler.Text
open FSharp.Compiler.Text.Range
open FSharp.Compiler.Text.Position

type NodeTrivia =
    {
        StartPos: pos
        FileName: string
    }

    member this.Range =
        mkRange this.FileName this.StartPos (mkPos this.StartPos.Line (this.StartPos.Column + 4))

let myTrivia =
    {
        StartPos = mkPos 1 0
        FileName = "MyFile"
    }

myTrivia.Range
