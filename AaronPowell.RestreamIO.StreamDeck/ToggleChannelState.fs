namespace AaronPowell.RestreamIO.StreamDeck

open BarRaider.SdTools
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open AaronPowell.RestreamIO

type ToggleChannelStateSettings() =
    inherit RestreamActionSettings()

    [<JsonProperty(PropertyName = "selectedChannel")>]
    member val SelectedChannel = "" with get, set

    static member CreateDefaultSettings() = ToggleChannelStateSettings()

[<PluginActionId("aaronpowell.restreamio.streamdeck.togglechannelstate")>]
type ToggleChannelStateAction(connection: SDConnection, initialPayload: InitialPayload) as this =
    inherit RestreamAction<ToggleChannelStateSettings>(connection, initialPayload)

    do
        this.settings <-
            match initialPayload.Settings with
            | ps when isNull ps || ps.Count = 0 ->
                let defaultSettings =
                    ToggleChannelStateSettings.CreateDefaultSettings()

                base.SaveSettings defaultSettings
                |> Async.AwaitTask
                |> Async.RunSynchronously

                defaultSettings
            | _ -> initialPayload.Settings.ToObject<ToggleChannelStateSettings>()

    override __.KeyPressed payload =
        Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed in F#")

    override __.KeyReleased payload =
        Logger.Instance.LogMessage(TracingLevel.INFO, "Key Released F#")

    override __.OnTick() = ()

    override this.ReceivedSettings payload =
        Tools.AutoPopulateSettings(this.settings, payload.Settings)
        |> ignore

        // settings <- payload.Settings.ToObject<ToggleChannelStateSettings>()

        this.SaveSettings this.settings
        |> Async.AwaitTask
        |> Async.RunSynchronously

    override __.ReceivedGlobalSettings payload = ()

    override __.Dispose() =
        Logger.Instance.LogMessage(TracingLevel.INFO, "Deconstructor called")
