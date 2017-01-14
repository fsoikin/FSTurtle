module Turtle.Client.Slider
open Fable.Core.JsInterop
open Fable.Arch
open Fable.Arch.App
open Fable.Arch.Html

type Model = private {
    Pos: float
    ButtonDown: bool
}

let pos model = model.Pos
let init pos = { Pos = pos; ButtonDown = false }

type Msg = 
    private
    | Move of float
    | Down
    | Up

let view model = 
    div [classy "slider"
         onMouseMove (fun e -> Move ( (unbox<float> e?offsetY + unbox e?target?offsetTop - unbox e?currentTarget?offsetTop) / unbox e?currentTarget?offsetHeight ))
         onMouseUp (fun _ -> Up)]
        [div [classy "tab"
              "style" =/> (sprintf "top: %.2f%%" <| 100.*model.Pos)
              onMouseDown (fun _ -> Down)] 
             []]

let update model = function 
    | Move f when model.ButtonDown && f >= 0. && f <= 1. -> { model with Pos = f }
    | Move _ -> model
    | Up -> { model with ButtonDown = false }
    | Down -> { model with ButtonDown = true }