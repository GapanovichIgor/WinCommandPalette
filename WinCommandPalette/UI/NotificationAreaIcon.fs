module WinCommandPalette.UI.NotificationAreaIcon

open System
open System.Windows.Forms
open WinCommandPalette

let create () =
    let notifyIcon = new NotifyIcon()

    notifyIcon.Icon <- Resources.getApplicationIcon ()

    notifyIcon.ContextMenuStrip <- new ContextMenuStrip()

    let exitHandler =
        EventHandler(fun _ _ -> Application.Exit())

    notifyIcon.ContextMenuStrip.Items.Add("Exit", null, exitHandler)
    |> ignore

    notifyIcon
