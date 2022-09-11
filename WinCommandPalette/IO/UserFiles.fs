module WinCommandPalette.IO.UserFiles

open System
open System.IO
open System.Threading.Tasks
open WinCommandPalette

[<Literal>]
let private userFileFolderName =
    "WinCommandPalette"

[<Literal>]
let private commandConfigFileName =
    "CommandConfig.json"

let private userFileDirectory =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), userFileFolderName)
    |> Directory.CreateDirectory

let private commandConfigFilePath =
    Path.Combine(userFileDirectory.FullName, commandConfigFileName)

let getOrInitializeCommandConfigFileAsync () : Task<Stream> =
    task {
        if not (File.Exists commandConfigFilePath) then
            use configStream =
                File.Create commandConfigFilePath

            let defaultConfigStream =
                Resources.getDefaultCommandConfig ()

            do! defaultConfigStream.CopyToAsync(configStream)
            do! configStream.FlushAsync()

        return File.OpenRead commandConfigFilePath
    }
