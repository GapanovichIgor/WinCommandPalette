open System
open System.Windows.Forms
open WinCommandPalette

[<EntryPoint; STAThread>]
let main _ =
    use windowInstanceManager =
        InputWindow.createWindowInstanceManager ()

    use keyboardHook =
        KeyboardHook.create windowInstanceManager.ShowWindowSingleInstance

    use notifyIcon =
        NotificationAreaIcon.create ()

    notifyIcon.Visible <- true

    Application.Run()

    notifyIcon.Visible <- false
    0
