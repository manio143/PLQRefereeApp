module Session

type CSRF = string
let createCSRF () : CSRF =
    let g = System.Guid.NewGuid()
    let sha = System.Security.Cryptography.SHA1.Create()
    System.Convert.ToBase64String <| sha.ComputeHash(g.ToByteArray())

open Suave
open Suave.State.CookieStateStore
open Suave.Http
open Suave.Operators
open Suave.Filters

open Domain
open Db

type Session =
    | NotLoggedIn of CSRF
    | LoggedIn of User * CSRF * Test option

let session (action:Session->WebPart) =
    statefulForSession
    >=> context (fun httpContext ->
            match HttpContext.state httpContext with
            | None -> 
                action (NotLoggedIn (createCSRF()))
            | Some state ->
                match state.get "userid", state.get "csrf", state.get "testid" with
                | Some userId, Some csrf, test ->
                    action (LoggedIn (getUser userId, csrf, if test.IsSome then Some(getTest test.Value) else None))
                | None, Some csrf, _ ->
                    action (NotLoggedIn (csrf))
                | _ -> action (NotLoggedIn (createCSRF()))
            )

let validateCSRF csrf = 
    session (fun session ->
            let _csrf = 
                match session with
                | NotLoggedIn ( _csrf) -> _csrf
                | LoggedIn (_, _csrf, _) -> _csrf
            fun x -> async {
                if csrf = _csrf then return Some x
                else return None
            }
    )

let POST (action:WebPart) httpContext =
    let validateCSRF httpContext =
        let csrftoken = httpContext.request.fieldData "csrftoken" 
                        |> function 
                            | Choice1Of2 (s) -> s 
                            | _ -> httpContext.request.header "X-CSRF-Token" 
                                   |> function
                                      | Choice1Of2 (s) -> s
                                      | _ -> ""
        validateCSRF csrftoken httpContext
    async {
        let! r = ((POST >=> validateCSRF) httpContext)
        match r with
        | Some x -> return! action x
        | None -> return! Views.CSRFValidationFailed httpContext  
     }