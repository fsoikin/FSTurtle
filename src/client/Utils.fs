[<AutoOpen>]
module Turtle.Client.Utils
open Fable.Arch.Html

let (=/>) (name: string) v = attribute name (string v)