namespace Turtle.Tests
open Turtle.Turtle
open Swensen.Unquote
open Xunit
open FsCheck.Xunit

module Serialization =

  open FsCheck
  type NonNullStrings =
    static member strings () = Arb.Default.String () |> Arb.filter ((<>) null)
  Arb.register<NonNullStrings> () |> ignore

  let [<Fact>] ``Basic`` () =
    let res = serialize [PenUp; Color "x"; Move (1,2); PenDown; Move (3,4); Color "y"]
    let expected = "U;C'x';M1,2;D;M3,4;C'y'"
    test <@ res = expected @>

  let [<Fact>] ``Commas and semicolons in colors get cut out`` () =
    test <@ serialize [Color "'"] = "C''" @>
    test <@ serialize [Color ";"] = "C''" @>
    test <@ serialize [Color "a'b"] = "C'ab'" @>
    test <@ serialize [Color "x;y"] = "C'xy'" @>

  let isNullOrBannedColor = function 
    | Color null -> true
    | Color c when c.Contains "'" || c.Contains ";" -> true
    | _ -> false
  let noNullsOrBannedColors = not << List.exists isNullOrBannedColor

  let [<Property>] ``serialize and deserialize are dual`` commands =
    noNullsOrBannedColors commands ==> 
      fun() -> (serialize commands |> deserialize) = commands