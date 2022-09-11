module WinCommandPalette.IO.CommandConfigParser

open System.IO
open System.Text.Json
open System.Threading.Tasks
open WinCommandPalette.Logic

let parseAsync (stream: Stream) : ValueTask<CommandConfig> =
    JsonSerializer.DeserializeAsync<CommandConfig>(stream)
