module Helpers

open Suave
open Suave.Logging

let postData (ctx:HttpRequest) key =
    ctx.formData key |> function
        | Choice1Of2 x -> Some x
        | Choice2Of2 _ -> None

let debugLog message ctx =
    ctx.runtime.logger.log Debug (fun _ -> Message.event Debug message) |> Async.RunSynchronously

let withDebugLog (message:string) =
    context (function ctx ->
                            fun ctx2 -> async {
                                do debugLog message ctx
                                return Some ctx2
                            }
            )

let makeCSRFinput csrftoken =
    sprintf "<input type=\"hidden\" name=\"csrftoken\" value\"%s\">" csrftoken
