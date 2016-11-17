module Turtle.CodeRunner.Tests
open Turtle
open CodeRunner
open Swensen.Unquote
open Xunit

let fsiPath = System.IO.Path.Combine( System.AppDomain.CurrentDomain.BaseDirectory, "fsi", "fsi.exe" )

let [<Fact>] ``Basic`` () =
  let res = runFsi (fun _ _ -> ()) fsiPath """printf "abc" """ 
  test <@ res = OK "abc" @>

let [<Fact>] ``Error`` () =
  let res = runFsi (fun _ _ -> ()) fsiPath """printf 55 """ 
  let isError = function | Error _ -> true | OK (_:string) | _ -> false
  test <@ isError res = true @>