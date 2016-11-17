module Turtle.Main
open System.Configuration
open WebSharper
open WebSharper.Sitelets
open WebSharper.UI.Next
open WebSharper.UI.Next.Html
open WebSharper.UI.Next.Client
open WebSharper.JavaScript

module Server =
  let fsiPath = 
    match ConfigurationManager.AppSettings.["FsiPath"] with
    | null -> System.IO.Path.Combine( System.AppDomain.CurrentDomain.BaseDirectory, "bin", "fsi", "Fsi.exe" )
    | s -> s

  let runFsi = CodeRunner.runFsi (Log.log4netLogger ()) fsiPath
  let run = TurtleRunner.run runFsi

  let [<Remote>] RunTurtle code = async { return run code }

[<JavaScript>]
module Client =
  open Turtle
  open Drawing

  let delay timeout x = async { 
    do! Async.Sleep timeout
    return x }

  type RunInput = { Code: string; Options: DrawOptions; }
  type RunResult = Commands of Turtle.Command list | Error of string | NoResult

  let cls = attr.``class``
  let divC c = divAttr [cls c]


  let runWithRetry code =
    let rec r count = async {
      let! result = Server.RunTurtle code
      match result |*> Turtle.deserialize with
      | Result.OK commands -> 
          return Commands commands
      | Result.Error err -> 
          return Error err
      | Timeout -> 
          if count = 0 then 
            return NoResult
          else
            do! Async.Sleep 50
            return! r (count-1)
    }
    r 5


  let runView code =
    let loadingFlag = Var.Create false
    let runResult = Var.Create NoResult

    code 
    |> View.MapAsync (delay 300) 
    |> View.Sink (fun code -> 
      Async.Start <|
        async { 
          Var.Set loadingFlag true
          let! r = runWithRetry code
          Var.Set loadingFlag false
          Var.Set runResult r } )

    loadingFlag, runResult


  let editor code = Doc.InputArea [] code
  let loadingDiv = Doc.BindView (fun f -> divC (if f then "loading show" else "loading") [])

  let codeLocalStorageKey = "Code"

  let page () =
    let code = Var.Create (JS.Window.LocalStorage.GetItem codeLocalStorageKey |> (fun x -> if x = null then "" else x))
    code.View |> View.Sink (fun c -> JS.Window.LocalStorage.SetItem( codeLocalStorageKey, c))

    let scale = Var.Create 0.5
    let canvas = canvas []
    let loading, runResult = runView code.View

    let output = 
      View.Do {
        let! runResult = runResult.View
        let! scale = scale.View
        let scale = exp <| (0.5-scale)*10.
        let opts = { Scale = scale; CenterX = canvas.Dom.ClientWidth/2.; CenterY = canvas.Dom.ClientHeight/2. }

        return
          match runResult with
          | Commands cmds -> 
              drawCommands (canvas.Dom |> As<CanvasElement>) opts cmds
              span []
          | Error err -> 
              span [text err]
          | _ -> 
              span []
      }

    divC "page" [
      divC "code" [editor code]
      divC "image" [canvas; Slider.create scale; loadingDiv loading.View]
      divC "output" [Doc.BindView id output]
    ]

[<Website>]
let site =
  Application.SinglePage (fun ctx ->
    Content.Page(
      Head = [ linkAttr [attr.rel "stylesheet"; attr.href (ctx.ResolveUrl "/Content/app.css")] [] ],
      Body = [ client <@ Client.page () @> ] ) )