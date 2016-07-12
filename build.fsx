#r "packages/FAKE/tools/FakeLib.dll"
open Fake

let buildDir = "./build"

Target "Clean" (fun _ -> CleanDirs [buildDir;])

Target "BuildApp" (fun _ ->
  !! "src/**/*.fsproj"
    -- "src/**/*.Tests.fsproj"
    |> MSBuildRelease buildDir "Build"
    |> Log "AppBuild-Output: "
)

"Clean"
  ==> "BuildApp"

RunTargetOrDefault "BuildApp"