module WinCommandPalette.IO.CommandConfigParser

open System.IO
open System.Text.Json
open System.Threading.Tasks
open WinCommandPalette.Logic

type CommandRaw =
    { handle: string
      text: string
      showToast: bool option }

let parseAsync (stream: Stream) : Task<Command list> =
    task {
        let! rawCommandList = JsonSerializer.DeserializeAsync<CommandRaw list>(stream)

        return
            rawCommandList
            |> List.map (fun c ->
                { Command.handle = c.handle
                  text = c.text
                  showToast = c.showToast |> Option.defaultValue true })
    }
