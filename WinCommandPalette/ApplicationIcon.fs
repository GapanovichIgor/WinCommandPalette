module WinCommandPalette.ApplicationIcon

open System.Drawing
open System.Reflection

let get () =
    let stream =
        Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream("WinCommandPalette.icon.png")

    let bitmap = new Bitmap(stream)
    Icon.FromHandle(bitmap.GetHicon())