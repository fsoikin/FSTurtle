#r "../packages/FSharp.Compiler.Service/lib/net45/FSharp.Compiler.Service.dll"
#r "../packages/Rx-Linq/lib/net40/System.Reactive.Linq.dll"
#r "../packages/Rx-Core/lib/net40/System.Reactive.Core.dll"
#r "../packages/Rx-Interfaces/lib/net40/System.Reactive.Interfaces.dll"
#r "../packages/FSharp.Control.Reactive/lib/net40/FSharp.Control.Reactive.dll"
#load "CodeRunner.fs"
#load "Turtle.fs"
  
open CodeRunner
open Turtle

serialize [PenUp; PenDown; Color "a"] |> deserialize