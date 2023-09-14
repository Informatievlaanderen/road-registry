#r "paket:
version 7.0.2
framework: net6.0
source https://api.nuget.org/v3/index.json

nuget Be.Vlaanderen.Basisregisters.Build.Pipeline 6.0.5 //"

#load "packages/Be.Vlaanderen.Basisregisters.Build.Pipeline/Content/build-generic.fsx"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO.FileSystemOperators
open Fake.IO
open Fake.JavaScript
open System.IO
open ``Build-generic``

let product = "Basisregisters Vlaanderen"
let copyright = "Copyright (c) Vlaamse overheid"
let company = "Vlaamse overheid"

let dockerRepository = "road-registry"
let assemblyVersionNumber = (sprintf "2.%s")
let nugetVersionNumber = (sprintf "%s")

let buildSolution = buildSolution assemblyVersionNumber
let buildSource = build assemblyVersionNumber
let buildTest = buildTest assemblyVersionNumber
let setVersions = (setSolutionVersions assemblyVersionNumber product copyright company)
let test = testSolution
let publishSource = publish assemblyVersionNumber
let pack = pack nugetVersionNumber
let containerize = containerize dockerRepository
let push = push dockerRepository

supportedRuntimeIdentifiers <- [ "msil"; "linux-x64" ]

// Solution -----------------------------------------------------------------------

Target.create "Restore_Solution" (fun _ -> restore "RoadRegistry")

Target.create "Publish_BackOfficeUI" (fun _ -> 
  Shell.cleanDir ("src" @@ "RoadRegistry.BackOffice.UI" @@ "dist")

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

  let dist = (buildDir @@ "RoadRegistry.BackOffice.UI" @@ "linux")
  let source = "src" @@ "RoadRegistry.BackOffice.UI"

  Shell.copyDir (dist @@ "dist") (source @@ "dist") (fun _ -> true)
  Shell.copyFile dist (source @@ "default.conf.template")
  Shell.copyFile dist (source @@ "Dockerfile")
  Shell.copyFile dist (source @@ "init.sh")
)

Target.create "Build_Solution" (fun _ ->
  Shell.cleanDir ("src" @@ "RoadRegistry.BackOffice.UI" @@ "dist")

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
  buildSolution "RoadRegistry")

Target.create "Test_Solution" (fun _ ->
  [
    "test" @@ "RoadRegistry.AdminHost.Tests"
    "test" @@ "RoadRegistry.BackOffice.Api.Tests"
    "test" @@ "RoadRegistry.BackOffice.CommandHost.Tests"
    "test" @@ "RoadRegistry.BackOffice.EventHost.Tests"
    "test" @@ "RoadRegistry.BackOffice.ExtractHost.Tests"
    "test" @@ "RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests"
    "test" @@ "RoadRegistry.BackOffice.Handlers.Sqs.Tests"
    "test" @@ "RoadRegistry.BackOffice.Handlers.Tests"
    "test" @@ "RoadRegistry.BackOffice.MessagingHost.Sqs.Tests"
    "test" @@ "RoadRegistry.BackOffice.ZipArchiveWriters.Tests"
    "test" @@ "RoadRegistry.Editor.ProjectionHost.Tests"
    "test" @@ "RoadRegistry.Legacy.Extract.Tests"
    "test" @@ "RoadRegistry.Legacy.Import.Tests"
    "test" @@ "RoadRegistry.Producer.Snapshot.ProjectionHost.Tests"
    "test" @@ "RoadRegistry.Product.ProjectionHost.Tests"
    "test" @@ "RoadRegistry.Projector.Tests"
    "test" @@ "RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests"
    "test" @@ "RoadRegistry.SyncHost.Tests"
    "test" @@ "RoadRegistry.Syndication.ProjectionHost.Tests"
    "test" @@ "RoadRegistry.Tests"
    "test" @@ "RoadRegistry.Wfs.ProjectionHost.Tests"
    "test" @@ "RoadRegistry.Wms.ProjectionHost.Tests"
  ] |> List.iter testWithDotNet
)

Target.create "Publish_Solution" (fun _ ->
  [
    "RoadRegistry.AdminHost"
    "RoadRegistry.BackOffice"
    "RoadRegistry.BackOffice.Abstractions"
    "RoadRegistry.BackOffice.Api"
    "RoadRegistry.BackOffice.CommandHost"
    "RoadRegistry.BackOffice.EventHost"
    "RoadRegistry.BackOffice.ExtractHost"
    "RoadRegistry.BackOffice.Handlers.Sqs.Lambda"
    "RoadRegistry.BackOffice.MessagingHost.Sqs"
    "RoadRegistry.BackOffice.ZipArchiveWriters"
    "RoadRegistry.Editor.ProjectionHost"
    "RoadRegistry.Legacy.Extract"
    "RoadRegistry.Legacy.Import"
    "RoadRegistry.Product.ProjectionHost"
    "RoadRegistry.Producer.Snapshot.ProjectionHost"
    "RoadRegistry.Projector"
    "RoadRegistry.Snapshot.Handlers.Sqs.Lambda"
    "RoadRegistry.SyncHost"
    "RoadRegistry.Syndication.ProjectionHost"
    "RoadRegistry.Wfs.ProjectionHost"
    "RoadRegistry.Wms.ProjectionHost"
  ] |> List.iter (fun projectName ->
    publishSource projectName

    //copy files
    let dist = (buildDir @@ projectName @@ "linux")
    let source = "src" @@ projectName

    [
      "Dockerfile"
      "init.sh"
    ] |> List.iter (fun fileName ->
      if File.Exists(source @@ fileName) then
        Shell.copyFile dist (source @@ fileName)
    )
  )

  let dist = (buildDir @@ "RoadRegistry.BackOffice.UI" @@ "linux")
  let source = "src" @@ "RoadRegistry.BackOffice.UI"

  Shell.copyDir (dist @@ "dist") (source @@ "dist") (fun _ -> true)
  Shell.copyFile dist (source @@ "default.conf.template")
  Shell.copyFile dist (source @@ "Dockerfile")
  Shell.copyFile dist (source @@ "init.sh")
)

Target.create "Pack_Solution" (fun _ ->
  [
    "RoadRegistry.BackOffice"
    "RoadRegistry.BackOffice.Api"
    "RoadRegistry.BackOffice.Abstractions"
    "RoadRegistry.BackOffice.ZipArchiveWriters"
    "RoadRegistry.Projector"
  ] |> List.map (fun projectName ->
      Shell.copyFile (buildDir @@ projectName @@ "linux") ("src" @@ projectName @@ "paket.template")
      Shell.copyFile (buildDir @@ projectName @@ "msil") ("src" @@ projectName @@ "paket.template")
      projectName
    )
    |> List.iter pack
)

type ContainerObject = { Project: string; Container: string }

// Target.create "Containerize" (fun _ ->
//   [|{ Project = "RoadRegistry.AdminHost"; Container = "backoffice-adminhost" }
//     { Project = "RoadRegistry.BackOffice.Api"; Container = "backoffice-api" }
//     { Project = "RoadRegistry.BackOffice.EventHost"; Container = "backoffice-eventhost" }
//     { Project = "RoadRegistry.BackOffice.ExtractHost"; Container = "backoffice-extracthost" }
//     { Project = "RoadRegistry.BackOffice.CommandHost"; Container = "backoffice-commandhost" }
//     { Project = "RoadRegistry.BackOffice.MessagingHost.Sqs"; Container = "backoffice-messaginghost-sqs" }
//     { Project = "RoadRegistry.BackOffice.UI"; Container = "backoffice-ui" }
//     { Project = "RoadRegistry.Projector"; Container = "projector" }
//     { Project = "RoadRegistry.Editor.ProjectionHost"; Container = "editor-projectionhost" }
//     { Project = "RoadRegistry.Product.ProjectionHost"; Container = "product-projectionhost" }
//     { Project = "RoadRegistry.Producer.Snapshot.ProjectionHost"; Container = "producer-snapshot-projectionhost" }
//     { Project = "RoadRegistry.StreetNameConsumer.ProjectionHost"; Container = "streetnameconsumer-projectionhost" }
//     { Project = "RoadRegistry.Syndication.ProjectionHost"; Container = "syndication-projectionhost" }
//     { Project = "RoadRegistry.Wfs.ProjectionHost"; Container = "wfs-projectionhost" }
//     { Project = "RoadRegistry.Wms.ProjectionHost"; Container = "wms-projectionhost" }
//     { Project = "RoadRegistry.Legacy.Import"; Container = "import-legacy" }
//     { Project = "RoadRegistry.Legacy.Extract"; Container = "extract-legacy" }
//   |] |> Array.Parallel.iter (fun o -> containerize o.Project o.Container))

Target.create "SetAssemblyVersions" (fun _ -> setVersions "SolutionInfo.cs")

Target.create "Containerize_BackOfficeApi" (fun _ -> containerize "RoadRegistry.BackOffice.Api" "backoffice-api")
Target.create "PushContainer_BackOfficeApi" (fun _ -> push "backoffice-api")

Target.create "Containerize_BackOfficeUI" (fun _ -> containerize "RoadRegistry.BackOffice.UI" "backoffice-ui")
Target.create "PushContainer_BackOfficeUI" (fun _ -> push "backoffice-ui")

Target.create "Containerize_BackOfficeEventHost" (fun _ -> containerize "RoadRegistry.BackOffice.EventHost" "backoffice-eventhost")
Target.create "PushContainer_BackOfficeEventHost" (fun _ -> push "backoffice-eventhost")

Target.create "Containerize_BackOfficeExtractHost" (fun _ -> containerize "RoadRegistry.BackOffice.ExtractHost" "backoffice-extracthost")
Target.create "PushContainer_BackOfficeExtractHost" (fun _ -> push "backoffice-extracthost")

Target.create "Containerize_BackOfficeCommandHost" (fun _ -> containerize "RoadRegistry.BackOffice.CommandHost" "backoffice-commandhost")
Target.create "PushContainer_BackOfficeCommandHost" (fun _ -> push "backoffice-commandhost")

Target.create "Containerize_BackOfficeMessagingHostSqs" (fun _ -> containerize "RoadRegistry.BackOffice.MessagingHost.Sqs" "backoffice-messaginghost-sqs")
Target.create "PushContainer_BackOfficeMessagingHostSqs" (fun _ -> push "backoffice-messaginghost-sqs")

Target.create "Containerize_AdminHost" (fun _ -> containerize "RoadRegistry.AdminHost" "adminhost")
Target.create "PushContainer_AdminHost" (fun _ -> push "adminhost")

Target.create "Containerize_Projector" (fun _ -> containerize "RoadRegistry.Projector" "projector")
Target.create "PushContainer_Projector" (fun _ -> push "projector")

Target.create "Containerize_EditorProjectionHost" (fun _ -> containerize "RoadRegistry.Editor.ProjectionHost" "editor-projectionhost")
Target.create "PushContainer_EditorProjectionHost" (fun _ -> push "editor-projectionhost")

Target.create "Containerize_ProductProjectionHost" (fun _ -> containerize "RoadRegistry.Product.ProjectionHost" "product-projectionhost")
Target.create "PushContainer_ProductProjectionHost" (fun _ -> push "product-projectionhost")

Target.create "Containerize_ProducerSnapshotProjectionHost" (fun _ -> containerize "RoadRegistry.Producer.Snapshot.ProjectionHost" "producer-snapshot-projectionhost")
Target.create "PushContainer_ProducerSnapshotProjectionHost" (fun _ -> push "producer-snapshot-projectionhost")

Target.create "Containerize_SyncHost" (fun _ -> containerize "RoadRegistry.SyncHost" "synchost")
Target.create "PushContainer_SyncHost" (fun _ -> push "synchost")

Target.create "Containerize_SyndicationProjectionHost" (fun _ -> containerize "RoadRegistry.Syndication.ProjectionHost" "syndication-projectionhost")
Target.create "PushContainer_SyndicationProjectionHost" (fun _ -> push "syndication-projectionhost")

Target.create "Containerize_WmsProjectionHost" (fun _ -> containerize "RoadRegistry.Wms.ProjectionHost" "wms-projectionhost")
Target.create "PushContainer_WmsProjectionHost" (fun _ -> push "wms-projectionhost")

Target.create "Containerize_WfsProjectionHost" (fun _ -> containerize "RoadRegistry.Wfs.ProjectionHost" "wfs-projectionhost")
Target.create "PushContainer_WfsProjectionHost" (fun _ -> push "wfs-projectionhost")

Target.create "Containerize_ImportLegacy" (fun _ -> containerize "RoadRegistry.Legacy.Import" "import-legacy")
Target.create "PushContainer_ImportLegacy" (fun _ -> push "import-legacy")

Target.create "Containerize_ExtractLegacy" (fun _ -> containerize "RoadRegistry.Legacy.Extract" "extract-legacy")
Target.create "PushContainer_ExtractLegacy" (fun _ -> push "extract-legacy")

// --------------------------------------------------------------------------------

Target.create "Build" ignore
Target.create "Test" ignore
Target.create "Publish" ignore
Target.create "Pack" ignore
Target.create "Push" ignore
Target.create "Containerize" ignore

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
  // ==> "Containerize_AdminHost"
  // ==> "Containerize_Projector"
  // ==> "Containerize_BackOfficeApi"
  // ==> "Containerize_EditorProjectionHost"
  // ==> "Containerize_ProductProjectionHost"
  // ==> "Containerize_ProducerSnapshotProjectionHost"
  // ==> "Containerize_WmsProjectionHost"
  // ==> "Containerize_WfsProjectionHost"
  // ==> "Containerize_SyndicationProjectionHost"
  // ==> "Containerize_BackOfficeEventHost"
  // ==> "Containerize_BackOfficeExtractHost"
  // ==> "Containerize_BackOfficeCommandHost"
  // ==> "Containerize_BackOfficeMessagingHostSqs"
  // ==> "Containerize_ImportLegacy"
  // ==> "Containerize_ExtractLegacy"
  // ==> "Containerize_BackOfficeUI"
  // ==> "Containerize_StreetNameConsumerProjectionHost"
  ==> "Containerize"
// Possibly add more projects to containerize here

"Containerize"
  ==> "DockerLogin"
  ==> "PushContainer_AdminHost"
  ==> "PushContainer_Projector"
  ==> "PushContainer_BackOfficeApi"
  ==> "PushContainer_BackOfficeUI"
  ==> "PushContainer_EditorProjectionHost"
  ==> "PushContainer_ProductProjectionHost"
  ==> "PushContainer_ProducerSnapshotProjectionHost"
  ==> "PushContainer_WmsProjectionHost"
  ==> "PushContainer_WfsProjectionHost"
  ==> "PushContainer_SyndicationProjectionHost"
  ==> "PushContainer_BackOfficeEventHost"
  ==> "PushContainer_BackOfficeExtractHost"
  ==> "PushContainer_BackOfficeCommandHost"
  ==> "PushContainer_BackOfficeMessagingHostSqs"
  ==> "PushContainer_ImportLegacy"
  ==> "PushContainer_ExtractLegacy"
  ==> "PushContainer_SyncHost"
  ==> "Push"
// Possibly add more projects to push here

// By default we build & test
Target.runOrDefault "Test"

// // Set assembly versions
// Target.create "SetAssemblyVersions" (fun _ -> setVersions "SolutionInfo.cs")