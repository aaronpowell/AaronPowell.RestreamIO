namespace AaronPowell.RestreamIO

open System

[<AutoOpen>]
module Types =
    open Newtonsoft.Json

    type TokenSuccess =
        { AccessToken: string
          Expires: int
          [<JsonProperty("expires_in")>]
          ExpiresIn: int
          RefreshToken: string
          Scopes: string []
          TokenType: string
          AccessTokenExpiresIn: int
          AccessTokeExpiresAt: DateTime
          AccessTokenExpiresEpoch: int
          RefreshTokenExpiresIn: int
          RefreshTokenExpiresAt: DateTime
          RefreshTokenExpiresEpoc: int }

    type ResponseError =
        { Error: {| StatusCode: int
                    Status: int
                    Code: int
                    Message: string
                    Name: string |} }

    type ApiResponse<'valid> =
        | Success of 'valid
        | Error of ResponseError

    type TokenType =
        | AccessToken
        | RefreshToken

    type Platform =
        { Id: int
          Name: string
          Url: string
          Image: {| Png: string; Svg: string |} }

    type Profile =
        { Id: int
          Username: string
          Email: string }

    type Ingest = { Id: int }

    type StreamKey = { StreamKey: string }

    type Channel =
        { Id: int
          StreamingPlatformId: int
          EmbedUrl: string
          Url: string
          Identifier: string
          DisplayName: string
          Active: bool }

    type ChannelMeta = { Title: string }

    type ChatUrl = { WebchatUrl: string }

    type Event =
        { Id: string
          Status: string
          Title: string
          Description: string
          CoverUrl: string option
          ScheduledFor: int
          Destinations: {| ChannelId: string
                           ExternalUrl: string option
                           StreamingPlatformId: int |} }
