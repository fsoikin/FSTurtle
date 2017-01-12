#r @"packages/build/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Testing.XUnit2

let root = __SOURCE_DIRECTORY__
let config = getBuildParamOrDefault "Config" "Debug"
let serverBin = sprintf "%s/src/server/bin/%s" root config
let testBin = sprintf "%s/tests/bin/%s" root config

Target "BuildServer" <| fun _ ->
  !! "src/server/*.fsproj"
  |> MSBuild "" "Build" ["Configuration", config]
  |> Log "Build: "
  Fake.FileUtils.cp "local.web.config" (sprintf "%s/web.config" serverBin)
  Fake.FileUtils.mkdir (sprintf "%s/logs" serverBin)

Target "BuildClient" <| fun _ ->
  NpmHelper.Npm (fun p -> { p with Command = NpmHelper.Install NpmHelper.Standard; WorkingDirectory = "src/client" })
  NpmHelper.Npm (fun p -> { p with Command = NpmHelper.Run "build"; WorkingDirectory = "src/client" })

Target "Build" DoNothing

Target "BuildTests" <| fun _ ->
  !! "tests/*.fsproj" 
  |> MSBuild "" "Build" ["Configuration", config]
  |> Log "Build tests: "

Target "DeployFsi" <| fun _ ->
  for target in [serverBin; testBin] do
    Fake.FileSystemHelper.ensureDirectory target
    Fake.FileUtils.cp_r "fsi" (sprintf "%s/fsi" target)

Target "DeployContent" <| fun _ ->
  let content = sprintf "%s/Content" serverBin
  Fake.FileSystemHelper.ensureDirectory content

  !! "src/client/*.css"
    -- "*.min.css"
    ++ "src/client/*.html"
    ++ "src/client/build/*.js"
    |> Seq.iter (Fake.FileHelper.CopyFileWithSubfolder "src/client" content)

Target "Clean" <| fun _ ->
  Fake.FileUtils.rm_rf serverBin
  Fake.FileUtils.rm_rf testBin

Target "RunTests" <| fun _ ->
  [sprintf "%s/Turtle.Tests.exe" testBin]
  |> xUnit2 (fun p -> { p with MaxThreads = MaxThreads 1 })

let isnull (s: string) = match s with | null -> "" | s -> s

if environVar "TRAVIS_BRANCH" = "deploy" && environVar "TRAVIS_PULL_REQUEST" = "false" then
  Target "DeployToCloud" <| fun _ ->
    let kuduRepoDir = sprintf "%s\\deploy" root
    let kuduRepoUrl = environVar "KUDU_REPO" |> isnull
    if kuduRepoUrl = "" then failwith "No KUDU config."
    else 
      Fake.FileUtils.rm_rf kuduRepoDir
      Git.CommandHelper.runGitCommand (DirectoryName kuduRepoDir) (sprintf "clone %s %s" kuduRepoUrl (filename kuduRepoDir)) |> ignore
      
      for d in System.IO.Directory.GetFileSystemEntries kuduRepoDir do
        if System.IO.Path.GetFileName d <> ".git" then
          Fake.FileUtils.rm_rf d

      Fake.FileUtils.cp_r serverBin kuduRepoDir
      Fake.FileUtils.cp "azure.web.config" (sprintf "%s/web.config" kuduRepoDir)
      Fake.FileUtils.rm_rf (sprintf "%s\\logs" kuduRepoDir)

      Git.Staging.StageAll kuduRepoDir
      Git.Commit.Commit kuduRepoDir (sprintf "CI deployment #%s" <| environVar "TRAVIS_BUILD_NUMBER")
      Git.Branches.push kuduRepoDir
else
  Target "DeployToCloud" <| fun _ ->
    failwith "Can't deploy to cloud: not a Travis build of the 'deploy' branch."

"BuildClient" ==> "DeployContent"
"BuildServer" ==> "Build"
"BuildClient" ==> "Build"
"DeployFsi" ==> "Build"
"DeployContent" ==> "Build"

"BuildServer" ==> "BuildTests" ==> "RunTests"
"DeployFsi" ==> "RunTests"

"Build" ==> "DeployToCloud"

RunTargetOrDefault "Build"