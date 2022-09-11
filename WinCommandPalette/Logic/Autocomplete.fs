module WinCommandPalette.Logic.Autocomplete

let getSuggestions (commands: Command list) (inputText: string) : string list =
    if inputText.Length = 0 then
        []
    else
        commands
        |> Seq.choose (fun c ->
            if c.handle.StartsWith(inputText) then
                Some(c.handle, c.handle.Length - inputText.Length)
            else
                None)
        |> Seq.sortBy snd
        |> Seq.map fst
        |> List.ofSeq

let completeUntilAmbiguity (commands: Command list) (inputText: string) : string =
    let handles =
        commands
        |> Seq.choose (fun c ->
            if c.handle.StartsWith(inputText) then
                Some c.handle
            else
                None)
        |> List.ofSeq

    let minLength =
        handles |> Seq.map String.length |> Seq.min

    let rec findCommonSubstrEnd completionLength =
        if inputText.Length + completionLength = minLength then
            completionLength
        else
            let charMatches =
                handles
                |> Seq.map (fun h -> h[inputText.Length + completionLength])
                |> Seq.distinct
                |> Seq.length
                |> ((=) 1)

            if charMatches then
                findCommonSubstrEnd (completionLength + 1)
            else
                completionLength

    let completionLength = findCommonSubstrEnd 0

    if completionLength = 0 then
        inputText
    else
        handles[0]
            .Substring(0, inputText.Length + completionLength)
