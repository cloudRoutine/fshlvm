// --------------------------------------------------------------------------------------
// FAKE build script 
// --------------------------------------------------------------------------------------

#r @"packages/FAKE/tools/NuGet.Core.dll"
#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open System

// --------------------------------------------------------------------------------------
// Information about the project to be used at NuGet and in AssemblyInfo files
// --------------------------------------------------------------------------------------
let project = "FsHlvm"

let summary = "FsHlvm A High Level Virtual Machine Written In F#"

let description = "FsHlvm is a cross-platform open-source virtual machine written in F# and uses the LLVM library for high-performance code generation."

let authors = [ "KP-Tech Kft."; "Zoltan Podlovics"]

let tags = "F# FSharp fsharp"

let solutionFile  = "FsHlvm.sln"

let testAssemblies = "tests/**/bin/Release/*Tests*.dll"

let gitHome = "https://github.com/kp-tech"

let gitName = "fshlvm"

let gitRaw = environVarOrDefault "gitRaw" "https://raw.github.com/kp-tech"

//// --------------------------------------------------------------------------------------
//// The rest of the code is standard F# build script 
//// --------------------------------------------------------------------------------------

Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let release = parseReleaseNotes (IO.File.ReadAllLines "RELEASE_NOTES.md")

let genFSAssemblyInfo (projectPath) = 
    let projectName = System.IO.Path.GetFileNameWithoutExtension(projectPath) 
    let basePath = "src/" + projectName 
    let fileName = basePath + "/AssemblyInfo.fs"
    CreateFSharpAssemblyInfo fileName
      [ Attribute.Title (projectName)
        Attribute.Product project
        Attribute.Description summary
        Attribute.Version release.AssemblyVersion
        Attribute.FileVersion release.AssemblyVersion ] 

let genCSAssemblyInfo (projectPath) = 
    let projectName = System.IO.Path.GetFileNameWithoutExtension(projectPath) 
    let basePath = "src/" + projectName + "/Properties"
    let fileName = basePath + "/AssemblyInfo.cs"
    CreateCSharpAssemblyInfo fileName
      [ Attribute.Title (projectName)
        Attribute.Product project
        Attribute.Description summary
        Attribute.Version release.AssemblyVersion
        Attribute.FileVersion release.AssemblyVersion ] 

// Generate assembly info files with the right version & up-to-date information
Target "AssemblyInfo" (fun _ ->
  let fsProjs =  !! "src/**/*.fsproj"
  let csProjs = !! "src/**/*.csproj"
  fsProjs |> Seq.iter genFSAssemblyInfo
  csProjs |> Seq.iter genCSAssemblyInfo
)

// --------------------------------------------------------------------------------------
// Clean build results & restore NuGet packages

Target "RestorePackages" RestorePackages

Target "Clean" (fun _ ->
    CleanDirs ["bin"; "temp"]
)

Target "CleanDocs" (fun _ ->
    CleanDirs ["docs/output"]
)

// --------------------------------------------------------------------------------------
// Build library & test project

Target "Build" (fun _ ->
        !! solutionFile
        |> MSBuildReleaseExt "" [] "Rebuild"
        |> ignore
)

Target "Buildx86" (fun _ ->
        !! solutionFile
        |> MSBuildReleaseExt "" [("Platform","x86")] "Rebuild"
        |> ignore
)

Target "Buildx64" (fun _ ->
        !! solutionFile
        |> MSBuildReleaseExt "" [("Platform","x64")] "Rebuild"
        |> ignore
)

// --------------------------------------------------------------------------------------
// Run the unit tests using test runner

Target "RunTests" (fun _ ->
    !! testAssemblies 
    |> NUnit (fun p ->
        { p with
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 20.
            OutputFile = "TestResults.xml" })
)


// --------------------------------------------------------------------------------------
// Build a NuGet package

Target "NuGet" (fun _ ->
    NuGet (fun p -> 
        { p with   
            Authors = authors
            Project = project
            Summary = summary
            Description = description
            Version = release.NugetVersion
            ReleaseNotes = String.Join(Environment.NewLine, release.Notes)
            Tags = tags
            OutputPath = "bin"
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            Publish = hasBuildParam "nugetkey"
            Dependencies = [] })
        ("nuget/" + project + ".nuspec")
)

// --------------------------------------------------------------------------------------
// Generate the documentation

Target "GenerateDocs" (fun _ ->
    executeFSIWithArgs "docs/tools" "generate.fsx" ["--define:RELEASE"] [] |> ignore
)

// --------------------------------------------------------------------------------------
// Release Scripts

Target "ReleaseDocs" (fun _ ->
    let tempDocsDir = "temp/gh-pages"
    CleanDir tempDocsDir
    Repository.cloneSingleBranch "" (gitHome + "/" + gitName + ".git") "gh-pages" tempDocsDir

    fullclean tempDocsDir
    CopyRecursive "docs/output" tempDocsDir true |> tracefn "%A"
    StageAll tempDocsDir
    Commit tempDocsDir (sprintf "Update generated documentation for version %s" release.NugetVersion)
    Branches.push tempDocsDir
)

Target "Release" DoNothing
Target "BuildPackage" DoNothing

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "All" DoNothing

"Clean"
  ==> "RestorePackages"
  ==> "AssemblyInfo"
  ==> "Build"
  ==> "Buildx86"
  ==> "Buildx64"
  ==> "CleanDocs"
  ==> "GenerateDocs"
  ==> "All"

"All" 
  ==> "ReleaseDocs"

"All" 
  ==> "NuGet"
  ==> "BuildPackage"

"ReleaseDocs"
    ==> "Release"

"BuildPackage"
    ==> "Release"

RunTargetOrDefault "All"
