namespace Turtle
open WebSharper
open WebSharper.UI.Next
open WebSharper.UI.Next.Html
open WebSharper.UI.Next.Client
open WebSharper.JavaScript

[<JavaScript>]
module Slider =
  let create (position: Var<float>) =

    let knob = divAttr [attr.``class`` "knob"] [div []]
    let setKnobPos pos =
      let pos = int (pos*100.)
      let pos = sprintf "%d%%" pos
      knob.SetStyle ("top", pos)

    let mutable mouseDown = false
    let slider = divAttr [attr.``class`` "slider"] [knob]
    position.View |> View.Sink setKnobPos
    let updatePosition (ev: Dom.MouseEvent) = position.Value <- (float ev.ClientY) / slider.Dom.ClientHeight
    slider
      .OnMouseDown(fun _ ev -> mouseDown <- true; updatePosition ev)
      .OnMouseUp(fun _ _ -> mouseDown <- false)
      .OnMouseMove(fun _ ev -> if mouseDown then updatePosition ev)