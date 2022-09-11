module WinCommandPalette.UI.ViewModelImpl

open WinCommandPalette.IO
open WinCommandPalette.Logic

type private State =
    | FocusOnText
    | FocusOnAutocomplete of selectionIndex: int

type ViewModelImpl(commandConfig: CommandConfig) =
    let commands =
        commandConfig.commands
        |> Seq.map (fun c -> (c.handle, c.text))
        |> Map.ofSeq

    let mutable inputText = ""

    let mutable autocompleteItems = []

    let mutable state = FocusOnText

    let inputTextUpdatedEvent = Event<unit>()
    let autocompleteUpdatedEvent = Event<unit>()
    let quitRequested = Event<unit>()

    interface ViewModel with
        member _.InputText = inputText

        member _.AutocompleteItems = autocompleteItems

        member _.AutocompleteSelectionIndex =
            match state with
            | FocusOnAutocomplete selectionIndex -> Some selectionIndex
            | _ -> None

        member _.InputTextUpdated =
            inputTextUpdatedEvent.Publish

        member _.AutocompleteUpdated =
            autocompleteUpdatedEvent.Publish

        member _.QuitRequested = quitRequested.Publish

        member _.Escape() =
            match state with
            | FocusOnText -> quitRequested.Trigger()
            | FocusOnAutocomplete _ ->
                state <- FocusOnText
                autocompleteUpdatedEvent.Trigger()

        member _.Enter() =
            match state with
            | FocusOnText ->
                match commands.TryFind(inputText) with
                | Some commandText ->
                    CommandExecutor.execute commandText
                    quitRequested.Trigger()
                | None -> () // TODO report wrong command
            | FocusOnAutocomplete selectionIndex ->
                let autocompleteItem = autocompleteItems[selectionIndex]
                let commandText = commands[autocompleteItem]
                CommandExecutor.execute commandText
                quitRequested.Trigger()

        member _.Tab() =
            inputText <- Autocomplete.completeUntilAmbiguity commandConfig inputText
            inputTextUpdatedEvent.Trigger()

        member _.Up() =
            match state with
            | FocusOnText ->
                if autocompleteItems.Length > 0 then
                    state <- FocusOnAutocomplete(autocompleteItems.Length - 1)
                    autocompleteUpdatedEvent.Trigger()
            | FocusOnAutocomplete selectionIndex ->
                let newIndex =
                    if selectionIndex > 0 then
                        selectionIndex - 1
                    else
                        autocompleteItems.Length - 1

                state <- FocusOnAutocomplete newIndex
                autocompleteUpdatedEvent.Trigger()

        member _.Down() =
            match state with
            | FocusOnText ->
                if autocompleteItems.Length > 0 then
                    state <- FocusOnAutocomplete 0
                    autocompleteUpdatedEvent.Trigger()
            | FocusOnAutocomplete selectionIndex ->
                let newIndex =
                    if selectionIndex < autocompleteItems.Length - 1 then
                        selectionIndex + 1
                    else
                        0

                state <- FocusOnAutocomplete newIndex
                autocompleteUpdatedEvent.Trigger()

        member _.UpdateInput(newInput) =
            state <- FocusOnText
            inputText <- newInput
            autocompleteItems <- Autocomplete.getSuggestions commandConfig inputText
            autocompleteUpdatedEvent.Trigger()
