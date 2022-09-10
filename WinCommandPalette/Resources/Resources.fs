module WinCommandPalette.Resources

open System.Drawing
open System.Reflection

let private getResource name =
    Assembly
        .GetExecutingAssembly()
        .GetManifestResourceStream($"WinCommandPalette.Resources.{name}")

let getApplicationIcon () =
    let stream = getResource "ApplicationIcon.png"

    let bitmap = new Bitmap(stream)
    Icon.FromHandle(bitmap.GetHicon())

let getDefaultCommandConfig () = getResource "DefaultCommandConfig.json"
