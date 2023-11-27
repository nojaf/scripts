(*

 _______________________________
< CliWrap and Cool cat cracking >
 -------------------------------
       \  
        \
         \
          \
          |\___/|
         =) oYo (=            
          \  ^  /
           )=*=(       
          /     \
          |     |
         /| | | |\
         \| | |_|/\
         //_// ___/
             \_) 
    
*)
#r "nuget: CliWrap, 3.6.4"

open System.IO
open System.Text.Json
open CliWrap
open CliWrap.Buffered

/// Small disclaimer, this will only work when all dependencies are built in advance.
/// For a more elaborate version, see https://github.com/nojaf/telplin/blob/main/src/Telplin.Core/TypedTree/Options.fs
let crackProjectLikeACoolCat (fsproj: string) =
    let targets =
        "Restore,ResolveAssemblyReferencesDesignTime,ResolveProjectReferencesDesignTime,ResolvePackageDependenciesDesignTime,FindReferenceAssembliesForReferences,_GenerateCompileDependencyCache,_ComputeNonExistentFileProperty,BeforeBuild,BeforeCompile,CoreCompile"

    let properties =
        "/p:DesignTimeBuild=True /p:SkipCompilerExecution=True /p:ProvideCommandLineArgs=True"

    // Thanks Chester
    let millionDollarFeature = " --getItem:FscCommandLineArgs"

    let result =
        Cli
            .Wrap("dotnet")
            .WithWorkingDirectory(Path.GetDirectoryName fsproj)
            .WithArguments($"msbuild /t:%s{targets} %s{properties} %s{millionDollarFeature}")
            .ExecuteBufferedAsync()
            .Task.Result

    let jsonDocument = JsonDocument.Parse result.StandardOutput

    let options =
        jsonDocument.RootElement
            .GetProperty("Items")
            .GetProperty("FscCommandLineArgs")
            .EnumerateArray()
        |> Seq.map (fun arg -> arg.GetProperty("Identity").GetString())
        |> Seq.toArray

    Array.iter (printfn "%s") options

crackProjectLikeACoolCat @"C:\Users\nojaf\Projects\telplin\src\Telplin.Core\Telplin.Core.fsproj"























(*

 _______________________
/ Downloading SDKs with \
\        FsHttp         /
 -----------------------
 \     /\  ___  /\
  \   // \/   \/ \\
     ((    o o    ))
      \\ /     \ //
       \/  | |  \/ 
        |  | |  |  
        |  | |  |  
        |   o   |  
        | |   | |  
        |m|   |m|  

*)


#r "nuget: FsHttp, 12.0.0"

open System
open FsHttp

// View the download URL
let directDownloadUrl =
    http {
        HEAD "https://aka.ms/dotnet/8.0.2xx/daily/dotnet-sdk-win-x64.zip"

        config_transformHttpClientHandler (fun client ->
            client.AllowAutoRedirect <- false
            client
        )
    }
    |> Request.send
    |> fun response -> response.headers.Location

// Download the SDK
let destinationPath =
    Path.Combine(@"C:\Users\nojaf\Downloads\SDKS", Path.GetFileName(directDownloadUrl.LocalPath))

let extractionFolder =
    Path.Combine(Path.GetDirectoryName(destinationPath), Path.GetFileNameWithoutExtension(destinationPath))

if Directory.Exists extractionFolder then
    printfn "Already extracted %s" extractionFolder
else
    get (string<Uri> directDownloadUrl)
    |> Request.send
    |> Response.saveFile destinationPath

    // Unzip
    Compression.ZipFile.ExtractToDirectory(destinationPath, extractionFolder)

Cli
    .Wrap(Path.Combine(extractionFolder, "dotnet.exe"))
    .WithArguments("--version")
    .WithWorkingDirectory(extractionFolder)
    .ExecuteBufferedAsync()
    .Task.Result.StandardOutput
|> printfn "Downloaded: %s"


























(*

 _____________________________
< Copy stuff to the clipboard >
 -----------------------------
   \
    \
     \
                '-.
      .---._     \ .--'
    /       `-..__)  ,-'
   |    0           /
    --.__,   .__.,`
     `-.___'._\_.'


*)


#r "nuget: TextCopy, 6.1.0"

open TextCopy

let mkMarker (text: string) =
    let lat, lng = text.Split(',').[0], text.Split(',').[1]
    $"<Marker latitude={{{lat}}} longitude={{{lng}}} />" |> ClipboardService.SetText

mkMarker "48.82900355501366, 2.0439287083823956"

// <Marker latitude={48.82900355501366} longitude={ 2.0439287083823956} />






















(*

 _________________________
< Parsing JSON with Thoth >
 -------------------------
         \
          \
           ___
          (o o)
         (  V  )
        /--m-m-

*)

#r "nuget: Thoth.Json.Net, 11.0.0"

open Thoth.Json.Net

let decoder: string -> JsonValue -> Result<float * float, DecoderError> =
    Decode.object (fun get -> get.Required.Field "lat" Decode.float, get.Required.Field "lng" Decode.float)

let sample =
    """
{
    "lat": 48.82900355501366,
    "lng": 2.0439287083823956
}
"""

match Decode.fromString decoder sample with
| Error error -> printfn "ERROR: %A" error
| Ok(lat, lng) -> printfn "Decoded %f , %f" lat lng


























(*

 ____________________
< Trim lines of file >
 --------------------
       \
        \
         \  _))
           > o\     _~
           `;'\\__-' \_
              | )  _ \ \
             / / ``   w w
            w w

*)

let trimLines (path: string) =
    let lines = File.ReadAllLines(path)
    let trimmed = lines |> Array.map (fun line -> line.TrimEnd())
    File.WriteAllLines(path, trimmed)

trimLines @"C:\Users\nojaf\Projects\fsharp\src\Compiler\AbstractIL\ilread.fsi"




























(*

 ______________________________
/ Format code programmatically \
\ with Fantomas                /
 ------------------------------
    \
     \
                  #[/[#:xxxxxx:#[/[\x
             [/\ &3N            W3& \/[x
          [[x@W                      W@x[[\
        /#&N                             N_#
      /#@                                  @#/x
    [/ NH_  ^@W               Nd_  ^@p      N /#
   [[d@#_ zz@[/x3           3x:d9zz \/#_N     d[[
  /[3^[JMMMJ/////&         ^#NMMMMM ////#W     H[[
 [/@p/NMMMML@#[:^/3       d/JMMMMMMEx[# x\      &/#
 /x &/LMMMMMMMMMM[_       x:MMMMMMMMMMMM /p      :/
[/d d/ELLLLLLLLLD/&        #LLLLLLLLLLLL3/N      d/[
//N   xxxxxxxxxxxxN       Wxxxxxxxxxxxxxx_       W//
/[                                                //
//N   p333333333333333333333333333333333p        W//
[/d   _^/#\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\/H       @/[
 /:     \#                              [x       :/
 [/@    d/x                             #:      &/#
  [[H    ^[x                            [      H[[
   [[d    _[x            &Hppp3d_      #\N    @[[
    [/ N   d#\        &NzDDDDDDDDJp^ x[xN   N /#
      /#&   N [:     pDDDDDDDDDDDDJ&#:H    &#/
       :/#_W  W^##x 3DDDDDDDDDJN&:\^p   W_#/
          [[x&W  p& xx ^^^^ x:x @W   W&x/[
             [/# &HW   WWWWN    WH& #/[
                 [/[#\xxxxxx\#[/[\x^@


*)

#r "nuget: Fantomas.Core, 5.2.0"

open Fantomas.Core

CodeFormatter.FormatDocumentAsync(
    false,
    """
let process  :
    (* foo *)
    int = 0
"""
)
|> Async.RunSynchronously


























(*

 ___________________
/ Using scripts in  \
\ FSharp.Formatting /
 -------------------
        \   ^__^
         \  (oo)\_______
            (__)\       )\/\
                ||----w |
                ||     ||

*)

// Demo of using F# script as documentation using FSharp.Formatting
// Adding a script to Telplin









// https://cowsay-svelte.vercel.app/
