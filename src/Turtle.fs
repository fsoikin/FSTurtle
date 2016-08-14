#if !INTERACTIVE
module Turtle.Turtle
#endif

type Command =
  | PenUp
  | PenDown
  | Color of string
  | Move of dx: int * dy: int
  
module Ambient =

  let mutable commands = []

  let start() = commands <- []

  let penUp() = commands <- PenUp :: commands
  let penDown() = commands <- PenDown :: commands
  let color c = commands <- (Color c) :: commands
  let move x y = commands <- (Move (x,y)) :: commands

  
let serialize cmds =
  let s = function
    | PenUp -> "U" | PenDown -> "D" 
    | Color c -> sprintf "C'%s'" (c.Replace("'", "").Replace(";", ""))
    | Move (x,y) -> sprintf "M%d,%d" x y
  cmds |> Seq.map s |> String.concat ";"
    
let deserialize (text: string) =
  let d = function
    | "U" -> Some PenUp | "D" -> Some PenDown
    | s when s.StartsWith "C'" && s.EndsWith "'" -> Some <| Color (s.Substring(2, s.Length-3))
    | s when s.StartsWith "M" ->
      let ds = s.Substring(1).Split [| ',' |]
      if ds.Length <> 2 then None
      else
        match System.Int32.TryParse ds.[0], System.Int32.TryParse ds.[1] with
        | (true, x), (true, y) -> Some <| Move (x, y)
        | _ -> None
    | _ -> None 
      
  text.Split [| ';' |]
  |> Seq.choose d
  |> Seq.toList