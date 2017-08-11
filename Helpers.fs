module Helpers

open Suave

let postData (ctx:HttpRequest) key =
    ctx.formData key |> function
        | Choice1Of2 x -> Some x
        | Choice2Of2 _ -> None