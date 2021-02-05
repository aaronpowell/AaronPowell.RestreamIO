namespace AaronPowell.RestreamIO.StreamDeck

open AaronPowell.RestreamIO
open Newtonsoft.Json

type RestreamChannel = { DisplayName: string; Id: int }

type RestreamActionSettings() =
    [<JsonProperty(PropertyName = "token")>]
    member val Token = "" with get, set

    [<JsonProperty(PropertyName = "channels")>]
    member val Channels = (Array.empty: RestreamChannel array) with get, set

    [<JsonProperty(PropertyName = "platforms")>]
    member val Platforms = (Seq.empty: Platform seq) with get, set
