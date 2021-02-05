open BarRaider.SdTools

[<EntryPoint>]
let main argv =
    // uncomment to debug
    // while (not System.Diagnostics.Debugger.IsAttached) do
    //     System.Threading.Thread.Sleep 1000

    SDWrapper.Run argv
    0
