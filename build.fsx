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
    "xunit -configuration Release -xml TestResults.xml"

let testWithDotNet path =
  DotNetCli.Test
    (fun p ->
      { p with
          Project = path
          AdditionalArgs = ["-l trx"] })

Target "DotNetCli" (fun _ ->
  if not(DotNetCli.isInstalled()) then
    customDotnetSdkPath <- Some <| DotNetCli.InstallDotNetSDK("2.0.6")
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

Target "Api_Build" (fun _ ->
  DotNetCli.Build(fun p ->
  { p with
      Project = "src" @@ "RoadRegistry.Api.Oslo" @@ "RoadRegistry.Api.Oslo.csproj"
      Configuration = "Release"
      // Output = currentDirectory @@ buildDir @@ "RoadRegistry.Api"
  })
)

Target "Api_Test" (fun _ ->
  [ "test" @@ "RoadRegistry.Projections.Oslo.Tests"
    "test" @@ "RoadRegistry.Tests" ]
  |> List.iter testWithXunit
)

Target "Api_Publish" (fun _ ->
  DotNetCli.Publish(fun p ->
  { p with
      Project = "src" @@ "RoadRegistry.Api.Oslo" @@ "RoadRegistry.Api.Oslo.csproj"
      Configuration = "Release"
      Output = currentDirectory @@ buildDir @@ "RoadRegistry.Api.Oslo" @@ "linux"
      Runtime = "debian.8-x64"
  })

  DotNetCli.Publish(fun p ->
  { p with
      Project = "src" @@ "RoadRegistry.Api.Oslo" @@ "RoadRegistry.Api.Oslo.csproj"
      Configuration = "Release"
      Output = currentDirectory @@ buildDir @@ "RoadRegistry.Api.Oslo" @@ "win"
      Runtime = "win10-x64"
  })
)

Target "Api_Containerize" (fun _ ->
  let result1 =
    ExecProcess (fun info ->
        info.FileName <- "docker"
        info.Arguments <- sprintf "build --no-cache --tag %s/wegenregister/api-oslo:%s ." dockerRegistry buildNumber
        info.WorkingDirectory <- currentDirectory @@ buildDir @@ "RoadRegistry.Api.Oslo" @@ "linux"
    ) (System.TimeSpan.FromMinutes 5.)

  if result1 <> 0 then failwith "Failed result from API Oslo Docker Build"

  let result2 =
    ExecProcess (fun info ->
        info.FileName <- "docker"
        info.Arguments <- sprintf "tag %s/wegenregister/api-oslo:%s %s/wegenregister/api-oslo:latest" dockerRegistry buildNumber dockerRegistry
    ) (System.TimeSpan.FromMinutes 5.)

  if result2 <> 0 then failwith "Failed result from API Oslo Docker Tag"
)

Target "Api_PushContainer" (fun _ ->
  let result1 =
    ExecProcess (fun info ->
        info.FileName <- "docker"
        info.Arguments <- sprintf "push %s/wegenregister/api-oslo:%s" dockerRegistry buildNumber
    ) (System.TimeSpan.FromMinutes 5.)

  if result1 <> 0 then failwith "Failed result from API Oslo Docker Push"

  let result2 =
    ExecProcess (fun info ->
        info.FileName <- "docker"
        info.Arguments <- sprintf "push %s/wegenregister/api-oslo:latest" dockerRegistry
    ) (System.TimeSpan.FromMinutes 5.)

  if result2 <> 0 then failwith "Failed result from API Oslo Docker Push Latest"
)

Target "PublishApi" DoNothing
Target "PublishAll" DoNothing
Target "PackageApi" DoNothing
Target "PackageSite" DoNothing
Target "PackageAll" DoNothing
Target "PushAll" DoNothing

// Publish ends up with artifacts in the build folder
"DotNetCli" ==> "Clean" ==> "Restore" ==> "Api_Build" ==> "Api_Test" ==> "Api_Publish" ==> "PublishApi"
"PublishApi" ==> "PublishAll"

// Package ends up with local Docker images
"PublishApi" ==> "Api_Containerize" ==> "PackageApi"
"PackageApi" ==> "PackageAll"

// Push ends up with Docker images in AWS
"PackageApi" ==> "DockerLogin" ==> "Api_PushContainer" ==> "PushAll"

RunTargetOrDefault "PackageAll"
