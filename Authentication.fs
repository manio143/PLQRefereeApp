module Authentication

open Suave
open Suave.Authentication
open Suave.Cookie
open Suave.State.CookieStateStore
open Suave.Operators

open Session

(* remove Session Cookies *)
let reset =
    unsetPair SessionAuthCookie
    >=> unsetPair StateCookie
    >=> Redirection.FOUND Routes.index

(* allow user to pass to action if authenticated, otherwise redirect to login *)
let loggedOn action =
    authenticate
        Cookie.CookieLife.Session
        true (* Enforce Cookie.Secure *)
        (fun () -> Choice2Of2(Routes.redirectWithReturnPath Routes.login))
        (fun _ -> Choice2Of2 reset)
        action

let loggedAdmin action =
    loggedOn (session (function
        | LoggedIn (user, _, _) -> if user.IsAdmin() then action
                                   else Views.Forbidden
        | _ -> Views.Unauthorized ))