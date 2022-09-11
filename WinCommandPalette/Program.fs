open System
open System.Threading
open System.Windows.Forms
open System.Windows.Media
open WinCommandPalette.IO
open WinCommandPalette.UI

[<Literal>]
let SingleInstanceMutexName =
    "WinCommandPalette.SingleInstance"

[<EntryPoint; STAThread>]
let main _ =
    use singleInstanceMutex =
        new Mutex(false, SingleInstanceMutexName)

    if not (singleInstanceMutex.WaitOne(1)) then
        0
    else
        let commandConfig =
            use configFileStream =
                UserFiles
                    .getOrInitializeCommandConfigFileAsync()
                    .Result

            CommandConfigParser
                .parseAsync(
                    configFileStream
                )
                .Result

        let style: InputWindow.Style =
            { lightBackground = Color.FromRgb(byte 80, byte 80, byte 80)
              darkBackground = Color.FromRgb(byte 50, byte 50, byte 50)
              lightText = Color.FromRgb(byte 150, byte 150, byte 150)
              fontFamily = FontFamily("Consolas")
              autocompleteSelectedBackground = Color.FromRgb(byte 100, byte 100, byte 150)
              autocompleteSelectedText = Color.FromRgb(byte 50, byte 50, byte 50) }

        use windowInstanceManager =
            InputWindow.createWindowInstanceManager ()

        use keyboardHook =
            KeyboardHook.create (fun () ->
                let viewModel =
                    ViewModelImpl.ViewModelImpl(commandConfig)

                windowInstanceManager.ShowWindowSingleInstance(style, viewModel))

        use notifyIcon =
            NotificationAreaIcon.create ()

        notifyIcon.Visible <- true

        Application.Run()

        notifyIcon.Visible <- false
        0
