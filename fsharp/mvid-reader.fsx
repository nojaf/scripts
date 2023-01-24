#r "nuget: System.Reflection.Metadata"

open System
open System.IO
open System.Reflection.Metadata
open System.Reflection.PortableExecutable

let getMvid refDll =
    use embeddedReader = new PEReader(File.OpenRead refDll)
    let sourceReader = embeddedReader.GetMetadataReader()
    let loc = sourceReader.GetModuleDefinition().Mvid
    let mvid = sourceReader.GetGuid(loc)
    printfn "%s at %s" (mvid.ToString()) (DateTime.Now.ToString())

let refDll =
    // @"C:\Users\nojaf\Projects\reference-assemblies-sample\A\obj\Debug\net6.0\refint\A.dll"
    // @"C:\Users\nojaf\Projects\telplin\src\Telplin.Core\obj\Debug\net7.0\refint\Telplin.Core.dll"
    @"C:\Users\nojaf\Projects\fantomas\src\Fantomas.FCS\bin\Debug\netstandard2.0\Fantomas.FCS.dll"

getMvid refDll
