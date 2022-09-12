module WinCommandPalette.Logging.IO.Logging

open System.IO
open NLog
open NLog.Config
open NLog.Layouts
open NLog.Targets
open WinCommandPalette.IO

let init () =
    let config = LoggingConfiguration()

    let fileTarget =
        new FileTarget(
            "file",
            FileName =
                (Path.Combine(UserFiles.logDirectoryPath, "${shortdate}.txt")
                 |> Layout.op_Implicit)
        )

    config.AddTarget(fileTarget)

    let rule =
        LoggingRule("*", LogLevel.Info, LogLevel.Fatal, fileTarget)

    config.AddRule(rule)

    LogManager.Configuration <- config
