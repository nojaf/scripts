#r "nuget: System.Collections.Immutable, 7.0.0"

open System
open System.Collections.Generic
open System.Collections.Immutable

type ImmutableArrayBuilderCode<'T> = delegate of byref<ImmutableArray<'T>.Builder> -> unit

type ImmutableArrayViaBuilder<'T>(builder: ImmutableArray<'T>.Builder) =
    member val Builder: ImmutableArray<'T>.Builder = builder with get

    member inline _.Delay([<InlineIfLambda>] f: unit -> ImmutableArrayBuilderCode<'T>) : ImmutableArrayBuilderCode<'T> =
        ImmutableArrayBuilderCode<_>(fun sm -> (f ()).Invoke &sm)

    member inline _.Zero() : ImmutableArrayBuilderCode<'T> =
        ImmutableArrayBuilderCode<_>(fun _sm -> ())

    member inline _.Combine
        (
            [<InlineIfLambda>] part1: ImmutableArrayBuilderCode<'T>,
            [<InlineIfLambda>] part2: ImmutableArrayBuilderCode<'T>
        )
        : ImmutableArrayBuilderCode<'T>
        =
        ImmutableArrayBuilderCode<_>(fun sm ->
            part1.Invoke &sm
            part2.Invoke &sm
        )

    member inline _.While
        (
            [<InlineIfLambda>] condition: unit -> bool,
            [<InlineIfLambda>] body: ImmutableArrayBuilderCode<'T>
        )
        : ImmutableArrayBuilderCode<'T>
        =
        ImmutableArrayBuilderCode<_>(fun sm ->
            while condition () do
                body.Invoke &sm
        )

    member inline _.TryWith
        (
            [<InlineIfLambda>] body: ImmutableArrayBuilderCode<'T>,
            [<InlineIfLambda>] handler: exn -> ImmutableArrayBuilderCode<'T>
        )
        : ImmutableArrayBuilderCode<'T>
        =
        ImmutableArrayBuilderCode<_>(fun sm ->
            try
                body.Invoke &sm
            with exn ->
                (handler exn).Invoke &sm
        )

    member inline _.TryFinally
        (
            [<InlineIfLambda>] body: ImmutableArrayBuilderCode<'T>,
            compensation: unit -> unit
        )
        : ImmutableArrayBuilderCode<'T>
        =
        ImmutableArrayBuilderCode<_>(fun sm ->
            try
                body.Invoke &sm
            with _ ->
                compensation ()
                reraise ()

            compensation ()
        )

    member inline b.Using
        (
            disp: #IDisposable,
            [<InlineIfLambda>] body: #IDisposable -> ImmutableArrayBuilderCode<'T>
        )
        : ImmutableArrayBuilderCode<'T>
        =
        // A using statement is just a try/finally with the finally block disposing if non-null.
        b.TryFinally(
            (fun sm -> (body disp).Invoke &sm),
            (fun () ->
                if not (isNull (box disp)) then
                    disp.Dispose()
            )
        )

    member inline b.For
        (
            sequence: seq<'TElement>,
            [<InlineIfLambda>] body: 'TElement -> ImmutableArrayBuilderCode<'T>
        )
        : ImmutableArrayBuilderCode<'T>
        =
        b.Using(
            sequence.GetEnumerator(),
            (fun e -> b.While((fun () -> e.MoveNext()), (fun sm -> (body e.Current).Invoke &sm)))
        )

    member inline _.Yield(v: 'T) : ImmutableArrayBuilderCode<'T> =
        ImmutableArrayBuilderCode<_>(fun sm -> sm.Add v)

    member inline b.YieldFrom(source: IEnumerable<'T>) : ImmutableArrayBuilderCode<'T> =
        ImmutableArrayBuilderCode<_>(fun sm -> sm.AddRange source)

    member inline b.Run([<InlineIfLambda>] code: ImmutableArrayBuilderCode<'T>) : ImmutableArray<'T> =
        let mutable builder = b.Builder
        code.Invoke &builder
        builder.ToImmutableArray()

let immarray<'T> = ImmutableArrayViaBuilder<'T>(ImmutableArray.CreateBuilder<'T>())

immarray {
    yield 1
    yield 2

    for i in 0..9 do
        yield i
}

let fixedImmarray<'T> capacity =
    ImmutableArrayViaBuilder(ImmutableArray.CreateBuilder<'T>(initialCapacity = capacity))

fixedImmarray 2 {
    yield 1
    yield 2
    yield 3
    yield 4
}


// ====================================================

// type ImmutableArrayBuilder<'T>(?capacity: int) =
//     let builder =
//         match capacity with
//         | None -> ImmutableArray.CreateBuilder<'T>()
//         | Some capacity -> ImmutableArray.CreateBuilder<'T>(capacity)
//
//     member this.Run _ = builder.ToImmutable()
//
//     member this.Yield(item: 'T) = builder.Add(item)
//
//     member this.YieldFrom(items: 'T seq) = builder.AddRange(items)
//
//     member this.Combine(_, r) = r
//
//     member this.Delay f = f ()
//
// let immarray<'T> = ImmutableArrayBuilder<'T>()
// let immarrayWith capacity = ImmutableArrayBuilder<'T>(capacity)
//
// let fixedSample =
//     immarrayWith 3 {
//         yield 1
//         yield! [ 2; 3 ]
//     }
//
// printfn $"Fixed sample has %i{fixedSample.Length} items!"
// let rnd = System.Random()
//
// let randomSample =
//     let items = [ 1 .. rnd.Next(10, 20) ]
//
//     immarray {
//         yield 0
//         yield! items
//     }
//
// printfn $"Random sample has %i{randomSample.Length} items!"
