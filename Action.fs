module Action

open Suave
open Suave.Operators

open Domain


type Action = Session -> HttpContext -> WebPart

type Modifier = Session -> HttpContext -> Action -> Action

let compose (action1:Action) (action2:Action) =
    fun sess ctx ->
        action1 sess ctx >=> context (fun ctx2 -> action2 sess ctx2)

let apply (modifier:Modifier) (action:Action) : Action = 
    fun sess ctx ->
        modifier sess ctx action sess ctx

let ofWebPart (webpart:WebPart) =
    fun (sess:Session) (ctx:HttpContext) (ctx2:HttpContext) ->
        async { return! webpart ctx2}

let resolve (action:Action) (f:HttpContext option -> Action) =
    fun (session:Session) (ctx:HttpContext) (ctx2:HttpContext) ->
        async {
            let! result = action session ctx2 ctx2
            return! f result session ctx2 ctx2
        }

let (>+=>) = compose

let (?>) = apply