module Cookies

open System

open Suave
open Suave.Http
open Suave.Cookie
open Suave.Operators

open Domain
open Helpers

#if DEBUG
let SECURE = false
#else
let SECURE = true
#endif

let HTTPONLY = true

let csrfTokenCookie = "CSRFToken"
let sessionCookie = "Session"

let createCookie name value expires secure httpOnly =
    { HttpCookie.createKV name value with
        expires = expires
        secure = secure
        httpOnly = httpOnly
    }

let createCSRFCookie token =
    let tomorrow = System.DateTime.Now.AddHours(12.0)    
    createCookie csrfTokenCookie token (Some (DateTimeOffset(tomorrow))) SECURE (not HTTPONLY)

let createSessionCookie (session:Session) =
    let tomorrow = System.DateTime.Now.AddHours(12.0)
    createCookie sessionCookie session.SessionId (Some (DateTimeOffset(tomorrow))) SECURE HTTPONLY

let setSessionCookie (session:Session) =
    let csrfCookie = createCSRFCookie session.Csrf
    let sessionCookie = createSessionCookie session
    withDebugLog "Setting session cookies" >=> setCookie csrfCookie >=> setCookie sessionCookie

let removeSessionCookie =
    unsetCookie csrfTokenCookie >=> unsetCookie sessionCookie

let getCSRFCookie httpContext =
    let cookie = httpContext.request.cookies.TryFind csrfTokenCookie
    verboseLog (sprintf "Getting CSRF cookie (%A)" (Option.map (fun c -> c.value) cookie)) httpContext
    cookie

let getSessionCookie httpContext =
    let cookie = httpContext.request.cookies.TryFind sessionCookie
    verboseLog (sprintf "Getting session cookie (%A)" (Option.map (fun c -> c.value) cookie)) httpContext
    cookie
