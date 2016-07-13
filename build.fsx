#r "packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.NpmHelper
let buildDir = "./build"

Target "Clean" (fun _ -> CleanDirs [buildDir;])

Target "BuildApp" (fun _ ->
  !! "src/**/*.fsproj"
    -- "src/**/*.Tests.fsproj"
    |> MSBuildRelease buildDir "Build"
    |> Log "AppBuild-Output: "
)

Target "DbMigrate" (fun _ ->
    let npmFilePath = environVarOrDefault "NPM_FILE_PATH" defaultNpmParams.NpmFilePath
    Npm (fun p ->
              { p with
                  Command = Install Standard
                  NpmFilePath = npmFilePath
              })
    Npm (fun p ->
              { p with
                  Command = (Run "migrate")
                  NpmFilePath = npmFilePath
              })
)

"Clean"
  ==> "DbMigrate"
  ==> "BuildApp"

RunTargetOrDefault "BuildApp"