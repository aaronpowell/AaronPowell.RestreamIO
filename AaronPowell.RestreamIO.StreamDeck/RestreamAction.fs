namespace AaronPowell.RestreamIO.StreamDeck

open BarRaider.SdTools
open Newtonsoft.Json.Linq

module private EventHandlers =
    open BarRaider.SdTools.Wrappers
    open BarRaider.SdTools.Events

    let handleSendToPlugin (connection: SDConnection) =
        (fun (evt: SDEventReceivedEventArgs<SendToPlugin>) ->
            let payload = evt.Event.Payload

            let operation =
                payload.["property_inspector"].Value<string>()

            match operation with
            | "updateAPI" ->
                let clientId = payload.["clientId"].Value<string>()
                let secretId = payload.["secretId"].Value<string>()

                TokenManager.Instance.SetAuth clientId secretId

            | "updateApprovalCode" ->
                let code = payload.["approvalCode"].Value<string>()

                TokenManager.Instance.GetAccessToken code
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> ignore

            | "getClientId" ->
                let clientId =
                    TokenManager.Instance.ClientId
                    |> Option.defaultValue ""

                connection.SendToPropertyInspectorAsync(JObject.FromObject({| clientId = clientId |}))
                |> Async.AwaitTask
                |> Async.RunSynchronously

            | _ ->
                Logger.Instance.LogMessage(TracingLevel.WARN, sprintf "received unhandled event from PI %s" operation))

open AaronPowell.RestreamIO

[<AbstractClass>]
type RestreamAction<'T when 'T :> RestreamActionSettings>(connection: SDConnection, initialPayload: InitialPayload) as this =
    inherit PluginBase(connection, initialPayload)

    [<DefaultValue>]
    val mutable settings: 'T

    do
        connection.OnSendToPlugin.Add
            (fun evt ->
                EventHandlers.handleSendToPlugin connection evt
                this.RefreshRestreamData())

    member __.SaveSettings(settings: 'T) =
        settings
        |> JObject.FromObject
        |> connection.SetSettingsAsync

    abstract member RefreshRestreamData: unit -> unit

    default __.RefreshRestreamData() =
        match TokenManager.Instance.AccessToken with
        | Some token ->
            let response =
                getChannels token
                |> Async.AwaitTask
                |> Async.RunSynchronously

            match response with
            | Success channels ->
                this.settings.Channels <-
                    channels
                    |> Seq.map
                        (fun channel ->
                            { DisplayName = channel.DisplayName
                              Id = channel.Id })
                    |> Seq.toArray

                this.SaveSettings this.settings
                |> Async.AwaitTask
                |> Async.RunSynchronously
            | Error error ->
                Logger.Instance.LogMessage(TracingLevel.ERROR, sprintf "Failed to get channels %s" error.Error.Message)
        | None -> ()
