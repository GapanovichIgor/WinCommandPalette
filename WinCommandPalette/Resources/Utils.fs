module WinCommandPalette.Utils

let forEachResult (fn: 'a -> Result<'b, 'e>) (seq: #seq<'a>) : Result<'b list, 'e> =
    let enumerator = seq.GetEnumerator()

    let result =
        System.Collections.Generic.List()

    let mutable error = None

    while error = None && enumerator.MoveNext() do
        match fn enumerator.Current with
        | Ok item -> result.Add(item)
        | Error e -> error <- Some e

    match error with
    | Some error -> Error error
    | None -> result |> List.ofSeq |> Ok
