open System
open System.Windows.Forms
open WinCommandPalette
open WinCommandPalette.UI

[<EntryPoint; STAThread>]
let main _ =
    let commands =
        use configFileStream =
            UserFiles
                .getOrInitializeCommandConfigFileAsync()
                .Result

        CommandConfigParser.parse configFileStream

    let commands =
        match commands with
        | Ok commands -> commands
        | Error e -> failwith e // TODO report config error

    let pathToString (path: string list) = path |> String.concat "/"

    let viewModel =
        { ViewModel.executeCommand =
            fun text ->
                let command =
                    commands
                    |> Seq.tryFind (fun c -> pathToString c.path = text)

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
