open System
open System.Threading
open System.Windows.Forms
open WinCommandPalette.IO
open WinCommandPalette.UI
open WinCommandPalette.Logic

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

        let viewModel =
            { ViewModel.executeCommand =
                fun handle ->
                    let command =
                        commandConfig.commands
                        |> Seq.tryFind (fun c -> c.handle = handle)

                    match command with // TODO report wrong command
                    | Some command -> CommandExecutor.execute command.text
                    | None -> () }

        use windowInstanceManager =
            InputWindow.createWindowInstanceManager ()

        use keyboardHook =
            KeyboardHook.create (fun () -> windowInstanceManager.ShowWindowSingleInstance viewModel)

        use notifyIcon =
            NotificationAreaIcon.create ()

        notifyIcon.Visible <- true

        Application.Run()

        notifyIcon.Visible <- false
        0
