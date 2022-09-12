module WinCommandPalette.IO.CommandExecutor

open System
open System.Diagnostics
open System.Threading.Tasks
open NLog

let private logger =
    LogManager.GetCurrentClassLogger()

let execute text =
    Task.Run<unit> (fun () ->
        task {
            use _ = logger.PushScopeProperty("CommandExecutionId", Guid.NewGuid())
            logger.Info($"Executing '{text}'")

            let processInfo =
                ProcessStartInfo("cmd.exe", "/C " + text)

            processInfo.CreateNoWindow <- true
            processInfo.RedirectStandardOutput <- true
            processInfo.RedirectStandardError <- true

            let proc = Process.Start(processInfo)
            do! proc.WaitForExitAsync()

            let! stdOut = proc.StandardOutput.ReadToEndAsync()

            if proc.ExitCode = 0 then
                logger.Info("Completed\n[ExitCode]\n{ExitCode}\n\n[StdOut]\n{StdOut}", proc.ExitCode, stdOut)
            else
                let! stdErr = proc.StandardError.ReadToEndAsync()
                logger.Error("Failed\n[ExitCode]\n{ExitCode}\n\n[StdOut]\n{StdOut}\n\n[StdErr]\n{StdErr}", proc.ExitCode, stdOut, stdErr)

            return ()
        })
    |> ignore
