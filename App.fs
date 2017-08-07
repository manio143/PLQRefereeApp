module App

open Suave
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open Suave.Successful

open Authentication
open Controllers

let app =
    choose [
        path Routes.index >=> Index.page
        path Routes.materials >=> Materials.page
        
        path Routes.login >=> Login.page
        path Routes.logout >=> Login.reset
        path Routes.register >=> Register.page

        path Routes.directory >=> Directory.page
        pathScan Routes.profile (fun id -> Profile.page id)

        path Routes.Account.myTests >=> loggedOn Tests.page
        path Routes.Account.myAccount >=> loggedOn Account.page

        path Routes.Tests.AR >=> loggedOn Tests.AR.page
        path Routes.Tests.SR >=> loggedOn Tests.SR.page
        path Routes.Tests.HR >=> loggedOn Tests.HR.page

        pathRegex "(.*)\.(css|jpg|svg|png|gif|js)" >=> Files.browseHome

        Views.NotFound
    ]

let serverConfig = 
    let envport = System.Environment.GetEnvironmentVariable "port"
    let port = (if not (isNull envport) then envport else "8000") |> int
    { defaultConfig with bindings = [ HttpBinding.createSimple HTTP "127.0.0.1" port ] }

startWebServer serverConfig app