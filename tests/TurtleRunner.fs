module Turtle.TurtleRunner.Tests
open Turtle
open CodeRunner
open Turtle
open TurtleRunner
open Swensen.Unquote
open Xunit

let runFsi _ = 42

let [<Fact>] ``Basic`` () = test <@ (run runFsi "") = 42 @>