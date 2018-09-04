// include Fake lib
#r "packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.FileSystemHelper
open Fake.NpmHelper

// Properties
let buildNumber = environVarOrDefault "BUILD_BUILDNUMBER" "develop"
let buildDir = "./dist/"
let dockerRegistry = "921707234258.dkr.ecr.eu-west-1.amazonaws.com"

let mutable customDotnetSdkPath : Option<string> = None

let testWithXunit path =
  DotNetCli.RunCommand
    (fun p ->
       { p with
          ToolPath =
            match customDotnetSdkPath with
            | None -> p.ToolPath
            | Some sdkPath -> sdkPath
          WorkingDir = path })
    "xunit -configuration Release -xml test-results/TestResults.xml"

let testWithDotNet path =
  DotNetCli.Test
    (fun p ->
      { p with
          Project = path
          AdditionalArgs = ["-l trx"] })

Target "DotNetCli" (fun _ ->
  if not(DotNetCli.isInstalled()) then
    customDotnetSdkPath <- Some <| DotNetCli.InstallDotNetSDK("2.1.1")
)

Target "DockerLogin" (fun _ ->
  let dockerLogin =
    ExecProcess (fun info ->
        info.FileName <- "bash"
        info.Arguments <- "docker_login.sh"
    ) (System.TimeSpan.FromMinutes 5.)

  if dockerLogin <> 0 then failwith "Failed result from Docker Login"
)

Target "Clean" (fun _ ->
  CleanDir buildDir
)

Target "Restore" (fun _ ->
  DotNetCli.Restore(id)
)

Target "Build" (fun _ ->
  DotNetCli.Build(fun p ->
  { p with
      Project = "src" @@ "RoadRegistry.Api" @@ "RoadRegistry.Api.csproj"
      Configuration = "Release"
  })

  DotNetCli.Build(fun p ->
  { p with
      Project = "src" @@ "RoadRegistry.UI" @@ "RoadRegistry.UI.csproj"
      Configuration = "Release"
  })

  DotNetCli.Build(fun p ->
  { p with
      Project = "src" @@ "RoadRegistry.LegacyStreamExtraction" @@ "RoadRegistry.LegacyStreamExtraction.csproj"
      Configuration = "Release"
  })

  DotNetCli.Build(fun p ->
  { p with
      Project = "src" @@ "RoadRegistry.LegacyStreamLoader" @@ "RoadRegistry.LegacyStreamLoader.csproj"
      Configuration = "Release"
  })
)

Target "Test" (fun _ ->
  [ "test" @@ "RoadRegistry.Projections.Tests"
    "test" @@ "RoadRegistry.Tests"
    "test" @@ "Shaperon.Tests" ]
  |> List.iter testWithXunit
)

Target "App_Publish" (fun _ ->
  DotNetCli.Publish(fun p ->
  { p with
      Project = "src" @@ "RoadRegistry.Api" @@ "RoadRegistry.Api.csproj"
      Configuration = "Release"
      Output = currentDirectory @@ buildDir @@ "RoadRegistry.Api" @@ "linux"
      Runtime = "debian.8-x64"
  })

  DotNetCli.Publish(fun p ->
  { p with
      Project = "src" @@ "RoadRegistry.UI" @@ "RoadRegistry.UI.csproj"
      Configuration = "Release"
      Output = currentDirectory @@ buildDir @@ "RoadRegistry.UI" @@ "linux"
      Runtime = "debian.8-x64"
  })

  DotNetCli.Publish(fun p ->
  { p with
      Project = "src" @@ "RoadRegistry.Api" @@ "RoadRegistry.Api.csproj"
      Configuration = "Release"
      Output = currentDirectory @@ buildDir @@ "RoadRegistry.Api" @@ "win"
      Runtime = "win10-x64"
  })

  DotNetCli.Publish(fun p ->
  { p with
      Project = "src" @@ "RoadRegistry.UI" @@ "RoadRegistry.UI.csproj"
      Configuration = "Release"
      Output = currentDirectory @@ buildDir @@ "RoadRegistry.UI" @@ "win"
      Runtime = "win10-x64"
  })
)

Target "App_Containerize" DoNothing
Target "App_PushContainer" DoNothing
Target "PublishApp" DoNothing
Target "PublishAll" DoNothing
Target "PackageApp" DoNothing
Target "PackageSite" DoNothing
Target "PackageAll" DoNothing
Target "PushAll" DoNothing

// Publish ends up with artifacts in the build folder
"DotNetCli" ==> "Clean" ==> "Restore" ==> "Build" ==> "Test" ==> "App_Publish" ==> "PublishApp"
"PublishApp" ==> "PublishAll"

// Package ends up with local Docker images
"PublishApp" ==> "App_Containerize" ==> "PackageApp"
"PackageApp" ==> "PackageAll"

// Push ends up with Docker images in AWS
"PackageApp" ==> "DockerLogin" ==> "App_PushContainer" ==> "PushAll"

RunTargetOrDefault "PackageAll"
