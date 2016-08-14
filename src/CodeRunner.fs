module Turtle.CodeRunner
open System
open System.Diagnostics
open System.IO
open FSharp.Control.Reactive

let run fsiPath (code:string) = 
  let fsiDir = Path.GetDirectoryName fsiPath
  let p =
    ProcessStartInfo(
      UseShellExecute = false, 
      FileName = fsiPath,
      WorkingDirectory = fsiDir,
      RedirectStandardInput = true,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      CreateNoWindow = true,
      Arguments = "--gui- --nologo --quiet " )
    |> Process.Start
    
  p.StandardInput.WriteLine code
  p.StandardInput.WriteLine ";; #quit;;"
    
  let wait =
    async {
      p.WaitForExit()

      let err = p.StandardError.ReadToEnd()
      return
        if err <> "" then Error err
        else OK (p.StandardOutput.ReadToEnd())         
    }
    |> Observable.ofAsync
  
  let killAfterTimeout =
    Observable.timerSpan (TimeSpan.FromMilliseconds 3000.)
    |> Observable.map (fun _ -> p.Kill(); Timeout)
    
  Observable.merge wait killAfterTimeout |> Observable.take 1 |> Observable.wait
