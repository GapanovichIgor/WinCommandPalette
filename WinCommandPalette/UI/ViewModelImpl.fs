module WinCommandPalette.UI.ViewModelImpl

open WinCommandPalette.IO
open WinCommandPalette.Logic

type private State =
    | FocusOnText
    | FocusOnAutocomplete of selectionIndex: int

type ViewModelImpl(commands: Command list) =
    let commandTextByHandle =
        commands
        |> Seq.map (fun c -> (c.handle, c.text))
        |> Map.ofSeq

    let syncObj = obj ()

    let mutable inputText = ""

    let mutable autocompleteItems = []

    let mutable state = FocusOnText

    let inputTextUpdatedEvent = Event<unit>()
    let autocompleteUpdatedEvent = Event<unit>()
    let quitRequested = Event<unit>()

    interface ViewModel with
        member _.InputText =
            lock syncObj (fun () -> inputText)

        member _.AutocompleteItems =
            lock syncObj (fun () -> autocompleteItems)

        member _.AutocompleteSelectionIndex =
            lock syncObj (fun () ->
                match state with
                | FocusOnAutocomplete selectionIndex -> Some selectionIndex
                | _ -> None)

        member _.InputTextUpdated =
            inputTextUpdatedEvent.Publish

        member _.AutocompleteUpdated =
            autocompleteUpdatedEvent.Publish

        member _.QuitRequested = quitRequested.Publish

        member _.Escape() =
            lock syncObj (fun () ->
                match state with
                | FocusOnText -> quitRequested.Trigger()
                | FocusOnAutocomplete _ ->
                    state <- FocusOnText
                    autocompleteUpdatedEvent.Trigger())

        member _.Enter(andContinue) =
            lock syncObj (fun () ->
                let inline execute commandText =
                    CommandExecutor.execute commandText

                    if andContinue then
                        inputText <- ""
                        autocompleteItems <- []
                        state <- FocusOnText
                        inputTextUpdatedEvent.Trigger()
                        autocompleteUpdatedEvent.Trigger()
                    else
                        quitRequested.Trigger()

                match state with
                | FocusOnText ->
                    match commandTextByHandle.TryFind(inputText) with
                    | Some commandText -> execute commandText
                    | None -> ()
                | FocusOnAutocomplete selectionIndex ->
                    let autocompleteItem =
                        autocompleteItems[selectionIndex]

                    let commandText =
                        commandTextByHandle[autocompleteItem]

                    execute commandText)

        member _.Tab() =
            lock syncObj (fun () ->
                inputText <- Autocomplete.completeUntilAmbiguity commands inputText
                inputTextUpdatedEvent.Trigger())

        member _.Up() =
            lock syncObj (fun () ->
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
                    autocompleteUpdatedEvent.Trigger())

        member _.Down() =
            lock syncObj (fun () ->
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
                    autocompleteUpdatedEvent.Trigger())

        member _.UpdateInput(newInput) =
            lock syncObj (fun () ->
                state <- FocusOnText
                inputText <- newInput
                autocompleteItems <- Autocomplete.getSuggestions commands inputText
                autocompleteUpdatedEvent.Trigger())
