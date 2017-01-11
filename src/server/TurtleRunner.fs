module Turtle.TurtleRunner

type private Dummy = Dummy
let private resman = lazy System.Resources.ResourceManager("Resources", typeof<Dummy>.Assembly)
let TurtleCode() = resman.Value.GetString "TurtleCode"

let run runFsi code =
  sprintf 
    "%s\nopen Ambient\n%s\nprintf \"%%s\" (serialize (List.rev Ambient.commands))" 
    (TurtleCode()) code
  |> runFsi