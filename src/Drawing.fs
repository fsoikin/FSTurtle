namespace Turtle
open WebSharper
open WebSharper.JavaScript

[<JavaScript>]
module Drawing =
  open Turtle

  type DrawOptions = {
    Scale: float
    CenterX: float
    CenterY: float }

  type TurtleState = {
    Color: string; 
    PenDown: bool;
    Position: int * int }
    with static member Start = { Color = "black"; PenDown = false; Position = 0,0 }
  let translate opts (x,y) = x*opts.Scale + opts.CenterX, y*opts.Scale + opts.CenterY

  let line opts (ctx: CanvasRenderingContext2D) color x1 y1 x2 y2 =
    ctx.StrokeStyle <- color
    ctx.BeginPath()
    ctx.MoveTo (translate opts (float x1, float y1))
    ctx.LineTo (translate opts (float x2, float y2))
    ctx.Stroke()

  let drawCmd line state = function
    | PenUp -> { state with PenDown = false }
    | PenDown -> { state with PenDown = true }
    | Color c -> { state with Color = c }
    | Move (dx,dy) -> 
      let x, y = state.Position
      let x', y' = x+dx, y+dy
      if state.PenDown then line state.Color x y x' y'
      { state with Position = (x', y') }

  let drawCommands (canvas: CanvasElement) opts commands =
    let ctx = canvas.GetContext "2d"
    canvas.Width <- int ctx.Canvas.ClientWidth
    canvas.Height <- int ctx.Canvas.ClientHeight
    commands |> List.fold (drawCmd (line opts ctx)) TurtleState.Start |> ignore
