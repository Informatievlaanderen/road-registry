#load "packages/Be.Vlaanderen.Basisregisters.Build.Pipeline/Content/build-generic.fsx"

open Fake
open Fake.NpmHelper
open ``Build-generic``

let dockerRepository = "roadregistry"
let assemblyVersionNumber = (sprintf "2.0.0.%s")
let nugetVersionNumber = (sprintf "2.0.%s")

let containerize = containerize dockerRepository
let push = push dockerRepository
let build = build assemblyVersionNumber
let publish = publish assemblyVersionNumber
let pack = pack nugetVersionNumber

Target "Build_CoreComponents" (fun _ ->
  [
    "RoadRegistry.BackOffice"
  ] |> List.iter build
)
Target "Test_CoreComponents" (fun _ ->
  [
    "test" @@ "RoadRegistry.BackOffice.Tests"
  ] |> List.iter testWithXunit
)

// Legacy stream extraction
Target "Build_LegacyDataExtraction" (fun _ -> build "RoadRegistry.LegacyStreamExtraction")
Target "Test_LegacyDataExtraction" DoNothing
Target "Publish_LegacyDataExtraction" (fun _ -> publish "RoadRegistry.LegacyStreamExtraction")

// Legacy stream loader
Target "Build_LegacyDataLoader" (fun _ -> build "RoadRegistry.LegacyStreamLoader")
Target "Test_LegacyDataLoader" DoNothing
Target "Publish_LegacyDataLoader" (fun _ -> publish "RoadRegistry.LegacyStreamLoader")
Target "Package_LegacyDataLoader" (fun _ -> containerize "RoadRegistry.LegacyStreamLoader" "legacy-stream-loader")
Target "PushContainer_LegacyDataLoader" (fun _ -> push "legacy-stream-loader")

// Projections
Target "Build_Projections" (fun _ -> build "RoadRegistry.BackOffice.Projections")
Target "Test_Projections" DoNothing
Target "Publish_Projections" (fun _ -> publish "RoadRegistry.BackOffice.Projections")
Target "Package_Projections" (fun _ -> containerize "RoadRegistry.BackOffice.Projections" "projections-legacy")
Target "PushContainer_Projections" (fun _ -> push "projections-legacy")

// Site (api + ui)
Target "Clean_Site" (fun _ -> 
  CleanDir ("src" @@ "RoadRegistry.UI" @@ "wwwroot")
)
Target "Build_Site" (fun _ ->
  let uiProjectDirectory = "src" @@ "RoadRegistry.UI"
  Npm (fun p ->
    { p with
        Command = Install Standard
        WorkingDirectory = uiProjectDirectory
    }
  )
  Npm (fun p ->
    { p with
        Command = (Run "build")
        WorkingDirectory = uiProjectDirectory
    }
  )

  [
    "RoadRegistry.BackOffice.Api"
    "RoadRegistry.UI"
  ] |> List.iter build
)
Target "Test_Site" DoNothing
Target "Publish_Site" (fun _ ->
  [
    "RoadRegistry.BackOffice.Api"
    "RoadRegistry.UI"
  ] |> List.iter publish
)
Target "Package_Site" (fun _ ->
  [
    ("RoadRegistry.BackOffice.Api", "api-legacy")
    ("RoadRegistry.UI", "ui")
  ] |> List.iter (fun (project, containerName) -> containerize project containerName)
)
Target "PushContainer_Site" (fun _ ->
  [
    "api-legacy"
    "ui"
  ] |> List.iter push
)

// Combined Targets
Target "BuildCore" DoNothing
Target "PublishAll" DoNothing
Target "PackageAll" DoNothing
Target "PushAll" DoNothing

// -- TARGET DEPENDENCIES --
// Publish artefacts to build folder
"DotNetCli" ==> "Clean" ==> "Clean_Site" ==> "Restore" ==> "Build_CoreComponents" ==> "Test_CoreComponents" ==> "BuildCore"
"BuildCore" ==> "Build_LegacyDataExtraction" ==> "Test_LegacyDataExtraction" ==> "Publish_LegacyDataExtraction"
"BuildCore" ==> "Build_LegacyDataLoader" ==> "Test_LegacyDataLoader" ==> "Publish_LegacyDataLoader"
"BuildCore" ==> "Build_Projections" ==> "Test_Projections" ==> "Publish_Projections"
"BuildCore" ==> "Build_Site" ==> "Test_Site" ==> "Publish_Site"

"Publish_LegacyDataExtraction" ==> "PublishAll"
"Publish_LegacyDataLoader" ==> "PublishAll"
"Publish_Projections" ==> "PublishAll"
"Publish_Site" ==> "PublishAll"

// Package the builds into local Docker images
"Publish_LegacyDataLoader" ==> "Package_LegacyDataLoader"
"Publish_Projections" ==> "Package_Projections"
"Publish_Site" ==> "Package_Site"

"Package_LegacyDataLoader" ==> "PackageAll"
"Package_Projections" ==> "PackageAll"
"Package_Site" ==> "PackageAll"

// Push ends up with Docker images in AWS
"Package_LegacyDataLoader" ==> "DockerLogin" ==> "PushContainer_LegacyDataLoader"
"Package_Projections" ==> "DockerLogin" ==> "PushContainer_Projections"
"Package_Site" ==> "DockerLogin" ==> "PushContainer_Site"

"PushContainer_LegacyDataLoader" ==> "PushAll"
"PushContainer_Projections" ==> "PushAll"
"PushContainer_Site" ==> "PushAll"

RunTargetOrDefault "PackageAll"
