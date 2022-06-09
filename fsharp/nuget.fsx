#r "nuget: NuGet.Protocol"

open System
open System.Threading
open NuGet.Common
open NuGet.Versioning
open NuGet.Protocol
open NuGet.Protocol.Core.Types

type SemanticVersion with

    member this.Alpha: int option =
        if isNull this.Release then
            None
        else
            this.Release.Replace("alpha-", "")
            |> Int32.TryParse
            |> fun (success, alpha) -> if success then Some alpha else None

async {
    let logger = NullLogger.Instance
    let cancellationToken = CancellationToken.None

    let cache = new SourceCacheContext()

    let repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json")

    let! resource =
        repository.GetResourceAsync<FindPackageByIdResource>()
        |> Async.AwaitTask

    let! versions =
        resource.GetAllVersionsAsync("fantomas-tool", cache, logger, cancellationToken)
        |> Async.AwaitTask

    versions
    |> Seq.filter (fun v ->
        v.Major = 4
        && v.Minor = 6
        && (Option.map (fun alpha -> alpha >= 4) v.Alpha
            |> Option.defaultValue true)
    )
    |> Seq.iter (fun v -> printfn "%A" v.Release)
}
|> Async.RunSynchronously


async {
    // Elmish
    let logger = NullLogger.Instance
    let cancellationToken = CancellationToken.None

    let cache = new SourceCacheContext()

    let repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json")

    let! findPackageByIdResource =
        repository.GetResourceAsync<FindPackageByIdResource>()
        |> Async.AwaitTask

    let! elmish =
        let version = NuGetVersion.Parse("3.1")

        findPackageByIdResource.GetDependencyInfoAsync("Elmish", version, cache, logger, cancellationToken)
        |> Async.AwaitTask

    let! packageMetadataResource =
        repository.GetResourceAsync<PackageMetadataResource>()
        |> Async.AwaitTask

    let! meta =
        packageMetadataResource.GetMetadataAsync(elmish.PackageIdentity, cache, logger, cancellationToken)
        |> Async.AwaitTask

    printfn "%A" meta.LicenseUrl

    ()
}
|> Async.RunSynchronously

type LicenseInformationResponse =
    { PackageId: string
      Version: string
      License: string option
      LicenseUrl: string }

let getLicensesInfo (packages: (string * string) seq) : Async<LicenseInformationResponse array> =
    async {
        let logger = NullLogger.Instance
        let cancellationToken = CancellationToken.None

        let cache = new SourceCacheContext()

        let repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json")

        let! findPackageByIdResource =
            repository.GetResourceAsync<FindPackageByIdResource>()
            |> Async.AwaitTask

        let! packageMetadataResource =
            repository.GetResourceAsync<PackageMetadataResource>()
            |> Async.AwaitTask

        let findPackage (packageId, version) =
            async {
                let version = NuGetVersion.Parse(version)

                let! packageInfo =
                    findPackageByIdResource.GetDependencyInfoAsync(packageId, version, cache, logger, cancellationToken)
                    |> Async.AwaitTask

                let! meta =
                    packageMetadataResource.GetMetadataAsync(
                        packageInfo.PackageIdentity,
                        cache,
                        logger,
                        cancellationToken
                    )
                    |> Async.AwaitTask

                return
                    { PackageId = packageInfo.PackageIdentity.Id
                      Version = string packageInfo.PackageIdentity.Version
                      License =
                        meta.LicenseMetadata
                        |> Option.ofObj
                        |> Option.map (fun license -> string license.LicenseExpression)
                      LicenseUrl = string meta.LicenseUrl }
            }

        return! packages |> Seq.map findPackage |> Async.Parallel
    }

getLicensesInfo
    [ "CliWrap", "3.3.3"
      "CommandLineParser.FSharp", "2.8" ]
|> Async.RunSynchronously
