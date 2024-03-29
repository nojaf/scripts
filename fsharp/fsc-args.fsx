#r "nuget: Fantomas.Core"

open System
open System.IO
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text
open FSharp.Compiler.Xml
open Fantomas.Core

let (</>) a b = Path.Combine(a, b)

let args =
    """
-o:C:\Users\nojaf\Projects\safesparrow-fsharp\artifacts\obj\DiamondTest\Debug\net7.0\DiamondTest.dll
-g
--debug:portable
--embed:C:\Users\nojaf\Projects\safesparrow-fsharp\artifacts\obj\DiamondTest\Debug\net7.0\buildproperties.fs
--embed:C:\Users\nojaf\Projects\safesparrow-fsharp\artifacts\obj\DiamondTest\Debug\net7.0\DiamondTest.AssemblyInfo.fs
--sourcelink:C:\Users\nojaf\Projects\safesparrow-fsharp\artifacts\obj\DiamondTest\Debug\net7.0\DiamondTest.sourcelink.json
--noframework
--define:TRACE
--define:DEBUG
--define:NETSTANDARD
--define:FX_NO_WINFORMS
--define:Debug
--define:NET
--define:NET7_0
--define:NETCOREAPP
--define:NET5_0_OR_GREATER
--define:NET6_0_OR_GREATER
--define:NET7_0_OR_GREATER
--define:NETCOREAPP1_0_OR_GREATER
--define:NETCOREAPP1_1_OR_GREATER
--define:NETCOREAPP2_0_OR_GREATER
--define:NETCOREAPP2_1_OR_GREATER
--define:NETCOREAPP2_2_OR_GREATER
--define:NETCOREAPP3_0_OR_GREATER
--define:NETCOREAPP3_1_OR_GREATER
--doc:C:\Users\nojaf\Projects\safesparrow-fsharp\artifacts\obj\DiamondTest\Debug\net7.0\DiamondTest.xml
--keyfile:C:\Users\nojaf\.nuget\packages\microsoft.dotnet.arcade.sdk\8.0.0-beta.22512.1\tools\snk/MSFT.snk
--publicsign+
--optimize-
--tailcalls-
-r:C:\Users\nojaf\.nuget\packages\fsharp.core\6.0.6\lib\netstandard2.1\FSharp.Core.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\Microsoft.CSharp.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\Microsoft.VisualBasic.Core.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\Microsoft.VisualBasic.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\Microsoft.Win32.Primitives.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\Microsoft.Win32.Registry.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\mscorlib.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\netstandard.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.AppContext.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Buffers.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Collections.Concurrent.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Collections.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Collections.Immutable.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Collections.NonGeneric.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Collections.Specialized.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.ComponentModel.Annotations.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.ComponentModel.DataAnnotations.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.ComponentModel.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.ComponentModel.EventBasedAsync.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.ComponentModel.Primitives.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.ComponentModel.TypeConverter.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Configuration.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Console.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Core.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Data.Common.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Data.DataSetExtensions.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Data.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Diagnostics.Contracts.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Diagnostics.Debug.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Diagnostics.DiagnosticSource.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Diagnostics.FileVersionInfo.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Diagnostics.Process.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Diagnostics.StackTrace.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Diagnostics.TextWriterTraceListener.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Diagnostics.Tools.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Diagnostics.TraceSource.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Diagnostics.Tracing.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Drawing.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Drawing.Primitives.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Dynamic.Runtime.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Formats.Asn1.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Formats.Tar.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Globalization.Calendars.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Globalization.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Globalization.Extensions.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.IO.Compression.Brotli.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.IO.Compression.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.IO.Compression.FileSystem.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.IO.Compression.ZipFile.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.IO.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.IO.FileSystem.AccessControl.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.IO.FileSystem.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.IO.FileSystem.DriveInfo.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.IO.FileSystem.Primitives.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.IO.FileSystem.Watcher.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.IO.IsolatedStorage.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.IO.MemoryMappedFiles.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.IO.Pipes.AccessControl.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.IO.Pipes.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.IO.UnmanagedMemoryStream.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Linq.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Linq.Expressions.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Linq.Parallel.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Linq.Queryable.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Memory.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Net.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Net.Http.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Net.Http.Json.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Net.HttpListener.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Net.Mail.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Net.NameResolution.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Net.NetworkInformation.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Net.Ping.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Net.Primitives.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Net.Quic.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Net.Requests.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Net.Security.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Net.ServicePoint.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Net.Sockets.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Net.WebClient.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Net.WebHeaderCollection.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Net.WebProxy.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Net.WebSockets.Client.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Net.WebSockets.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Numerics.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Numerics.Vectors.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.ObjectModel.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Reflection.DispatchProxy.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Reflection.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Reflection.Emit.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Reflection.Emit.ILGeneration.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Reflection.Emit.Lightweight.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Reflection.Extensions.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Reflection.Metadata.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Reflection.Primitives.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Reflection.TypeExtensions.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Resources.Reader.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Resources.ResourceManager.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Resources.Writer.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Runtime.CompilerServices.Unsafe.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Runtime.CompilerServices.VisualC.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Runtime.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Runtime.Extensions.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Runtime.Handles.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Runtime.InteropServices.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Runtime.InteropServices.JavaScript.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Runtime.InteropServices.RuntimeInformation.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Runtime.Intrinsics.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Runtime.Loader.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Runtime.Numerics.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Runtime.Serialization.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Runtime.Serialization.Formatters.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Runtime.Serialization.Json.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Runtime.Serialization.Primitives.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Runtime.Serialization.Xml.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Security.AccessControl.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Security.Claims.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Security.Cryptography.Algorithms.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Security.Cryptography.Cng.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Security.Cryptography.Csp.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Security.Cryptography.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Security.Cryptography.Encoding.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Security.Cryptography.OpenSsl.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Security.Cryptography.Primitives.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Security.Cryptography.X509Certificates.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Security.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Security.Principal.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Security.Principal.Windows.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Security.SecureString.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.ServiceModel.Web.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.ServiceProcess.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Text.Encoding.CodePages.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Text.Encoding.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Text.Encoding.Extensions.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Text.Encodings.Web.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Text.Json.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Text.RegularExpressions.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Threading.Channels.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Threading.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Threading.Overlapped.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Threading.Tasks.Dataflow.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Threading.Tasks.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Threading.Tasks.Extensions.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Threading.Tasks.Parallel.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Threading.Thread.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Threading.ThreadPool.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Threading.Timer.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Transactions.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Transactions.Local.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.ValueTuple.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Web.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Web.HttpUtility.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Windows.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Xml.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Xml.Linq.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Xml.ReaderWriter.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Xml.Serialization.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Xml.XDocument.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Xml.XmlDocument.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Xml.XmlSerializer.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Xml.XPath.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\System.Xml.XPath.XDocument.dll
-r:C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.0-rc.2.22472.3\ref\net7.0\WindowsBase.dll
--target:exe
--nowarn:FS2003,NU5105,IL2121
--warn:3
--warnaserror:3239,1182,0025
--fullpaths
--flaterrors
--highentropyva+
--targetprofile:netcore
--nocopyfsharpcore
--deterministic+
--simpleresolution
--nowarn:3384
--times
--nowarn:75
--simpleresolution
--refout:C:\Users\nojaf\Projects\safesparrow-fsharp\artifacts\obj\DiamondTest\Debug\net7.0\refint\DiamondTest.dll
C:\Users\nojaf\Projects\safesparrow-fsharp\artifacts\obj\DiamondTest\Debug\net7.0\buildproperties.fs
C:\Users\nojaf\Projects\safesparrow-fsharp\artifacts\obj\DiamondTest\Debug\net7.0\.NETCoreApp,Version=v7.0.AssemblyAttributes.fs
C:\Users\nojaf\Projects\safesparrow-fsharp\artifacts\obj\DiamondTest\Debug\net7.0\DiamondTest.AssemblyInfo.fs
A.fsi
A.fs
B1.fs
B2.fs
C1.fs
C2.fs
D.fs
Program.fs
"""

let projectFolder = @"C:\Users\nojaf\Projects\safesparrow-fsharp\tests\DiamondTest"

let mapFile projectFolder (line: string) =
    let parts = line.Split(':')

    if parts.Length > 2 then
        line
    elif parts.Length = 1 then
        let isRelative = not (Path.IsPathRooted parts.[0])
        let isFile = parts.[0].Contains(".")

        if isFile && isRelative then
            projectFolder </> line
        else
            line
    else

    let isRelative = not (Path.IsPathRooted parts.[1])
    let isFile = parts.[1].Contains(".")

    if isFile && isRelative then
        let rooted = projectFolder </> parts.[1]
        $"{parts.[0]}:{rooted}"
    else
        line

let mkConstString v =
    SynExpr.Const(SynConst.String(v, SynStringKind.Verbatim, Range.Zero), Range.Zero)

let mkArrayExpr xs =
    SynExpr.ArrayOrList(true, Array.toList xs, Range.Zero)

let trimLine (s: string) = s.Trim()

let updateArgsInFile projectFolder argsFile =
    File.ReadAllLines argsFile
    |> Array.map (trimLine >> mapFile projectFolder)
    |> String.concat "\n"
    |> fun content -> File.WriteAllText(argsFile, content)

updateArgsInFile
    @"C:\Users\nojaf\Projects\safesparrow-fsharp\tests\TcSignatureRepro"
    @"C:\Users\nojaf\Projects\safesparrow-fsharp\tests\TcSignatureRepro\args.txt"

args.Split([| "\n" |], StringSplitOptions.RemoveEmptyEntries)
|> Array.map (trimLine >> mapFile projectFolder >> mkConstString)
|> mkArrayExpr
|> fun expr ->
    ParsedInput.ImplFile(
        ParsedImplFileInput(
            "filename.fsx",
            true,
            QualifiedNameOfFile(Ident("", Range.Zero)),
            [],
            [],
            [
                SynModuleOrNamespace(
                    [],
                    false,
                    SynModuleOrNamespaceKind.AnonModule,
                    [ SynModuleDecl.Expr(expr, Range.Zero) ],
                    PreXmlDoc.Empty,
                    [],
                    None,
                    Range.Zero,
                    {
                        ModuleKeyword = None
                        NamespaceKeyword = None
                    }
                )
            ],
            (false, false),
            {
                ConditionalDirectives = []
                CodeComments = []
            }
        )
    )
|> fun ast ->
    CodeFormatter.FormatASTAsync(
        ast,
        config =
            { FormatConfig.FormatConfig.Default with
                MultilineBlockBracketsOnSameColumn = true
            }
    )
|> Async.RunSynchronously
|> printfn "%s"
