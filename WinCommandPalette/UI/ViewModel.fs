namespace WinCommandPalette.UI

type ViewModel =
    abstract InputText: string
    abstract AutocompleteItems: string list
    abstract AutocompleteSelectionIndex: int option
    abstract InputTextUpdated: IEvent<unit>
    abstract AutocompleteUpdated: IEvent<unit>
    abstract QuitRequested: IEvent<unit>
    abstract Escape: unit -> unit
    abstract Enter: unit -> unit
    abstract Tab: unit -> unit
    abstract Up: unit -> unit
    abstract Down: unit -> unit
    abstract UpdateInput: string -> unit
