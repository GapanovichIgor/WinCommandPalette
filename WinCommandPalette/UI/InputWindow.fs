module WinCommandPalette.UI.InputWindow

open System
open System.Runtime.InteropServices
open System.Threading
open System.Windows
open System.Windows.Controls
open System.Windows.Input
open System.Windows.Interop
open System.Windows.Media
open WinCommandPalette.Logic

module private PInvoke =
    [<DllImport("user32.dll")>]
    extern IntPtr GetForegroundWindow()

type WindowInstanceManager(createWindow: ViewModel -> Window) =
    let singleInstanceSyncEvent =
        new AutoResetEvent(true)

    interface IDisposable with
        member _.Dispose() = singleInstanceSyncEvent.Dispose()

    member _.ShowWindowSingleInstance(viewModel: ViewModel) =
        if singleInstanceSyncEvent.WaitOne(1) then
            let thread =
                Thread (fun () ->
                    let window = createWindow viewModel

                    window.ShowDialog() |> ignore

                    singleInstanceSyncEvent.Set() |> ignore)

            thread.SetApartmentState(ApartmentState.STA)
            thread.Start()

let private createTextBox (closeWindow: unit -> unit, viewModel: ViewModel) =
    let textBox = TextBox()

    textBox.BorderThickness <- Thickness(0)
    textBox.Background <- SolidColorBrush(Color.FromArgb(byte 0, byte 0, byte 0, byte 0))
    textBox.Foreground <- SolidColorBrush(Color.FromRgb(byte 150, byte 150, byte 150))
    textBox.CaretBrush <- SolidColorBrush(Color.FromRgb(byte 150, byte 150, byte 150))

    textBox.FontFamily <- FontFamily("Consolas")
    textBox.FontSize <- 32
    textBox.Padding <- Thickness(0, 0, 20, 0)

    textBox.KeyDown.Add (fun e ->
        match e.Key with
        | Key.Escape ->
            e.Handled <- true
            closeWindow ()
        | Key.Enter ->
            e.Handled <- true
            viewModel.executeCommand textBox.Text
            closeWindow ()
        | _ -> ())

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

let private recenterWindow (window: Window) =
    let hwnd =
        WindowInteropHelper(window).Handle

    let screen =
        System.Windows.Forms.Screen.FromHandle(hwnd)

    let outsideWidth =
        float screen.Bounds.Width - window.ActualWidth

    let left = outsideWidth / 2.0
    window.Left <- left

let private createWindow (viewModel: ViewModel) =
    let window = Window()
    window.Topmost <- true
    window.WindowStartupLocation <- WindowStartupLocation.CenterScreen
    window.ShowInTaskbar <- false
    window.WindowStyle <- WindowStyle.None
    window.ResizeMode <- ResizeMode.NoResize
    window.Background <- SolidColorBrush(Color.FromRgb(byte 80, byte 80, byte 80))
    window.MinWidth <- 600
    window.SizeToContent <- SizeToContent.WidthAndHeight

    let mutable closing = false

    let closeWindow () = window.Close()

    window.Closing.Add(fun _ -> closing <- true)

    window.Loaded.Add (fun _ ->
        window.MoveFocus(TraversalRequest(FocusNavigationDirection.First))
        |> ignore)

    window.Deactivated.Add(fun _ -> if not closing then closeWindow ())

    window.Loaded.Add(fun _ -> activateWindow window)

    window.SizeChanged.Add(fun _ -> recenterWindow window)

    let textBox =
        createTextBox (closeWindow, viewModel)

    let border = createBorder textBox

    window.Content <- border

    window

let createWindowInstanceManager () = new WindowInstanceManager(createWindow)
