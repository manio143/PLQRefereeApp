module Helpers

open Suave
open Suave.Logging

[<AutoOpen>]
module ChoiceOption =
    let optionOfChoice c =
        match c with
        | Choice1Of2 x -> Some x
        | Choice2Of2 _ -> None        

    (** returns a if a.IsSome else b **)
    let (>>=>) a b =
        match a with
        | Some x -> Some x
        | None ->
            match b with
            | Some x -> Some x
            | None -> None

    let postData (ctx:HttpRequest) key =
        ctx.formData key |> optionOfChoice
        >>=> (ctx.fieldData key |> optionOfChoice)

[<AutoOpen>]
module Logging =
    let logMessage level message ctx =
        ctx.runtime.logger.log level (fun _ -> Message.event level message) |> Async.RunSynchronously

    let verboseLog message = logMessage Verbose message
    let debugLog message = logMessage Debug message
    let withVerboseLog (message:string) =
        context (function ctx ->
                                fun ctx2 -> async {
                                    do verboseLog message ctx
                                    return Some ctx2
                                }
                )
    let withDebugLog (message:string) =
        context (function ctx ->
                                fun ctx2 -> async {
                                    do debugLog message ctx
                                    return Some ctx2
                                }
                )

let makeCSRFinput csrftoken =
    sprintf "<input type=\"hidden\" name=\"csrftoken\" value=\"%s\">" csrftoken
