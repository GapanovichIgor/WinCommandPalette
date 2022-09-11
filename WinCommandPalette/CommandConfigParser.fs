module WinCommandPalette.CommandConfigParser

open System.IO
open System.Text.Json
open System.Text.Json.Nodes
open WinCommandPalette.Utils

type Command = { path: string list; text: string }

let rec private parseNode (path: string list) (jsonNode: JsonNode) =
    match jsonNode with
    | :? JsonObject as object ->
        object
        |> forEachResult (fun kv -> parseNode (kv.Key :: path) kv.Value)
        |> Result.map (List.collect id)
    | :? JsonValue as value ->
        let element = value.GetValue<JsonElement>()

        match element.ValueKind with
        | JsonValueKind.String ->
            let value = value.GetValue<string>()

            Ok [ { path = path |> List.rev
                   text = value } ]
        | _ ->
            let path = String.concat "/" path
            Error $"{path}: Expected a string"
    | _ ->
        let path = String.concat "/" path
        Error $"{path}: Expected a JSON object or value"

let parse (stream: Stream) : Result<Command list, string> =
    let node = JsonNode.Parse(stream)

    match node with
    | :? JsonObject -> parseNode [] node
    | _ -> Error "Top level value in the config file should be a JSON object"
