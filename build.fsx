#load "packages/Aiv.Vbr.Build.Pipeline/Content/build-generic.fsx"

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

Target "Clean" (fun _ ->
  CleanDir buildDir
  CleanDir ("src" @@ "RoadRegistry.Api" @@ "wwwroot")
)

Target "Build_CoreComponents" (fun _ ->
  [
    "Shaperon"
    "RoadRegistry"
  ] |> List.iter build
)

Target "Build_LegacyDataExtraction" (fun _ -> build "RoadRegistry.LegacyStreamExtraction")
Target "Build_LegacyDataLoader" (fun _ -> build "RoadRegistry.LegacyStreamLoader")
Target "Build_Projections" (fun _ -> build "RoadRegistry.Projections")

Target "Build_Site" (fun _ ->
  [
    "RoadRegistry.Api"
    "RoadRegistry.UI"
  ] |> List.iter build
)

Target "Test_CoreComponents" (fun _ ->
  [
    "test" @@ "Shaperon.Tests"
    "test" @@ "RoadRegistry.Tests"
  ] |> List.iter testWithXunit
)

Target "Test_LegacyDataExtraction" DoNothing
Target "Test_LegacyDataLoader" DoNothing

Target "Test_Projections" (fun _ ->
  [ "test" @@ "RoadRegistry.Projections.Tests" ]
  |> List.iter testWithXunit
)

Target "Test_Site" DoNothing

Target "Publish_LegacyDataExtraction" (fun _ -> publish "RoadRegistry.LegacyStreamExtraction")
Target "Publish_LegacyDataLoader" (fun _ -> publish "RoadRegistry.LegacyStreamLoader")
Target "Publish_Projections" (fun _ -> publish "RoadRegistry.Projections")

Target "Publish_Site" (fun _ ->
  publish "RoadRegistry.Api"
  publish "RoadRegistry.UI"
)

Target "PublishAll" DoNothing

Target "PackageAll" DoNothing
Target "PushAll" DoNothing


// Publish artefacts to build folder
"DotNetCli" ==> "Clean" ==> "Restore" ==> "Build_CoreComponents" ==> "Test_CoreComponents" ==> "Build_CoreComponents"
"Build_CoreComponents" ==> "Build_LegacyDataExtraction" ==> "Test_LegacyDataExtraction" ==> "Publish_LegacyDataExtraction"
"Build_CoreComponents" ==> "Build_LegacyDataLoader" ==> "Test_LegacyDataLoader" ==> "Publish_LegacyDataLoader"
"Build_CoreComponents" ==> "Build_Projections" ==> "Test_Projections" ==> "Publish_Projections"
"Build_CoreComponents" ==> "Build_Site" ==> "Test_Site" ==> "Publish_Site"

"Publish_LegacyDataExtraction" ==> "PublishAll"
"Publish_LegacyDataLoader" ==> "PublishAll"
"Publish_Projections" ==> "PublishAll"
"Publish_Site" ==> "PublishAll"

(* DISABLED "PushAll" for now, replaced it with just excuting the "PublishAll"

Target "App_Containerize" DoNothing
Target "App_PushContainer" DoNothing
Target "PackageApp" DoNothing

// Package ends up with local Docker images
"PublishApp" ==> "App_Containerize" ==> "PackageApp"
"PackageApp" ==> "PackageAll"

// Push ends up with Docker images in AWS
"PackageApp" ==> "DockerLogin" ==> "App_PushContainer" ==> "PushAll"

*)

"PublishAll" ==> "PushAll"

RunTargetOrDefault "PackageAll"
