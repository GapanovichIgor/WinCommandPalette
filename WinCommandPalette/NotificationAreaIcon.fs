module WinCommandPalette.NotificationAreaIcon

open System
open System.Windows.Forms

let create () =
    let notifyIcon = new NotifyIcon()

    notifyIcon.Icon <- ApplicationIcon.get ()

    notifyIcon.ContextMenuStrip <- new ContextMenuStrip()

    let exitHandler =
        EventHandler(fun _ _ -> Application.Exit())

    notifyIcon.ContextMenuStrip.Items.Add("Exit", null, exitHandler)
    |> ignore

    notifyIcon