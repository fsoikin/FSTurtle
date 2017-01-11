module Turtle.Client.Image
open Turtle
open Turtle
open Svg
open Fable.Arch
open Fable.Arch.App
open Fable.Arch.Html

type Model = { 
    Pic: Turtle.Picture
    Scale: float }

type Msg = 
    | UpdatePic of Turtle.Picture
    | Scale of float

let initModel = { Pic = Turtle.Picture []; Scale = 1. }

let shape = function
    | Line ( Point (x1, y1), Point (x2, y2), color ) ->
        svgElem "line" [ "x1" ==> x1; "y1" ==> y1; "x2" ==> x2; "y2" ==> y2; strokeWidth "2"; stroke color ] []

let picture (Picture shapes) = [for s in shapes -> shape s]

let view model = 
    div [classy "image"] 
        [svg 
            [viewBox "-500 -500 1000 1000"; attribute "preserveAspectRatio" "xMidYMid"] 
            (picture model.Pic)
        ]

let update model = function
    | UpdatePic pic -> 
        { model with Pic = pic }

    | Scale f ->
        { model with Scale = f }