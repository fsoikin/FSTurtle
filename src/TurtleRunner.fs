module Turtle.TurtleRunner

type private Dummy = Dummy
let private resman = lazy System.Resources.ResourceManager("Resources", typeof<Dummy>.Assembly)
let TurtleCode() = resman.Value.GetString "TurtleCode"

let run fsiPath code =
  let code = sprintf "%s\nopen Ambient\n%s\nprintf \"%%s\" (serialize (List.rev Ambient.commands))" (TurtleCode()) code
  CodeRunner.run fsiPath code |*> Turtle.deserialize