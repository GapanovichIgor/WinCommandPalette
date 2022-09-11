module WinCommandPalette.IO.CommandExecutor

open System.Diagnostics

let execute text =
    let processInfo =
        ProcessStartInfo("cmd.exe", "/C " + text)

    processInfo.CreateNoWindow <- true

    Process.Start(processInfo) |> ignore
