module App

open Suave
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open Suave.Successful
open Suave.Logging

open Session
open Action
open Controllers
open Helpers
open Suave.Redirection

let processAction action =
    session action

let app =
    choose [
        path Routes.index >=> processAction Index.page
        path Routes.materials >=> processAction Materials.page
        path Routes.payment >=> processAction Payment.page
        
        path Routes.login >=> processAction Login.page
        path Routes.logout >=> Login.reset
        path Routes.register >=> processAction Register.page

        path Routes.directory >=> processAction Directory.page
        pathScan Routes.profile (Profile.page >> processAction)

        path Routes.Account.myAccount >=> processAction Account.page

        path Routes.Tests.AR >=> processAction Tests.AR.page
        path Routes.Tests.SR >=> processAction Tests.SR.page
        path Routes.Tests.HR >=> processAction Tests.HR.page
        path Routes.Tests.Test >=> processAction Tests.TestEnvironment.page
        path Routes.Tests.startTest >=> processAction Tests.TestEnvironment.startTest
        path Routes.Tests.finishTest >=> processAction Tests.TestEnvironment.finishTest
        path Routes.Tests.answerTest >=> processAction Tests.TestEnvironment.answerTest

        pathRegex "(.*)\.(css|jpg|svg|png|gif|js)" >=> Files.browseHome

        processAction Views.NotFound
    ]

let appWithTrace app =
    context (fun ctx -> 
                withInfoLog (sprintf "Requested %s" ctx.request.path)
            )
    >=> app

let errorHandler (exc:System.Exception) reason ctx =
    OK "<html><head><meta http-equiv=\"refresh\" content=\"5;url=/\"></head><body><h1>500 Internal Server Error</h1><a href=\"/\">Redirecting...</a></body></html>"
    >=> withErrorLog (sprintf "%s - %s\n%s" reason  exc.Message exc.StackTrace)
    <| ctx

let appSecurity app =
    let assertHTTPS settings app =
        context (fun ctx ->
                    let url = ctx.request.url.AbsoluteUri
                    let decoded = System.Net.WebUtility.UrlDecode url
                    if decoded.StartsWith("http:") then redirect ("https" + decoded.Substring(4))
                    else settings >=> app
                )
    let addHSTS = Writers.addHeader "Strict-Transport-Security" "max-age=31536000; includeSubDomains"
    let addCSP = Writers.addHeader "Content-Security-Policy" "default-src 'self'"
    let addXFrame = Writers.addHeader "X-Frame-Options" "SAMEORIGIN"
    let addXSSP = Writers.addHeader "X-XSS-Protection" "1; mode=block"
    let addCTO = Writers.addHeader "X-Content-Type-Options" "nosniff"
    let addRP = Writers.addHeader "Referrer-Policy" "origin-when-cross-origin"
    assertHTTPS (addHSTS >=> addCSP >=> addXFrame >=> addXSSP >=> addCTO >=> addRP) app

let fullApp = app |> appWithTrace |> appSecurity

let serverConfig = 
    let envport = System.Environment.GetEnvironmentVariable "port"
    let port = (if not (isNull envport) then envport else "8000") |> int
    { defaultConfig 
        with
            bindings = [ HttpBinding.createSimple HTTP "0.0.0.0" port ]
            logger = Targets.create Debug [| "Suave"; "Suave.Tcp" |]
            homeFolder = Some (System.IO.Directory.GetCurrentDirectory())
            errorHandler = errorHandler
    }

startWebServer serverConfig fullApp
