module WinCommandPalette.CommandConfig

open System.IO
open System.Text.Json
open System.Text.Json.Nodes

type Command =
    { name: string
      text: string }

// let private parseNode (jsonNode : JsonNode) =


let parse (stream : Stream) : Result<Command list, string> =
    // JsonSerializer.
    let node = JsonNode.Parse(stream)

    match node with
    | :? JsonObject as object ->
        for kv in object do
            ()
        Ok []
    | _ -> Error "Top level value in the config file should be a JSON object"
