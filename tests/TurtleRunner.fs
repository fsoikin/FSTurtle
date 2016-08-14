module Turtle.TurtleRunner.Tests
open Turtle
open CodeRunner
open Turtle
open TurtleRunner
open Swensen.Unquote
open Xunit

let fsiPath = CodeRunner.Tests.fsiPath

let [<Fact>] ``Basic`` () =
  let res = run fsiPath "penUp(); penDown(); move 5 6"
  let expected = OK [PenUp; PenDown; Move (5,6)]
  test <@ res = expected @>


let [<Fact>] ``Complex program`` () =
  let res = run fsiPath """
let repeat f n = for i = 1 to n do f()
let mv d () = move d d

penDown()
repeat (mv 5) 3
penUp()
move -10 0
penDown()
repeat (mv 3) 2
penUp()
    """
  let expected = OK [PenDown; Move (5,5); Move (5,5); Move (5,5); PenUp; Move (-10,0); PenDown; Move (3,3); Move (3,3); PenUp]
  test <@ res = expected @>