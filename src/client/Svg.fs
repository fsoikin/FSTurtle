module Turtle.Client.Svg
open Fable.Arch
open Fable.Arch.App
open Fable.Arch.Html

type Svg = Svg

let (?) (s: Svg) name = svgElem name
let (==>) (name: string) v = attribute name (string v)