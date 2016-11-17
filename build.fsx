#r @"packages/build/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Testing.XUnit2

let root = __SOURCE_DIRECTORY__
let config = getBuildParamOrDefault "Config" "Debug"
let out = sprintf "%s/out/%s" root config
let bin = sprintf "%s/bin" out
let testBin = sprintf "%s/out/test/%s" root config

Target "Build" <| fun _ ->
  !! "src/*.fsproj"
  |> MSBuild "" "Build" ["Configuration", config]
  |> Log "Build: "

Target "BuildTests" <| fun _ ->
  !! "tests/*.fsproj" 
  |> MSBuild "" "Build" ["Configuration", config]
  |> Log "Build tests: "

Target "DeployFsi" <| fun _ ->
  Fake.FileSystemHelper.ensureDirectory bin
  Fake.FileUtils.cp_r "fsi" (sprintf "%s/fsi" bin)
  Fake.FileSystemHelper.ensureDirectory testBin
  Fake.FileUtils.cp_r "fsi" (sprintf "%s/fsi" testBin)

Target "DeployContent" <| fun _ ->
  let content = sprintf "%s/Content" out
  Fake.FileSystemHelper.ensureDirectory content
  Fake.FileSystemHelper.ensureDirectory out

  !! "content/*.css"
    -- "*.min.css"
    |> Seq.iter (fun file -> Fake.FileUtils.cp file content)

  !! "content/web.config" |> Seq.iter (fun path -> Fake.FileUtils.cp path out)

Target "Clean" <| fun _ ->
  Fake.FileUtils.rm_rf bin

Target "RunTests" <| fun _ ->
  [sprintf "%s/Turtle.Tests.dll" testBin]
  |> xUnit2 (fun p -> { p with MaxThreads = MaxThreads 1 })

let isnull (s: string) = match s with | null -> "" | s -> s

if environVar "TRAVIS_BRANCH" = "deploy" && environVar "TRAVIS_PULL_REQUEST" = "false" then
  Target "DeployToCloud" <| fun _ ->
    let kuduRepoDir = sprintf "%s\\out\\deploy" root
    let kuduRepoUrl = environVar "KUDU_REPO" |> isnull
    if kuduRepoUrl = "" then failwith "No KUDU config."
    else 
      printfn "%s" kuduRepoDir
      Fake.FileUtils.rm_rf kuduRepoDir
      Git.CommandHelper.runGitCommand (DirectoryName kuduRepoDir) (sprintf "clone %s %s" kuduRepoUrl (filename kuduRepoDir)) |> ignore
      Fake.FileUtils.cp_r out kuduRepoDir
      Fake.FileUtils.rm_rf <| sprintf "%s\\logs" kuduRepoDir
      Git.Staging.StageAll kuduRepoDir
      Git.Commit.Commit kuduRepoDir (sprintf "CI deployment #%s" <| environVar "TRAVIS_BUILD_NUMBER")
      Git.Branches.push kuduRepoDir
else
  Target "DeployToCloud" <| fun _ ->
    failwith "Can't deploy to cloud: not a Travis build of the 'deploy' branch."

"DeployFsi" ==> "Build"
"DeployContent" ==> "Build"
"Build" ==> "BuildTests" ==> "RunTests"
"Build" ==> "DeployToCloud"

RunTargetOrDefault "Build"