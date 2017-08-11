module Authentication

open Suave
open Suave.Authentication
open Suave.Cookie
open Suave.State.CookieStateStore
open Suave.Operators

open Session
open Routes

(* remove Session Cookies *)
let reset =
    deauthenticate
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

let authenticateUser (user:Domain.User) =
    authenticated Cookie.CookieLife.Session true 
    >=> sessionStore (fun store -> store.set "userid" user.Id)
    >=> returnPathOrHome
