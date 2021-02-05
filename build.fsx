#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.Net
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.BuildServer

Target.initEnvironment ()

let sln = "./AaronPowell.RestreamIO.sln"

let sdkProj =
    "./AaronPowell.RestreamIO/AaronPowell.RestreamIO.fsproj"

let sdProj =
    "./AaronPowell.RestreamIO.StreamDeck/AaronPowell.RestreamIO.StreamDeck.fsproj"

let publishPath = "./.nupkg"

let tmpPath = "./.tmp"

let sdPluginUUID = "com.aaron-powell.restreamio"

let sdPluginPath =
    sprintf "%s.sdPlugin" sdPluginUUID
    |> Path.combine tmpPath

let sdPath =
    "C:\Program Files\Elgato\StreamDeck\StreamDeck.exe"

let getChangelog () =
    let changelog = "CHANGELOG.md" |> Changelog.load
    changelog.LatestEntry

let isRelease (targets: Target list) =
    targets
    |> Seq.map (fun t -> t.Name)
    |> Seq.exists ((=) "Release")

let configuration (targets: Target list) =
    let defaultVal =
        if isRelease targets then
            "Release"
        else
            "Debug"

    match Environment.environVarOrDefault "CONFIGURATION" defaultVal with
    | "Debug" -> DotNet.BuildConfiguration.Debug
    | "Release" -> DotNet.BuildConfiguration.Release
    | config -> DotNet.BuildConfiguration.Custom config

let getVersionNumber (changeLog: Changelog.ChangelogEntry) (targets: Target list) =
    match GitHubActions.Environment.CI false, isRelease targets with
    | (true, true) -> changeLog.NuGetVersion
    | (true, false) -> sprintf "%s-ci-%s" changeLog.NuGetVersion GitHubActions.Environment.RunId
    | (_, _) -> sprintf "%s-local" changeLog.NuGetVersion

Target.create
    "Clean"
    (fun _ ->
        DotNet.exec id "clean" "" |> ignore

        !!publishPath ++ tmpPath ++ sdPluginPath
        |> Shell.cleanDirs)

Target.create "Restore" (fun _ -> DotNet.restore id sln)

Target.create
    "Build"
    (fun ctx ->
        let changelog = getChangelog ()

        let args =
            [ sprintf "/p:PackageVersion=%s" (getVersionNumber changelog (ctx.Context.AllExecutingTargets))
              "--no-restore" ]

        DotNet.build
            (fun c ->
                { c with
                      Configuration = configuration (ctx.Context.AllExecutingTargets)
                      Common = c.Common |> DotNet.Options.withAdditionalArgs args })
            sln)

Target.create
    "Publish"
    (fun ctx ->
        let changelog = getChangelog ()

        let args =
            [ sprintf "/p:PackageVersion=%s" (getVersionNumber changelog (ctx.Context.AllExecutingTargets))
              "--no-restore"
              "--no-build" ]

        DotNet.publish
            (fun c ->
                { c with
                      Configuration = configuration (ctx.Context.AllExecutingTargets)
                      Common = c.Common |> DotNet.Options.withAdditionalArgs args })
            sln)

Target.create
    "PackageSdk"
    (fun ctx ->
        let changelog = getChangelog ()

        let args =
            [ sprintf "/p:PackageVersion=%s" (getVersionNumber changelog (ctx.Context.AllExecutingTargets))
              sprintf "/p:PackageReleaseNotes=\"%s\"" (sprintf "%O" changelog) ]

        DotNet.pack
            (fun c ->
                { c with
                      Configuration = configuration (ctx.Context.AllExecutingTargets)
                      OutputPath = Some publishPath
                      Common = c.Common |> DotNet.Options.withAdditionalArgs args })
            sdkProj)

Target.create
    "PackageVersion"
    (fun _ ->
        let version = getChangelog ()
        printfn "The version is %s" version.NuGetVersion)

Target.create
    "PackageStreamDeckPlugin"
    (fun ctx ->
        Directory.ensure tmpPath

        let streamDeckTool =
            Http.downloadFile
                (Path.combine tmpPath "DistributionToolWindows.zip")
                "https://developer.elgato.com/documentation/stream-deck/distributiontool/DistributionToolWindows.zip"

        printfn "Downloaded Stream Deck distribution tool to %s" streamDeckTool
        Zip.unzip tmpPath streamDeckTool

        let changelog = getChangelog ()

        let args =
            [ sprintf "/p:PackageVersion=%s" (getVersionNumber changelog (ctx.Context.AllExecutingTargets))
              "--no-restore" ]

        DotNet.build
            (fun c ->
                { c with
                      OutputPath = Some sdPluginPath
                      Configuration = configuration (ctx.Context.AllExecutingTargets)
                      Common = c.Common |> DotNet.Options.withAdditionalArgs args })
            sdProj

        let distToolPath =
            Path.combine tmpPath "DistributionTool.exe"

        CreateProcess.fromRawCommand
            distToolPath
            [ "--build"
              "--input"
              sdPluginPath
              "--output"
              publishPath ]
        |> Proc.run
        |> ignore)

Target.create
    "Changelog"
    (fun _ ->
        let changelog = getChangelog ()
        Directory.ensure publishPath

        [| sprintf "%O" changelog |]
        |> File.append "./.nupkg/changelog.md")

Target.create
    "KillStreamDeck"
    (fun _ ->
        async {
            Shell.Exec("taskkill", "/f /im streamdeck.exe")
            |> ignore

            Shell.Exec("taskkill", sprintf "/f /im %s.exe" sdPluginUUID)
            |> ignore

            do! Async.Sleep 2000
        }
        |> Async.RunSynchronously)

Target.create
    "InstallStreamDeckPlugin"
    (fun _ ->
        async {
            let path =
                [| (Environment.environVar ("APPDATA"))
                   "Elgato"
                   "StreamDeck"
                   "Plugins"
                   sprintf "%s.sdPlugin" sdPluginUUID |]
                |> String.concat Path.directorySeparator

            Shell.cleanDir path
            Shell.deleteDir path

            System.Diagnostics.ProcessStartInfo sdPath
            |> System.Diagnostics.Process.Start
            |> ignore

            do! Async.Sleep 5000

            let p =
                System.Diagnostics.ProcessStartInfo(
                    sprintf "%s%s%s.streamDeckPlugin" publishPath Path.directorySeparator sdPluginUUID
                )

            p.UseShellExecute <- true
            System.Diagnostics.Process.Start p |> ignore

            return ()
        }
        |> Async.RunSynchronously)

Target.create "Package" ignore
Target.create "Default" ignore
Target.create "Release" ignore
Target.create "CI" ignore

"Clean" ==> "Restore" ==> "Build" ==> "Default"

"Default"
==> "PackageStreamDeckPlugin"
==> "InstallStreamDeckPlugin"

"KillStreamDeck" ==> "InstallStreamDeckPlugin"

"PackageStreamDeckPlugin"
==> "PackageSdk"
==> "Package"

"Default"
==> "Publish"
// ==> "Test"
==> "Package"
==> "Changelog"
==> "Release"

"Default"
==> "Publish"
// ==> "Test"
==> "Package"
==> "Changelog"
==> "CI"

Target.runOrDefault "Default"
