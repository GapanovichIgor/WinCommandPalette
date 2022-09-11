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

type Style =
    { lightBackground: Color
      lightText: Color
      darkBackground: Color
      fontFamily: FontFamily
      autocompleteSelectedBackground: Color
      autocompleteSelectedText: Color }

type WindowInstanceManager(createWindow: Style * ViewModel -> Window) =
    let singleInstanceSyncEvent =
        new AutoResetEvent(true)

    interface IDisposable with
        member _.Dispose() = singleInstanceSyncEvent.Dispose()

    member _.ShowWindowSingleInstance(style: Style, viewModel: ViewModel) =
        if singleInstanceSyncEvent.WaitOne(1) then
            let thread =
                Thread (fun () ->
                    let window = createWindow (style, viewModel)

                    window.ShowDialog() |> ignore

                    singleInstanceSyncEvent.Set() |> ignore)

            thread.SetApartmentState(ApartmentState.STA)
            thread.Start()

let private createTextBox (style: Style, viewModel: ViewModel) =
    let textBox = TextBox()

    textBox.BorderThickness <- Thickness(0)
    textBox.Background <- SolidColorBrush(Color.FromArgb(byte 0, byte 0, byte 0, byte 0))
    textBox.Foreground <- SolidColorBrush(style.lightText)
    textBox.CaretBrush <- SolidColorBrush(style.lightText)

    textBox.FontFamily <- style.fontFamily
    textBox.FontSize <- 32
    textBox.Padding <- Thickness(0, 0, 20, 0)

    textBox.PreviewKeyDown.Add (fun e ->
        match e.Key with
        | Key.Escape ->
            e.Handled <- true
            viewModel.Escape()
        | Key.Enter ->
            e.Handled <- true
            viewModel.Enter()
        | Key.Down ->
            e.Handled <- true
            viewModel.Down()
        | Key.Up ->
            e.Handled <- true
            viewModel.Up()
        | Key.Tab ->
            e.Handled <- true
            viewModel.Tab()

        | _ -> ())

    textBox.TextChanged.Add(fun _ -> viewModel.UpdateInput textBox.Text)

    viewModel.InputTextUpdated.Add (fun () ->
        textBox.Text <- viewModel.InputText
        textBox.CaretIndex <- textBox.Text.Length)

    textBox

let private createBorder child =
    let border = Border()
    border.Background <- SolidColorBrush(Color.FromRgb(byte 50, byte 50, byte 50))
    border.Padding <- Thickness(10)
    border.Margin <- Thickness(10)

    border.CornerRadius <- CornerRadius(16)

    border.Child <- child

    border

let private createAutocompleteList (style: Style, viewModel: ViewModel) =
    let list = Grid()
    list.Margin <- Thickness(10)
    list.Background <- SolidColorBrush(style.darkBackground)
    list.Visibility <- Visibility.Collapsed

    viewModel.AutocompleteUpdated.Add (fun () ->
        list.Dispatcher.Invoke (fun () ->
            if viewModel.AutocompleteItems = [] then
                list.Visibility <- Visibility.Collapsed
            else
                list.Visibility <- Visibility.Visible

                list.Children.Clear() // TODO optimize
                list.RowDefinitions.Clear()

                for index, item in viewModel.AutocompleteItems |> Seq.indexed do
                    let textBlock = TextBlock()
                    textBlock.Text <- item
                    textBlock.FontFamily <- style.fontFamily
                    textBlock.FontSize <- 24
                    textBlock.Padding <- Thickness(5)

                    match viewModel.AutocompleteSelectionIndex with
                    | Some i when i = index ->
                        textBlock.Foreground <- SolidColorBrush(style.autocompleteSelectedText)
                        textBlock.Background <- SolidColorBrush(style.autocompleteSelectedBackground)
                    | _ -> textBlock.Foreground <- SolidColorBrush(style.lightText)

                    list.RowDefinitions.Add(RowDefinition())
                    list.Children.Add(textBlock) |> ignore
                    Grid.SetRow(textBlock, index)))

    list

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

let private createWindow (style: Style, viewModel: ViewModel) =
    let window = Window()
    window.Topmost <- true
    window.WindowStartupLocation <- WindowStartupLocation.CenterScreen
    window.ShowInTaskbar <- false
    window.WindowStyle <- WindowStyle.None
    window.ResizeMode <- ResizeMode.NoResize
    window.Background <- SolidColorBrush(style.lightBackground)
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

    viewModel.QuitRequested.Add(fun () -> closeWindow ())

    let textBox =
        createTextBox (style, viewModel)

    let border = createBorder textBox

    let autocompleteList =
        createAutocompleteList (style, viewModel)

    let grid = Grid()

    grid.RowDefinitions.Add(RowDefinition())
    grid.RowDefinitions.Add(RowDefinition())

    grid.Children.Add(border) |> ignore
    grid.Children.Add(autocompleteList) |> ignore

    Grid.SetRow(border, 0)
    Grid.SetRow(autocompleteList, 1)

    window.Content <- grid

    window

let createWindowInstanceManager () = new WindowInstanceManager(createWindow)
