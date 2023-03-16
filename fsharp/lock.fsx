open System
open System.Collections.Concurrent

type Node =
    {
        lockObj: obj
        mutable value: int
        Events: ConcurrentQueue<DateTime * string>
    }

    member this.SetValue(v: int) =
        this.Events.Enqueue(DateTime.Now, $"Received %i{v}")

        lock
            this.lockObj
            (fun () ->
                if v < this.value || this.value = 0 then
                    this.value <- v
            )

let rnd = new Random()

let node =
    {
        lockObj = obj ()
        value = 0
        Events = ConcurrentQueue()
    }

[ 1..100 ]
|> List.map (fun idx ->
    async {
        do! Async.Sleep(rnd.Next(2000))
        node.SetValue(idx)
        do! Async.Sleep(rnd.Next(1000))
        node.SetValue(idx)
    }
)
|> Async.Parallel
|> Async.Ignore
|> Async.RunSynchronously

for ev in node.Events do
    printfn "%A" ev

node.Events.Count
node.value
