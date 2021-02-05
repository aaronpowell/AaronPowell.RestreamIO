namespace AaronPowell.RestreamIO.StreamDeck

open BarRaider.SdTools
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type internal PluginSettings() =
    [<FilenameProperty>]
    [<JsonProperty(PropertyName = "outputFileName")>]
    member val OutputFileName = "" with get, set

    [<JsonProperty(PropertyName = "inputString")>]
    member val InputString = "" with get, set

    static member CreateDefaultSettings() =
        let instance = PluginSettings()
        instance

[<PluginActionId("aaronpowell.restreamio.streamdeck.pluginaction")>]
type PluginAction(connection: SDConnection, initialPayload: InitialPayload) =
    inherit PluginBase(connection, initialPayload)

    let saveSettings (settings: PluginSettings) =
        settings
        |> JObject.FromObject
        |> connection.SetSettingsAsync

    let mutable settings =
        match initialPayload.Settings with
        | ps when isNull ps || ps.Count = 0 -> // ->
            let ds = PluginSettings.CreateDefaultSettings()

            saveSettings ds
            |> Async.AwaitTask
            |> Async.RunSynchronously

            ds
        | ps -> ps.ToObject<PluginSettings>()
        | _ ->
            let ds = PluginSettings.CreateDefaultSettings()

            saveSettings ds
            |> Async.AwaitTask
            |> Async.RunSynchronously

            ds

    override __.KeyPressed payload =
        Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed in F#")

    override __.KeyReleased payload =
        Logger.Instance.LogMessage(TracingLevel.INFO, "Key Released F#")

    override __.OnTick() = ()

    override __.ReceivedSettings payload =
        Tools.AutoPopulateSettings(settings, payload.Settings)
        |> ignore

        saveSettings settings
        |> Async.AwaitTask
        |> Async.RunSynchronously

    override __.ReceivedGlobalSettings payload = ()

    override __.Dispose() =
        Logger.Instance.LogMessage(TracingLevel.INFO, "Deconstructor called")
