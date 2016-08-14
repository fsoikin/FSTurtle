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
    | null -> System.IO.Path.Combine( System.AppDomain.CurrentDomain.BaseDirectory, "bin", "fsi", "fsi.exe" )
    | s -> s

  let [<Remote>] RunTurtle code = async { 
    return TurtleRunner.run fsiPath code
  }

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

  let run code =
    let rec r count = async {
      let! result = Server.RunTurtle code
      match result with
      | Result.OK commands -> return Commands commands
      | Result.Error err -> return Error err
      | Timeout -> 
          if count = 0 then return NoResult
          else
            do! Async.Sleep 50
            return! r (count-1)
    }
    r 5

  let editor code = Doc.InputArea [] code

  let codeLocalStorageKey = "Code"

  let page () =
    let code = Var.Create (JS.Window.LocalStorage.GetItem codeLocalStorageKey)
    code.View |> View.Sink (fun c -> JS.Window.LocalStorage.SetItem( codeLocalStorageKey, c))

    let scale = Var.Create 0.5
    let canvas = canvas []
    let output = 
      View.Do {
        let! runResult = code.View |> View.MapAsync (delay 100) |> View.MapAsync run
        let! scale = scale.View
        Console.Log (sprintf "%A" scale)
        let scale = exp <| (0.5-scale)*10.
        Console.Log (sprintf "%A" scale)
        let opts = { Scale = scale; CenterX = canvas.Dom.ClientWidth/2.; CenterY = canvas.Dom.ClientHeight/2. }

        let _ =
          match runResult with
          | Commands cmds -> drawCommands (canvas.Dom |> As<CanvasElement>) opts cmds
          | _ -> ()
       
        let output =
          match runResult with
          | Error err -> span [text err]
          | _ -> span []

        return output
      }

    divC "page" [
      divC "code" [editor code]
      divC "image" [canvas; Slider.create scale]
      divC "output" [Doc.BindView id output]
    ]

[<Website>]
let site =
  Application.SinglePage (fun ctx ->
    Content.Page(
      Head = [ linkAttr [attr.rel "stylesheet"; attr.href (ctx.ResolveUrl "/Content/app.css")] [] ],
      Body = [ client <@ Client.page () @> ]
    ) )
