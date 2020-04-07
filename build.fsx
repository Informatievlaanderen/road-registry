#r "paket:
version 5.241.6
framework: netstandard20
source https://api.nuget.org/v3/index.json
nuget Be.Vlaanderen.Basisregisters.Build.Pipeline 3.3.2 //"

#load "packages/Be.Vlaanderen.Basisregisters.Build.Pipeline/Content/build-generic.fsx"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO.FileSystemOperators
open Fake.IO
open Fake.JavaScript

open System
open System.IO

open ``Build-generic``

// The buildserver passes in `BITBUCKET_BUILD_NUMBER` as an integer to version the results
// and `BUILD_DOCKER_REGISTRY` to point to a Docker registry to push the resulting Docker images.

// NpmInstall
// Run an `npm install` to setup Commitizen and Semantic Release.

// DotNetCli
// Checks if the requested .NET Core SDK and runtime version defined in global.json are available.
// We are pedantic about these being the exact versions to have identical builds everywhere.

// Clean
// Make sure we have a clean build directory to start with.

// Restore
// Restore dependencies for debian.8-x64 and win10-x64 using dotnet restore and Paket.

// Build
// Builds the solution in Release mode with the .NET Core SDK and runtime specified in global.json
// It builds it platform-neutral, debian.8-x64 and win10-x64 version.

// Test
// Runs `dotnet test` against the test projects.

// Publish
// Runs a `dotnet publish` for the debian.8-x64 and win10-x64 version as a self-contained application.
// It does this using the Release configuration.

// Pack
// Packs the solution using Paket in Release mode and places the result in the dist folder.
// This is usually used to build documentation NuGet packages.

// Containerize
// Executes a `docker build` to package the application as a docker image. It does not use a Docker cache.
// The result is tagged as latest and with the current version number.

// DockerLogin
// Executes `ci-docker-login.sh`, which does an aws ecr login to login to Amazon Elastic Container Registry.
// This uses the local aws settings, make sure they are working!

// Push
// Executes `docker push` to push the built images to the registry.

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

let getDotnetExePath defaultPath : string =
  match customDotnetExePath with
  | None -> defaultPath
  | Some dotnetExePath -> dotnetExePath

let determineInstalledFxVersions () : string list =
  printfn "Determining CLR Versions using %s" (getDotnetExePath "dotnet")

  let clrVersions =
    try
      let dotnetCommand = getDotnetExePath "dotnet"

      ["--list-runtimes"]
      |> CreateProcess.fromRawCommand dotnetCommand
      |> CreateProcess.withWorkingDirectory Environment.CurrentDirectory
      |> CreateProcess.withTimeout (TimeSpan.FromMinutes 30.)
      |> CreateProcess.redirectOutput
      |> Proc.run
      |> fun output -> output.Result.Output.Split([| Environment.NewLine |], StringSplitOptions.None)
      |> Seq.filter (fun line -> line.Contains("Microsoft.NETCore.App"))
      |> Seq.map (fun line -> line.Split([| " " |], StringSplitOptions.None).[1].Trim())
      |> Seq.sortDescending
      |> Seq.toList
    with
      | _ -> []

  printfn "Determined CLR Versions: %A" clrVersions
  clrVersions

let determineInstalledSdkVersions () : string list =
  printfn "Determining SDK Versions using %s" (getDotnetExePath "dotnet")

  let sdkVersions =
    try
      let dotnetCommand = getDotnetExePath "dotnet"

      ["--list-sdks"]
      |> CreateProcess.fromRawCommand dotnetCommand
      |> CreateProcess.withWorkingDirectory Environment.CurrentDirectory
      |> CreateProcess.withTimeout (TimeSpan.FromMinutes 30.)
      |> CreateProcess.redirectOutput
      |> Proc.run
      |> fun output -> output.Result.Output.Split([| Environment.NewLine |], StringSplitOptions.None)
      |> Seq.map (fun line -> line.Split([| " " |], StringSplitOptions.None).[0].Trim())
      |> Seq.sortDescending
      |> Seq.toList
    with
      | _ -> []

  printfn "Determined SDK Versions: %A" sdkVersions
  sdkVersions

Target.create "DotNetCliCustom" (fun _ ->
  let desiredFxVersion = getDotNetClrVersionFromGlobalJson()
  let installedFxVersions = determineInstalledFxVersions()
  let desiredSdkVersion = DotNet.getSDKVersionFromGlobalJson()
  let installedSdkVersions = determineInstalledSdkVersions()

  if not (List.contains desiredSdkVersion installedSdkVersions) then
    printfn "Invalid SDK Version, Desired: %s Installed: %A" desiredSdkVersion installedSdkVersions

    let install = DotNet.install (fun p ->
      { p with
          Version = DotNet.getSDKVersionFromGlobalJson() |> DotNet.Version
      })

    let installOptions = DotNet.Options.Create() |> install

    customDotnetExePath <- Some installOptions.DotNetCliPath
    printfn "Custom SDK Path: %s" customDotnetExePath.Value
  else
    printfn "Desired SDK Version %s found" desiredSdkVersion

  if not (List.contains desiredFxVersion installedFxVersions) then
    failwithf "Invalid CLR Version, Desired: %s Installed: %A" desiredFxVersion installedFxVersions
  else
    printfn "Desired CLR Version %s found" desiredFxVersion
)

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
    "RoadRegistry.BackOffice"
    "RoadRegistry.BackOffice.Schema"
    "RoadRegistry.BackOffice.ProjectionHost"
    "RoadRegistry.BackOffice.EventHost"
    "RoadRegistry.BackOffice.CommandHost"
    "RoadRegistry.Legacy.Extract"
    "RoadRegistry.Legacy.Import"
    "RoadRegistry.BackOffice.Api"
    "RoadRegistry.BackOffice.UI"
  ] |> List.iter publish)

Target.create "Pack_Solution" ignore

Target.create "Containerize_BackOfficeApi" (fun _ -> containerize "RoadRegistry.BackOffice.Api" "backoffice-api")
Target.create "PushContainer_BackOfficeApi" (fun _ -> push "backoffice-api")

Target.create "Containerize_BackOfficeUI" (fun _ -> containerize "RoadRegistry.BackOffice.UI" "backoffice-ui")
Target.create "PushContainer_BackOfficeUI" (fun _ -> push "backoffice-ui")

Target.create "Containerize_BackOfficeProjectionHost" (fun _ -> containerize "RoadRegistry.BackOffice.ProjectionHost" "backoffice-projectionhost")
Target.create "PushContainer_BackOfficeProjectionHost" (fun _ -> push "backoffice-projectionhost")

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
  ==> "DotNetCliCustom"
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
  ==> "Containerize_BackOfficeProjectionHost"
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
  ==> "PushContainer_BackOfficeProjectionHost"
  ==> "PushContainer_BackOfficeEventHost"
  ==> "PushContainer_BackOfficeCommandHost"
  ==> "PushContainer_ImportLegacy"
  ==> "PushContainer_ExtractLegacy"
  ==> "Push"
// Possibly add more projects to push here

// By default we build & test
Target.runOrDefault "Test"
