module Authentication

open Suave
open Suave.Authentication
open Suave.Cookie
open Suave.State.CookieStateStore
open Suave.Operators

open Action
open Domain
open Db
open Cookies
open Session
open Routes

(* allow user to pass to action if authenticated, otherwise redirect to login *)
let loggedOn session (ctx:HttpContext) (action:Action) =
    match session with
    | LoggedIn _ -> action
    | NotLoggedIn _ -> Action.ofWebPart (Routes.redirectWithReturnPath Routes.login)

let notLoggedOn session (ctx:HttpContext) (action:Action) =
    match session with
    | NotLoggedIn _ -> action
    | LoggedIn _ -> Action.ofWebPart (Routes.returnPathOrHome)

let loggedAdmin (session:Session) (ctx:HttpContext) (action:Action) =
    if session.User.IsSome && session.User.Value.IsAdmin then action
    else Views.Forbidden

let authenticateUser (user:Domain.User) =
    let session = createSession (Some user)
    saveSession session
    setSessionCookie session
    >=> returnPathOrHome
    |> Action.ofWebPart
