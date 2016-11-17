module Turtle.CodeRunner
open System
open System.Diagnostics
open System.IO
open FSharp.Control.Reactive
open System.Text

let runProcess (logger:Log.Logger) cmd dir args writeInput =
  let debug = logger Log.Debug
  let p =
    ProcessStartInfo(
      UseShellExecute = false, 
      FileName = cmd,
      WorkingDirectory = dir,
      RedirectStandardInput = true,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      CreateNoWindow = true,
      Arguments = args )
    |> Process.Start
    
  debug <| sprintf "Started %s, sending input..." cmd
  writeInput p.StandardInput
    
  let wait =
    async {
      debug "Sent input, waiting for execution to end..."
      let output = p.StandardOutput.ReadToEnd()
      let err = p.StandardError.ReadToEnd()
      debug <| sprintf "Execution ended, stderr output = %s" err
      return
        if err <> "" then Error err
        else OK output
    }
    |> Observable.ofAsync
  
  let killAfterTimeout =
    Observable.timerSpan (TimeSpan.FromMilliseconds 10000.)
    |> Observable.map (fun _ -> p.Kill(); debug ("Timeout: " + p.StandardOutput.ReadToEnd()); Timeout)
    
  Observable.merge wait killAfterTimeout |> Observable.take 1 |> Observable.wait

let runFsi (logger:Log.Logger) fsiPath (code:string) = 
  runProcess logger 
    fsiPath (Path.GetDirectoryName fsiPath) 
    "--gui- --nologo --quiet "
    <| fun input ->
        input.WriteLine code
        input.WriteLine ";; #quit;;"
