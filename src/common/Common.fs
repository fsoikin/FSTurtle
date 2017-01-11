[<AutoOpen>]
module Turtle.Common

type Result<'t> = OK of 't | Error of string | Timeout

let (>>=) r f = match r with | OK x -> f x | Error s -> Error s | Timeout -> Timeout
let (|*>) r f = r >>= (OK << f)