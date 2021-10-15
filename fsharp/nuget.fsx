#r "nuget: NuGet.Protocol"

open System
open System.Threading
open NuGet.Common
open NuGet.Versioning
open NuGet.Protocol
open NuGet.Protocol.Core.Types

let fourSixAlphaOne = SemanticVersion.Parse("4.6.0-alpha-001")
let fourSixAlphaThree = SemanticVersion.Parse("4.6.0-alpha-003")
let fourSixBetaThree = SemanticVersion.Parse("4.6.0-alpha-004")

fourSixAlphaThree.CompareTo fourSixAlphaOne

let private replaceAlphaBetaAndDashes (v: string) =
    System.Text.RegularExpressions.Regex.Replace(v, "(\\-|alpha|beta)*", "")

let (|Stable|Alpha|Beta|) (v: SemanticVersion) : Choice<unit, int, int> =
    if v.Release.StartsWith("alpha") then
        replaceAlphaBetaAndDashes v.Release
        |> Int32.Parse
        |> Choice2Of3
    elif v.Release.StartsWith("beta") then
        replaceAlphaBetaAndDashes v.Release
        |> Int32.Parse
        |> Choice3Of3
    else
        Choice1Of3()

let isNewerOrSameVersion (source: SemanticVersion) (target: SemanticVersion) : bool =
    let isExactlyTheSame =
        source.Major = target.Major
        && source.Minor = target.Minor
        && source.Patch = target.Patch
        && source.Release = target.Release

    let majorIsHigher = source.Major < target.Major

    let minorIsHigher =
        source.Major = target.Major
        && source.Minor < target.Minor

    let patchIsHigher =
        source.Major = target.Major
        && source.Minor = target.Minor
        && source.Patch < target.Patch

    let prereleaseIsHigher =
        source.Major = target.Major
        && source.Minor = target.Minor
        && source.Patch = target.Patch
        && (
            match source, target with
            | Beta _, Alpha _ -> false
            | Alpha _, Beta _ -> true
            | Alpha sa, Alpha ta -> ta > sa
            | Beta sb, Beta tb -> tb > sb
            | _ -> false
        )

    isExactlyTheSame
    || majorIsHigher
    || minorIsHigher
    || patchIsHigher
    || prereleaseIsHigher

async {
    let logger = NullLogger.Instance
    let cancellationToken = CancellationToken.None

    let cache = new SourceCacheContext()

    let repository =
        Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json")

    let! resource =
        repository.GetResourceAsync<FindPackageByIdResource>()
        |> Async.AwaitTask

    let! versions =
        resource.GetAllVersionsAsync(
            "fantomas-tool",
            cache,
            logger,

            cancellationToken
        )
        |> Async.AwaitTask

    versions
    |> Seq.filter (isNewerOrSameVersion fourSixBetaThree)
    |> Seq.iter (printfn "%A")
}
|> Async.RunSynchronously
