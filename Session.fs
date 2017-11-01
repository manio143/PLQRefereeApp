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
open Action

let createSession user =
    let sessionId = createSessionId()
    let csrfToken = createCSRFToken()
    match user with
    | None -> NotLoggedIn (sessionId, csrfToken)
    | Some usr -> LoggedIn (sessionId, usr, csrfToken, None)

let session (action:Action) =
    context (fun httpContext ->
                debugLog "Activate session" httpContext
                let withNewSession () =
                    let session = createSession None
                    verboseLog "  New session created" httpContext
                    do saveSession session
                    setSessionCookie session >=> withVerboseLog (sprintf "Session: %A" session) >=> action session httpContext
                match getSessionCookie httpContext with
                | None ->
                    withNewSession ()
                | Some sessionCookie ->
                    match getSession sessionCookie.value with
                    | None ->
                        withNewSession ()
                    | Some session ->
                        withVerboseLog (sprintf "Session: %A" session) >=> action session httpContext
            )

let newCsrfToken =
    fun session (ctx:HttpContext) ->
        let newCsrf = createCSRFToken ()
        let session = 
            match session with
            | NotLoggedIn (id, _) -> NotLoggedIn(id, newCsrf)
            | LoggedIn (id, usr, _, test) -> LoggedIn(id, usr, newCsrf, test)
        saveSession session
        setSessionCookie session

let validateCSRF csrf (session:Session) = csrf = session.Csrf

let POST =
    let validateCSRF session httpContext =
        let csrftoken = (postData httpContext.request "csrftoken"
                        >>=> (httpContext.request.header "X-CSRF-Token" |> optionOfChoice)
                        >>=> Some "").Value
        verboseLog (sprintf "csrftoken = %s" csrftoken) httpContext
        validateCSRF csrftoken session

    fun sess ctx ->
        POST >=> withDebugLog "Validating CSRF token"
        >=> (if validateCSRF sess ctx then newCsrfToken sess ctx
             else 
                withDebugLog "CSRF validation failed."
                >=> Views.CSRFValidationFailed sess ctx
            )

let sessionWithTest testOption =
    fun session context ->
        let session = 
            match session with
            | LoggedIn (id, usr, csrf, _) -> LoggedIn(id, usr, csrf, testOption)
            | x -> x
        saveSession session
        withDebugLog (sprintf "Saved test to session {%A}" testOption) >=> setSessionCookie session
