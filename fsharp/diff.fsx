#r "nuget: FSharp.Object.Diff, 2.1.0"

open FSharp.Object.Diff

type X = { Y: Y }
and Y = { Z: int }

let left = { Y = { Z = 0 } }
let right = { Y = { Z = 1 } }

ObjectDifferBuilder.BuildDefault().Compare(left, right)
