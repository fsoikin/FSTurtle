namespace Turtle.Tests
open Turtle.Turtle
open Swensen.Unquote
open Xunit
open FsCheck.Xunit

module Serialization =

  open FsCheck

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

  type CommandGenerator() =
    static member Gen: Arbitrary<Command> = Arb.Default.Derive<Command>() |> Arb.filter (not << isNullOrBannedColor)

  let [<Property(Arbitrary = [|typeof<CommandGenerator>|])>] ``serialize and deserialize are dual`` commands =
    (serialize commands |> deserialize) = commands