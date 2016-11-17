module Turtle.Log

type Level = Debug | Info | Error
type Logger = Level -> string -> unit

let log4netLogger () : Logger =
  log4net.Config.XmlConfigurator.Configure() |> ignore
  let logger = log4net.LogManager.GetLogger "App"
  fun level msg ->
    let msg = sprintf "[th%d] %s" System.Threading.Thread.CurrentThread.ManagedThreadId msg
    match level with
    | Debug -> logger.Debug msg
    | Info -> logger.Info msg
    | Error -> logger.Error msg