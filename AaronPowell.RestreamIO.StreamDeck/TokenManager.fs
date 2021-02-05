namespace AaronPowell.RestreamIO.StreamDeck

open System.Threading.Tasks
open FSharp.Control.Tasks.V2
open AaronPowell.RestreamIO
open BarRaider.SdTools

type TokenManager() =
    static let mutable instance = (None: TokenManager option)
    member val ClientId = (None: string option) with get, set
    member val ClientSecret = (None: string option) with get, set
    member val AccessToken = (None: string option) with get, set
    member val RefreshToken = (None: string option) with get, set

    member this.HasAccessToken = this.AccessToken.IsSome

    static member Instance =
        match instance with
        | Some instance -> instance
        | None ->
            instance <- Some(TokenManager())
            instance.Value

    member this.SetAuth clientId clientSecret =
        this.ClientId <- Some clientId
        this.ClientSecret <- Some clientSecret

    member this.GetAccessToken code =
        match this.AccessToken with
        | Some token -> Task.FromResult(Some token)
        | None ->
            match (this.ClientId, this.ClientSecret) with
            | (Some clientId, Some clientSecret) ->
                task {
                    let! result = getToken clientId clientSecret "https://localhost:44376/" code

                    match result with
                    | Success result ->
                        this.AccessToken <- Some(result.AccessToken)
                        this.RefreshToken <- Some(result.RefreshToken)

                        return Some result.AccessToken
                    | Error error ->
                        Logger.Instance.LogMessage(
                            TracingLevel.ERROR,
                            sprintf "Failed to retrieve access token: %O" error
                        )

                        return None
                }
            | _ -> Task.FromResult None
