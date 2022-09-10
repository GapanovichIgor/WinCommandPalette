open System
open System.Drawing
open System.Runtime.InteropServices
open System.Threading
open System.Windows.Forms
open Milki.Extensions.MouseKeyHook
open WinCommandPalette

[<DllImport("user32.dll", SetLastError = true)>]
extern IntPtr SetFocus(IntPtr hWnd)

[<DllImport("user32.dll", SetLastError = true)>]
extern bool BringWindowToTop(IntPtr hWnd)

[<DllImport("user32.dll")>]
extern IntPtr GetForegroundWindow()

[<EntryPoint; STAThread>]
let main _ =
    let showFormSyncEvent =
        new AutoResetEvent(true)

    let hook =
        KeyboardHookFactory.CreateGlobal()

    let hotkeyModifiers =
        HookModifierKeys.Control

    let hotkeyKey = HookKeys.LWin

    let hotkeyHandler =
        KeyboardCallback (fun _ _ _ ->
            if showFormSyncEvent.WaitOne(1) then
                let formThread =
                    Thread (fun () ->
                        let form = new Form()
                        form.FormBorderStyle <- FormBorderStyle.None
                        form.BackColor <- Color.FromArgb(255, 80, 80, 80)

                        form.LostFocus.Add(fun _ -> form.Close())

                        form.Shown.Add (fun _ ->
                            let t0 = DateTime.Now
                            let waitWindow = TimeSpan.FromSeconds(5)

                            while form.Handle <> GetForegroundWindow()
                                  && (DateTime.Now - t0) < waitWindow do
                                form.Activate())

                        let textBox = new TextBox()

                        textBox.BorderStyle <- BorderStyle.None
                        textBox.BackColor <- Color.FromArgb(255, 50, 50, 50)
                        textBox.ForeColor <- Color.FromArgb(255, 150, 150, 150)
                        textBox.Padding <- Padding(10)
                        textBox.Dock <- DockStyle.Fill

                        // textBox.Font <- new Font("Cascadia Mono", 32f)
                        textBox.Font <- new Font("Consolas", 32f)

                        textBox.LostFocus.Add(fun _ -> form.Close())

                        form.Controls.Add(textBox)

                        Application.Run(form)
                        showFormSyncEvent.Set() |> ignore)

                formThread.SetApartmentState(ApartmentState.STA)
                formThread.Start())

    let hotkey =
        hook.RegisterHotkey(hotkeyModifiers, hotkeyKey, hotkeyHandler)

    let notifyIcon = new NotifyIcon()

    notifyIcon.Icon <- Icon.get ()

    notifyIcon.ContextMenuStrip <- new ContextMenuStrip()

    let exitHandler =
        EventHandler(fun _ _ -> Application.Exit())

    notifyIcon.ContextMenuStrip.Items.Add("Exit", null, exitHandler)
    |> ignore

    notifyIcon.Visible <- true

    Application.Run()

    notifyIcon.Visible <- false

    hook.TryUnregister(hotkey) |> ignore
    0
