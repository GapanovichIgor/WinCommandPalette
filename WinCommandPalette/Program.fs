open System
open System.Threading.Tasks
open System.Windows.Forms
open WinCommandPalette
open WinCommandPalette.UI

[<EntryPoint; STAThread>]
let main _ =
    let commands =
        UserFiles
            .getOrInitializeCommandConfigFileAsync()
            .Result
        |> CommandConfig.parse

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
