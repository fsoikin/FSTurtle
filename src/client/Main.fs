module Turtle.Client.Main

open Fable
open Fable.Core
open Fable.Core.JsInterop
open Fable.PowerPack
open Fable.Arch
open Fable.Arch.App
open Fable.Arch.Html
open Fable.Import
open Turtle

module Fetch =
    open Fetch.Fetch_types

    let resultToPromise = function 
        | OK x -> Promise.create (fun succ err -> succ x)
        | Error e -> Promise.create (fun succ err -> err (exn e))
        | Timeout -> Promise.create (fun succ err -> err (exn "Timeout"))

    let fetch code = promise {
        let! r = Fetch.fetch "/run" [Body (Case3 code); Method HttpMethod.POST]
        let! u = r.text()
        return! 
            ofJson<Turtle.Common.Result<string>> u
            |*> Turtle.deserialize
            |*> Turtle.pictureFromCommands
            |> resultToPromise
    }

type Model = {
    Code: string
    Errors: string list
    Image: Image.Model
    State: State
}
and State = WaitingToFetch of int | Fetching of int | Idle

type Msg =
    | UpdateCode of string
    | Fetch of int
    | Fetched of int * Turtle.Picture
    | FailedToFetch of string

let codeView model =
    div [classy "code"] 
        [textarea 
            [onKeyup (fun e -> unbox<string>(e?target?value))] 
            [text model]]

let errorsView model =
    let errs = [for e in model -> span [] [text e]]
    div [classy "errors"] errs

let view model =
    div [] 
        [codeView model.Code |> Html.map UpdateCode
         Image.view model.Image
         errorsView model.Errors]

let actn p k = p |> Promise.catch (fun e -> FailedToFetch e.Message) |> Promise.iter k

let timeout tag = actn <| promise {
    do! Promise.sleep 300
    return Fetch tag
}

let fetch tag code = actn <| promise {
    let! r = Fetch.fetch code
    return Fetched (tag, r)
}

let update model = function
    | UpdateCode code -> 
        let tag = int System.DateTime.Now.Ticks
        { model with Code = code; State = WaitingToFetch tag }, [timeout tag]

    | Fetch tag ->
        match model.State with
        | WaitingToFetch tag' when tag = tag' ->
            { model with State = Fetching tag }, [fetch tag model.Code]
        | _ ->
            model, []

    | Fetched (tag, res) ->
        match model.State with
        | Fetching tag' when tag = tag' ->
            { model with Image = Image.update model.Image (Image.UpdatePic res); State = Idle; Errors = [] }, []
        | _ ->
            model, []

    | FailedToFetch msg ->
        { model with Errors = ["ERROR: " + msg] }, []

createApp 
    { Code = ""; Errors = []; Image = Image.initModel; State = Idle }
    view update Fable.Arch.Virtualdom.createRender
|> withStartNodeSelector "#root"
|> withSubscriber (printfn "%A")
|> start