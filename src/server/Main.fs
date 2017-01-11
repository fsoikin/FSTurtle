module Turtle.Web
open Suave
open Suave.Filters
open Suave.Operators

[<AutoOpen>]
module Json =
    let private jsonConverter = Fable.JsonConverter()
    let toJson x = Newtonsoft.Json.JsonConvert.SerializeObject( x, jsonConverter )

module FS =
    let home = System.AppDomain.CurrentDomain.BaseDirectory
    let resolve segments = System.IO.Path.Combine( Array.ofList <| home::segments ) |> System.IO.Path.GetFullPath

module Fsi =
    let private logger = Log.log4netLogger()
    let fsiPath = 
        match System.Configuration.ConfigurationManager.AppSettings.["FsiPath"] with
        | null -> FS.resolve [ "fsi"; "Fsi.exe" ]
        | s -> s
    let run = TurtleRunner.run (CodeRunner.runFsi logger fsiPath)

let homeDir = FS.resolve ["Content"]

let files =
    if System.Configuration.ConfigurationManager.AppSettings.["Debug"] = "true" then 
        context (fun c -> Files.browseHome)
    else
        Files.browseHome

let app =
    choose [
        GET >=> choose 
            [ path "/" >=> Files.file (homeDir + "/index.html")
              files ]

        POST >=> path "/run" >=> request (fun req -> 
            let code = System.Text.Encoding.UTF8.GetString( req.rawForm )
            let res = Fsi.run code
            Successful.OK( toJson res ))
    ]

[<EntryPoint>]
let main argv =
    printfn "HOME = %s" homeDir
    startWebServer { defaultConfig with homeFolder = Some homeDir } app
    0