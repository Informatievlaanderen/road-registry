#load "packages/Be.Vlaanderen.Basisregisters.Build.Pipeline/Content/build-generic.fsx"

open Fake
open Fake.NpmHelper
open ``Build-generic``

let dockerRepository = "roadregistry"
let assemblyVersionNumber = (sprintf "2.%s")
let nugetVersionNumber = (sprintf "%s")

let build = buildSolution assemblyVersionNumber
let test = testSolution
let publish = publish assemblyVersionNumber
let pack = pack nugetVersionNumber
let containerize = containerize dockerRepository
let push = push dockerRepository

// Solution -----------------------------------------------------------------------

Target "Restore_Solution" (fun _ -> restore "RoadRegistry")

Target "Build_Solution" (fun _ ->
  CleanDir ("src" @@ "RoadRegistry.UI" @@ "wwwroot")

  Npm (fun p ->
    { p with
        Command = Install Standard
        WorkingDirectory = "src" @@ "RoadRegistry.UI"
    }
  )

  Npm (fun p ->
    { p with
        Command = (Run "build")
        WorkingDirectory = "src" @@ "RoadRegistry.UI"
    }
  )

  build "RoadRegistry"
)

Target "Test_Solution" (fun _ -> test "RoadRegistry")

Target "Publish_Solution" (fun _ ->
  [
    "RoadRegistry.BackOffice"
    "RoadRegistry.BackOffice.Schema"
    "RoadRegistry.BackOffice.Projections"
    "RoadRegistry.LegacyStreamLoader"
    "RoadRegistry.LegacyStreamExtraction"
    "RoadRegistry.BackOffice.Api"
    "RoadRegistry.UI"
  ] |> List.iter publish)

Target "Pack_Solution" DoNothing

Target "Containerize_Api" (fun _ -> containerize "RoadRegistry.BackOffice.Api" "backoffice-api")
Target "PushContainer_Api" (fun _ -> push "backoffice-api")

Target "Containerize_UI" (fun _ -> containerize "RoadRegistry.UI" "backoffice-ui")
Target "PushContainer_UI" (fun _ -> push "backoffice-ui")

Target "Containerize_Projections" (fun _ -> containerize "RoadRegistry.BackOffice.Projections" "backoffice-projections")
Target "PushContainer_Projections" (fun _ -> push "backoffice-projections")

Target "Containerize_LegacyStreamLoader" (fun _ -> containerize "RoadRegistry.LegacyStreamLoader" "legacystreamloader")
Target "PushContainer_LegacyStreamLoader" (fun _ -> push "legacystreamloader")

// --------------------------------------------------------------------------------

Target "Build" DoNothing
Target "Test" DoNothing
Target "Publish" DoNothing
Target "Pack" DoNothing
Target "Containerize" DoNothing
Target "Push" DoNothing

"DotNetCli"          ==> "Build"
"Clean"              ==> "Build"
"Restore_Solution"   ==> "Build"
"Build_Solution"     ==> "Build"

"Build"              ==> "Test"
"Test_Solution"      ==> "Test"

"Test"               ==> "Publish"
"Publish_Solution"   ==> "Publish"

"Publish"            ==> "Pack"
"Pack_Solution"      ==> "Pack"

"Pack"                                    ==> "Containerize"
"Containerize_Api"                        ==> "Containerize"
"Containerize_UI"                         ==> "Containerize"
"Containerize_Projections"                ==> "Containerize"
"Containerize_LegacyStreamLoader"         ==> "Containerize"
// Possibly add more projects to containerize here

"Containerize"                            ==> "Push"
"DockerLogin"                             ==> "Push"
"PushContainer_Api"                       ==> "Push"
"PushContainer_UI"                        ==> "Push"
"PushContainer_Projections"               ==> "Push"
"PushContainer_LegacyStreamLoader"        ==> "Push"
// Possibly add more projects to push here

// By default we build & test
RunTargetOrDefault "Test"
