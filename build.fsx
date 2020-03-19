#load "packages/Be.Vlaanderen.Basisregisters.Build.Pipeline/Content/build-generic.fsx"

open Fake
open Fake.NpmHelper
open ``Build-generic``

let dockerRepository = "roadregistry"
let assemblyVersionNumber = (sprintf "2.%s")
let nugetVersionNumber = (sprintf "%s")

let build = buildSolution assemblyVersionNumber
let publish = publish assemblyVersionNumber
let pack = pack nugetVersionNumber
let containerize = containerize dockerRepository
let push = push dockerRepository

// Solution -----------------------------------------------------------------------

Target "Restore_Solution" (fun _ -> restore "RoadRegistry")

Target "Build_Solution" (fun _ ->
  CleanDir ("src" @@ "RoadRegistry.BackOffice.UI" @@ "wwwroot")
  CleanDir ("src" @@ "RoadRegistry.BackOffice.UI" @@ "elm-stuff")

  Npm (fun p ->
    { p with
        Command = Install Standard
        WorkingDirectory = "src" @@ "RoadRegistry.BackOffice.UI"
    }
  )

  Npm (fun p ->
    { p with
        Command = (Run "build")
        WorkingDirectory = "src" @@ "RoadRegistry.BackOffice.UI"
    }
  )

  build "RoadRegistry"
)

Target "Test_Solution" (fun _ -> test "RoadRegistry.BackOffice.Tests")

Target "Publish_Solution" (fun _ ->
  [
    "RoadRegistry.BackOffice"
    "RoadRegistry.BackOffice.Schema"
    "RoadRegistry.BackOffice.Projections"
    "RoadRegistry.BackOffice.EventHost"
    "RoadRegistry.BackOffice.CommandHost"
    "RoadRegistry.LegacyStreamExtraction"
    "RoadRegistry.LegacyStreamLoader"
    "RoadRegistry.BackOffice.Api"
    "RoadRegistry.BackOffice.UI"
  ] |> List.iter publish)

Target "Pack_Solution" DoNothing

Target "Containerize_BackOfficeApi" (fun _ -> containerize "RoadRegistry.BackOffice.Api" "backoffice-api")
Target "PushContainer_BackOfficeApi" (fun _ -> push "backoffice-api")

Target "Containerize_BackOfficeUI" (fun _ -> containerize "RoadRegistry.BackOffice.UI" "backoffice-ui")
Target "PushContainer_BackOfficeUI" (fun _ -> push "backoffice-ui")

Target "Containerize_BackOfficeProjectionHost" (fun _ -> containerize "RoadRegistry.BackOffice.ProjectionHost" "backoffice-projectionhost")
Target "PushContainer_BackOfficeProjectionHost" (fun _ -> push "backoffice-projectionhost")

Target "Containerize_BackOfficeEventHost" (fun _ -> containerize "RoadRegistry.BackOffice.EventHost" "backoffice-eventhost")
Target "PushContainer_BackOfficeEventHost" (fun _ -> push "backoffice-eventhost")

Target "Containerize_BackOfficeCommandHost" (fun _ -> containerize "RoadRegistry.BackOffice.CommandHost" "backoffice-commandhost")
Target "PushContainer_BackOfficeCommandHost" (fun _ -> push "backoffice-commandhost")

Target "Containerize_ImportLegacy" (fun _ -> containerize "RoadRegistry.LegacyStreamLoader" "import-legacy")
Target "PushContainer_ImportLegacy" (fun _ -> push "import-legacy")

Target "Containerize_ExtractLegacy" (fun _ -> containerize "RoadRegistry.LegacyStreamExtraction" "extract-legacy")
Target "PushContainer_ExtractLegacy" (fun _ -> push "extract-legacy")

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

"Pack"                                  ==> "Containerize"
"Containerize_BackOfficeApi"            ==> "Containerize"
"Containerize_BackOfficeUI"             ==> "Containerize"
"Containerize_BackOfficeProjectionHost" ==> "Containerize"
"Containerize_BackOfficeEventHost"      ==> "Containerize"
"Containerize_BackOfficeCommandHost"    ==> "Containerize"
"Containerize_ImportLegacy"             ==> "Containerize"
"Containerize_ExtractLegacy"            ==> "Containerize"
// Possibly add more projects to containerize here

"Containerize"                             ==> "Push"
"DockerLogin"                              ==> "Push"
"PushContainer_BackOfficeApi"              ==> "Push"
"PushContainer_BackOfficeUI"               ==> "Push"
"PushContainer_BackOfficeProjectionHost"   ==> "Push"
"PushContainer_BackOfficeEventHost"        ==> "Push"
"PushContainer_BackOfficeCommandHost"      ==> "Push"
"PushContainer_ImportLegacy"               ==> "Push"
"PushContainer_ExtractLegacy"              ==> "Push"
// Possibly add more projects to push here

// By default we build & test
RunTargetOrDefault "Test"
