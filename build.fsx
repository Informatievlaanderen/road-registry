#r "paket:
version 5.241.6
framework: netstandard20
source https://api.nuget.org/v3/index.json
nuget Be.Vlaanderen.Basisregisters.Build.Pipeline 4.1.0 //"

#load "packages/Be.Vlaanderen.Basisregisters.Build.Pipeline/Content/build-generic.fsx"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO.FileSystemOperators
open Fake.IO
open Fake.JavaScript
open ``Build-generic``

let product = "Basisregisters Vlaanderen"
let copyright = "Copyright (c) Vlaamse overheid"
let company = "Vlaamse overheid"

let dockerRepository = "road-registry"
let assemblyVersionNumber = (sprintf "2.%s")
let nugetVersionNumber = (sprintf "%s")

let build = buildSolution assemblyVersionNumber
let setVersions = (setSolutionVersions assemblyVersionNumber product copyright company)
let test = testSolution
let publish = publish assemblyVersionNumber
let pack = pack nugetVersionNumber
let containerize = containerize dockerRepository
let push = push dockerRepository

supportedRuntimeIdentifiers <- [ "linux-x64" ]

// Solution -----------------------------------------------------------------------

Target.create "Restore_Solution" (fun _ -> restore "RoadRegistry")

Target.create "Build_Solution" (fun _ ->
  Shell.cleanDir ("src" @@ "RoadRegistry.BackOffice.UI" @@ "wwwroot")
  Shell.cleanDir ("src" @@ "RoadRegistry.BackOffice.UI" @@ "elm-stuff")

  Npm.install (fun p ->
    { p with
        WorkingDirectory = "src" @@ "RoadRegistry.BackOffice.UI"
    }
  )

  Npm.run "build" (fun p ->
    { p with
        WorkingDirectory = "src" @@ "RoadRegistry.BackOffice.UI"
    }
  )

  setVersions "SolutionInfo.cs"
  build "RoadRegistry"
)

Target.create "Test_Solution" (fun _ -> test "RoadRegistry")

Target.create "Publish_Solution" (fun _ ->
  [
    "RoadRegistry.Editor.ProjectionHost"
    "RoadRegistry.Product.ProjectionHost"
    "RoadRegistry.Wms.ProjectionHost"
    "RoadRegistry.BackOffice.EventHost"
    "RoadRegistry.BackOffice.CommandHost"
    "RoadRegistry.Legacy.Extract"
    "RoadRegistry.Legacy.Import"
    "RoadRegistry.BackOffice.Api"
  ] |> List.iter publish

  let dist = (buildDir @@ "RoadRegistry.BackOffice.UI" @@ "linux")
  let source = "src" @@ "RoadRegistry.BackOffice.UI"

  Shell.copyDir (dist @@ "wwwroot") (source @@ "wwwroot") (fun _ -> true)
  Shell.copyFile dist (source @@ "Dockerfile")
  Shell.copyFile dist (source @@ "default.conf")
  Shell.copyFile dist (source @@ "config.js")
  Shell.copyFile dist (source @@ "init.sh")
)

Target.create "Pack_Solution" ignore

Target.create "Containerize_BackOfficeApi" (fun _ -> containerize "RoadRegistry.BackOffice.Api" "backoffice-api")
Target.create "PushContainer_BackOfficeApi" (fun _ -> push "backoffice-api")

Target.create "Containerize_BackOfficeUI" (fun _ -> containerize "RoadRegistry.BackOffice.UI" "backoffice-ui")
Target.create "PushContainer_BackOfficeUI" (fun _ -> push "backoffice-ui")

Target.create "Containerize_EditorProjectionHost" (fun _ -> containerize "RoadRegistry.Editor.ProjectionHost" "editor-projectionhost")
Target.create "PushContainer_EditorProjectionHost" (fun _ -> push "editor-projectionhost")

Target.create "Containerize_ProductProjectionHost" (fun _ -> containerize "RoadRegistry.Product.ProjectionHost" "product-projectionhost")
Target.create "PushContainer_ProductProjectionHost" (fun _ -> push "product-projectionhost")

Target.create "Containerize_WmsProjectionHost" (fun _ -> containerize "RoadRegistry.Wms.ProjectionHost" "wms-projectionhost")
Target.create "PushContainer_WmsProjectionHost" (fun _ -> push "wms-projectionhost")

Target.create "Containerize_BackOfficeEventHost" (fun _ -> containerize "RoadRegistry.BackOffice.EventHost" "backoffice-eventhost")
Target.create "PushContainer_BackOfficeEventHost" (fun _ -> push "backoffice-eventhost")

Target.create "Containerize_BackOfficeCommandHost" (fun _ -> containerize "RoadRegistry.BackOffice.CommandHost" "backoffice-commandhost")
Target.create "PushContainer_BackOfficeCommandHost" (fun _ -> push "backoffice-commandhost")

Target.create "Containerize_ImportLegacy" (fun _ -> containerize "RoadRegistry.Legacy.Import" "import-legacy")
Target.create "PushContainer_ImportLegacy" (fun _ -> push "import-legacy")

Target.create "Containerize_ExtractLegacy" (fun _ -> containerize "RoadRegistry.Legacy.Extract" "extract-legacy")
Target.create "PushContainer_ExtractLegacy" (fun _ -> push "extract-legacy")

// --------------------------------------------------------------------------------

Target.create "Build" ignore
Target.create "Test" ignore
Target.create "Publish" ignore
Target.create "Pack" ignore
Target.create "Containerize" ignore
Target.create "Push" ignore

"NpmInstall"
  ==> "DotNetCli"
  ==> "Clean"
  ==> "Restore_Solution"
  ==> "Build_Solution"
  ==> "Build"

"Build"
  ==> "Test_Solution"
  ==> "Test"

"Test"
  ==> "Publish_Solution"
  ==> "Publish"

"Publish"
  ==> "Pack_Solution"
  ==> "Pack"

"Pack"
  ==> "Containerize_BackOfficeApi"
  ==> "Containerize_BackOfficeUI"
  ==> "Containerize_EditorProjectionHost"
  ==> "Containerize_ProductProjectionHost"
  ==> "Containerize_WmsProjectionHost"
  ==> "Containerize_BackOfficeEventHost"
  ==> "Containerize_BackOfficeCommandHost"
  ==> "Containerize_ImportLegacy"
  ==> "Containerize_ExtractLegacy"
  ==> "Containerize"
// Possibly add more projects to containerize here

"Containerize"
  ==> "DockerLogin"
  ==> "PushContainer_BackOfficeApi"
  ==> "PushContainer_BackOfficeUI"
  ==> "PushContainer_EditorProjectionHost"
  ==> "PushContainer_ProductProjectionHost"
  ==> "PushContainer_WmsProjectionHost"
  ==> "PushContainer_BackOfficeEventHost"
  ==> "PushContainer_BackOfficeCommandHost"
  ==> "PushContainer_ImportLegacy"
  ==> "PushContainer_ExtractLegacy"
  ==> "Push"
// Possibly add more projects to push here

// By default we build & test
Target.runOrDefault "Test"
