#if !INTERACTIVE
module Turtle.Turtle
#endif

type Color = string

type Command =
  | PenUp
  | PenDown
  | Color of Color
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
        try
          Some <| Move (System.Int32.Parse ds.[0], System.Int32.Parse ds.[1])
        with 
          _ -> None
    | _ -> None 
      
  text.Split [| ';' |]
  |> Seq.choose d
  |> Seq.toList


type Point = Point of x: int * y: int with static member (+) ( (Point (ax,ay)), (Point (bx, by)) ) = Point (ax+bx, ay+by)
type Shape = Line of start: Point * finish: Point * color: Color
type Picture = Picture of Shape list

type State = { Shapes: Shape list; Position: Point; Color: string; PenDown: bool }
  with static member Empty = { Shapes = []; Position = Point(0,0); Color = "black"; PenDown = false }

let pictureFromCommands cmds = 
  let onCmd state = function
    | PenUp -> { state with PenDown = false }
    | PenDown -> { state with PenDown = true }
    | Color c -> { state with Color = c }
    | Move (dx, dy) when state.PenDown ->
        let finish = state.Position + Point(dx, dy)
        let line = Line (state.Position, finish, state.Color)
        { state with Shapes = line::state.Shapes; Position = finish }
    | Move (dx, dy) ->
        { state with Position = state.Position + Point(dx, dy) }

  let s = List.fold onCmd State.Empty cmds
  Picture s.Shapes