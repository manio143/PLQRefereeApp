module Authentication

open Suave
open Suave.Authentication
open Suave.Cookie
open Suave.State.CookieStateStore
open Suave.Operators

open Domain
open Db
open Cookies
open Session
open Routes

(* allow user to pass to action if authenticated, otherwise redirect to login *)
let loggedOn action =
    session (function
        | NotLoggedIn _ -> Routes.redirectWithReturnPath Routes.login
        | LoggedIn _ -> action)

let notLoggedOn action =
    session (function
        | NotLoggedIn _ -> action
        | LoggedIn _ -> Routes.returnPathOrHome)

let loggedAdmin action =
    loggedOn (session (function
        | LoggedIn (_, user, _, _) -> if user.IsAdmin() then action
                                      else Views.Forbidden
        | _ -> Views.Unauthorized ))

let authenticateUser (user:Domain.User) =
    session (fun _ ->
                let session = createSession (Some user)
                saveSession session
                setSessionCookie session
            )
    >=> returnPathOrHome
