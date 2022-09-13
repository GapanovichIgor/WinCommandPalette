module WinCommandPalette.UI.NotificationAreaIcon

open System
open System.Diagnostics
open System.Windows.Forms
open WinCommandPalette
open WinCommandPalette.IO

let create () =
    let notifyIcon = new NotifyIcon()

    notifyIcon.Icon <- Resources.getApplicationIcon ()

    notifyIcon.ContextMenuStrip <- new ContextMenuStrip()

    let openDataFolderHandler =
        EventHandler (fun _ _ ->
            Process.Start("explorer.exe", UserFiles.userFileDirectory.FullName)
            |> ignore)

    let exitHandler =
        EventHandler(fun _ _ -> Application.Exit())

    notifyIcon.ContextMenuStrip.Items.Add("Open data folder", null, openDataFolderHandler)
    |> ignore

    notifyIcon.ContextMenuStrip.Items.Add("Exit", null, exitHandler)
    |> ignore

    notifyIcon
