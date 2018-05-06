module Main.Tests.ProjectParserTests

open Main
open Main.Tests.Common
open System
open System.IO
open Microsoft.FSharp.Compiler.SourceCodeServices
open SimpleTest

let ``test parsing a JSON project file`` (t: TestContext) = 
    let json = """
    {
      "version": 3,
      "targets": {
        ".NETCoreApp,Version=v2.0": {
          "FSharp.Compiler.Service/16.0.2": {
            "type": "package",
            "compile": {
              "lib/netstandard1.6/FSharp.Compiler.Service.dll": {}
            }
          }
        }
      },
      "libraries": {
        "FSharp.Compiler.Service/16.0.2": {
          "path": "fsharp.compiler.service/16.0.2"
        }
      },
      "packageFolders": {
        "/Users/george/.nuget/packages/": {},
        "/usr/local/share/dotnet/sdk/NuGetFallbackFolder": {}
      },
      "project": {
        "version": "1.0.0",
        "frameworks": {
          "netcoreapp2.0": {
            "dependencies": {
              "FSharp.Compiler.Service": {
                "target": "Package",
                "version": "[16.0.2, )"
              }
            }
          }
        }
      }
    }"""
    let parsed = ProjectParser.parseAssetsJson json
    if not (Map.containsKey ".NETCoreApp,Version=v2.0" parsed.targets) then Fail("Failed")
    if not (Map.containsKey "FSharp.Compiler.Service/16.0.2" parsed.libraries) then Fail("Failed")
    if not (Seq.exists ((=) "/Users/george/.nuget/packages/") parsed.packageFolders) then Fail("Failed")

let ``test parsing a project file`` (t: TestContext) = 
    let file = FileInfo(Path.Combine [|projectRoot.FullName; "src"; "Main"; "Main.fsproj"|])
    let parsed = ProjectParser.parseProjectOptions file
    let referencedProjects = Array.map (fun (f, _) -> f) parsed.ReferencedProjects
    let hasName (name: string) (f: string) = f.EndsWith(name)
    if not (Seq.exists (hasName "ProjectManager.fs") parsed.SourceFiles) then Fail("Failed")
    if not (Seq.exists (hasName "LSP.dll") referencedProjects) then Fail(sprintf "No LSP.dll in %A" referencedProjects)
    if not (Seq.exists (hasName "FSharp.Compiler.Service.dll") parsed.OtherOptions) then Fail(sprintf "No FSharp.Compiler.Service.dll in %A" parsed.OtherOptions)