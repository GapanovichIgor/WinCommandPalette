module WinCommandPalette.CommandExecutor

open System.Diagnostics

let execute text =
    Process.Start("cmd.exe", "/C " + text) |> ignore