module Session

open Suave
open Suave.State.CookieStateStore
open Suave.Http
open Suave.Operators
open Suave.Filters
open Suave.Logging

open Domain
open Db
open Cookies
open Helpers
open Generators

let createSession user =
    let sessionId = createSessionId()
    let csrfToken = createCSRFToken()
    match user with
    | None -> NotLoggedIn (sessionId, csrfToken)
    | Some usr -> LoggedIn (sessionId, usr, csrfToken, None)

let session (action:Session->WebPart) =
    context (fun httpContext ->
                debugLog "Activate session" httpContext
                let withNewSession () =
                    let session = createSession None
                    verboseLog "  New session created" httpContext
                    do saveSession session
                    setSessionCookie session >=> withVerboseLog (sprintf "Session: %A" session) >=> action session
                match getSessionCookie httpContext with
                | None ->
                    withNewSession ()
                | Some sessionCookie ->
                    match getSession sessionCookie.value with
                    | None ->
                        withNewSession ()
                    | Some session ->
                        withVerboseLog (sprintf "Session: %A" session) >=> action session
            )

let withSession action = session (fun _ -> action)

let newCsrfToken =
    session (fun session ->
                let newCsrf = createCSRFToken ()
                let session = 
                    match session with
                    | NotLoggedIn (id, _) -> NotLoggedIn(id, newCsrf)
                    | LoggedIn (id, usr, _, test) -> LoggedIn(id, usr, newCsrf, test)
                saveSession session
                setSessionCookie session
            )

let withCSRF action =
    session (function session -> action (session.Csrf))

let validateCSRF csrf = 
    session (fun session ->
            fun x -> async {
                if csrf = session.Csrf then return Some x
                else return None
            }
    )

let POST (action:WebPart) httpContext =
    let validateCSRF httpContext =
        let csrftoken = (postData httpContext.request "csrftoken"
                        >>=> (httpContext.request.header "X-CSRF-Token" |> optionOfChoice)
                        >>=> Some "").Value
        verboseLog (sprintf "csrftoken = %s" csrftoken) httpContext
        validateCSRF csrftoken httpContext
    async {
        verboseLog (sprintf "POST { %s }" (httpContext.request.rawForm |> System.Text.Encoding.UTF8.GetString)) httpContext
        let! r = ((POST >=> withDebugLog "Validating CSRF token" >=> validateCSRF) httpContext)
        match r with
        | Some x -> return! (newCsrfToken >=> action) x
        | None -> return! (withDebugLog "CSRF validation failed." >=> Views.CSRFValidationFailed) httpContext
     }
