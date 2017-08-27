// load dependencies from source folder to allow bootstrapping
#r "/bin/Plainion.CI/FAKE/FakeLib.dll"
#load "/bin/Plainion.CI/bits/PlainionCI.fsx"

open Fake
open PlainionCI

Target "CreatePackage" (fun _ ->
    !! ( outputPath </> "*.*Tests.*" )
    ++ ( outputPath </> "*nunit*" )
    ++ ( outputPath </> "*Moq*" )
    ++ ( outputPath </> "TestResult.xml" )
    ++ ( outputPath </> "**/*.pdb" )
    |> DeleteFiles

    !! ( outputPath </> "TestData" )
    |> DeleteDirs

    PZip.PackRelease()
)

Target "Deploy" (fun _ ->
    let releaseDir = @"\bin\RaynMaker"

    CleanDir releaseDir

    let zip = PZip.GetReleaseFile()
    Unzip releaseDir zip
)

Target "Publish" (fun _ ->
    let zip = PZip.GetReleaseFile()
    PGitHub.Release [ zip ]
)

RunTarget()
