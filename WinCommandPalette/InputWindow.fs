module WinCommandPalette.InputWindow

open System
open System.Runtime.InteropServices
open System.Threading
open System.Windows
open System.Windows.Controls
open System.Windows.Input
open System.Windows.Interop
open System.Windows.Media

module private PInvoke =
    [<DllImport("user32.dll")>]
    extern IntPtr GetForegroundWindow()

type WindowInstanceManager(createWindow: unit -> Window) =
    let singleInstanceSyncEvent =
        new AutoResetEvent(true)

    interface IDisposable with
        member _.Dispose() = singleInstanceSyncEvent.Dispose()

    member _.ShowWindowSingleInstance() =
        if singleInstanceSyncEvent.WaitOne(1) then
            let thread =
                Thread (fun () ->
                    let window = createWindow ()

                    window.ShowDialog() |> ignore

                    singleInstanceSyncEvent.Set() |> ignore)

            thread.SetApartmentState(ApartmentState.STA)
            thread.Start()

let private createTextBox closeWindow =
    let textBox = TextBox()

    textBox.BorderThickness <- Thickness(0)
    textBox.Background <- SolidColorBrush(Color.FromArgb(byte 0, byte 0, byte 0, byte 0))
    textBox.Foreground <- SolidColorBrush(Color.FromRgb(byte 150, byte 150, byte 150))
    textBox.CaretBrush <- SolidColorBrush(Color.FromRgb(byte 150, byte 150, byte 150))

    textBox.FontFamily <- FontFamily("Consolas")
    textBox.FontSize <- 32

    textBox.KeyUp.Add (fun e ->
        if e.Key = Key.Escape then
            e.Handled <- true
            closeWindow ())

    textBox

let private createBorder child =
    let border = Border()
    border.Background <- SolidColorBrush(Color.FromRgb(byte 50, byte 50, byte 50))
    border.Padding <- Thickness(10)
    border.Margin <- Thickness(10)

    border.CornerRadius <- CornerRadius(16)

    border.Child <- child

    border

let private activateWindow window =
    let t0 = DateTime.Now
    let timeout = TimeSpan.FromSeconds(5)

    let hwnd =
        WindowInteropHelper(window).Handle

    while hwnd <> PInvoke.GetForegroundWindow()
          && (DateTime.Now - t0) < timeout do
        window.Activate() |> ignore

let private createWindow () =
    let window = Window()
    window.Topmost <- true
    window.ShowInTaskbar <- false
    window.WindowStyle <- WindowStyle.None
    window.ResizeMode <- ResizeMode.NoResize
    window.Background <- SolidColorBrush(Color.FromRgb(byte 80, byte 80, byte 80))
    window.Height <- 80
    window.Width <- 400
    window.WindowStartupLocation <- WindowStartupLocation.CenterScreen

    let mutable closing = false

    let closeWindow () = window.Close()

    window.Closing.Add(fun _ -> closing <- true)

    window.Loaded.Add (fun _ ->
        window.MoveFocus(TraversalRequest(FocusNavigationDirection.First))
        |> ignore)

    window.Deactivated.Add(fun _ -> if not closing then closeWindow ())

    window.Loaded.Add(fun _ -> activateWindow window)

    let textBox = createTextBox closeWindow

    let border = createBorder textBox

    window.Content <- border

    window

let createWindowInstanceManager () = new WindowInstanceManager(createWindow)
