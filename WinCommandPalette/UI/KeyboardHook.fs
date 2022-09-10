module WinCommandPalette.UI.KeyboardHook

open System
open Milki.Extensions.MouseKeyHook

type Hook(action: unit -> unit) =
    let hookImpl =
        KeyboardHookFactory.CreateGlobal()

    let hotkeyModifiers =
        HookModifierKeys.Control

    let hotkeyKey = HookKeys.LWin

    let hotkeyHandler =
        KeyboardCallback(fun _ _ _ -> action ())

    let hotkey =
        hookImpl.RegisterHotkey(hotkeyModifiers, hotkeyKey, hotkeyHandler)

    interface IDisposable with
        member _.Dispose() =
            hookImpl.TryUnregister(hotkey) |> ignore
            hookImpl.Dispose()

let create (action: unit -> unit) = new Hook(action)
