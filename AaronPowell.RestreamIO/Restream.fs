namespace AaronPowell.RestreamIO

open FSharp.Control.Tasks.V2
open Newtonsoft.Json
open FSharp.Data.HttpRequestHeaders

[<AutoOpen>]
module Api =
    open FSharp.Data

    let private getInternal url token =
        let headers =
            match token with
            | Some token -> [ Authorization(sprintf "Bearer %s" token) ]
            | None -> []

        Http.AsyncRequest(sprintf "https://api.restream.io/v2%s" url, headers = headers)

    let private patchInternal url token body =
        let headers =
            match token with
            | Some token ->
                [ Authorization(sprintf "Bearer %s" token)
                  ContentType HttpContentTypes.Json ]
            | None -> [ ContentType HttpContentTypes.Json ]

        Http.AsyncRequest(
            sprintf "https://api.restream.io/v2%s" url,
            headers = headers,
            httpMethod = HttpMethod.Patch,
            body = body
        )

    let inline parser<'t> json = JsonConvert.DeserializeObject<'t> json

    let inline parseSuccess<'t> json = Success(parser<'t> json)
    let inline parseError json = Error(parser<ResponseError> json)

    let private parseResponse<'t> (http: Async<HttpResponse>) =
        task {
            let! res = http

            return
                match res.Body with
                | Text json ->
                    match res.StatusCode with
                    | 200 -> parseSuccess<'t> json
                    | _ -> parseError json
                | _ -> failwith "Unexpected response type from request"
        }

    let getToken clientId clientSecret redirectUri code =
        Http.AsyncRequest(
            "https://api.restream.io/oauth/token",
            httpMethod = HttpMethod.Post,
            body =
                FormValues [ "grant_type", "authorization_code"
                             "redirect_uri", redirectUri
                             "code", code
                             "client_id", clientId
                             "client_secret", clientSecret ]
        )
        |> parseResponse<TokenSuccess>

    let refreshToken clientId clientSecret refreshToken =
        Http.AsyncRequest(
            "https://api.restream.io/oauth/token",
            httpMethod = HttpMethod.Post,
            body =
                FormValues [ "grant_type", "refresh_token"
                             "refresh_token", refreshToken
                             "client_id", clientId
                             "client_secret", clientSecret ]
        )
        |> parseResponse<TokenSuccess>

    let revokeToken token tokenType =
        task {
            let tokenTypeHint =
                match tokenType with
                | AccessToken -> "access_token"
                | RefreshToken -> "refresh_token"

            let! _ =
                Http.AsyncRequest(
                    "https://api.restream.io/oauth/revoke",
                    body =
                        FormValues [ "token", token
                                     "token_type_hint", tokenTypeHint ]
                )

            return ()
        }

    let getPlatforms () =
        getInternal "/platform/all" None
        |> parseResponse<Platforms seq>

    let getProfile accessToken =
        getInternal "/user/profile" (Some(accessToken))
        |> parseResponse<Profile>

    let getIngest accessToken =
        getInternal "/user/ingest" (Some(accessToken))
        |> parseResponse<Ingest>

    let getStreamKey accessToken =
        getInternal "/user/streamKey" (Some(accessToken))
        |> parseResponse<StreamKey>

    let getChannels accessToken =
        getInternal "/user/channel/all" (Some(accessToken))
        |> parseResponse<Channel seq>

    let getChannel accessToken id =
        getInternal (sprintf "/user/channel/%d" id) (Some(accessToken))
        |> parseResponse<Channel>

    let updateChannel accessToken id (active: bool) =
        let payload = {| active = active |}

        patchInternal
            (sprintf "/user/channel/%d" id)
            (Some(accessToken))
            (TextRequest(JsonConvert.SerializeObject payload))
        |> parseResponse<unit>

    let getChannelMeta accessToken id =
        getInternal (sprintf "/user/channel-meta/%d" id) (Some(accessToken))
        |> parseResponse<ChannelMeta>

    let updateChannelMeta accessToken id title =
        let payload = { Title = title }

        patchInternal
            (sprintf "/user/channel-meta/%d" id)
            (Some(accessToken))
            (TextRequest(JsonConvert.SerializeObject payload))
        |> parseResponse<ChannelMeta>

    let getChatUrl accessToken =
        getInternal "/user/webchat/url" (Some(accessToken))
        |> parseResponse<ChatUrl>

    let getUpcomingEvents accessToken =
        getInternal "/user/events/upcoming" (Some(accessToken))
        |> parseResponse<Event seq>

    let getInProgressEvents accessToken =
        getInternal "/user/events/in-progress" (Some(accessToken))
        |> parseResponse<Event seq>

    let getEvent accessToken id =
        getInternal (sprintf "/user/events/%s" id) (Some(accessToken))
        |> parseResponse<Event>

    let getEventStreamKey accessToken id =
        getInternal (sprintf "/user/events/%s/streamKey" id) (Some(accessToken))
        |> parseResponse<StreamKey>
