module App

open Suave
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open Suave.Successful
open Suave.Logging

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
        pathScan Routes.profile Profile.page

        path Routes.Account.myTests >=> loggedOn Tests.page
        path Routes.Account.myAccount >=> loggedOn Account.page

        path Routes.Tests.AR >=> loggedOn Tests.AR.page
        path Routes.Tests.SR >=> loggedOn Tests.SR.page
        path Routes.Tests.HR >=> loggedOn Tests.HR.page
        path Routes.Tests.Test >=> POST >=> loggedOn Tests.TestEnvironment.page
        path Routes.Tests.startTest >=> Tests.TestEnvironment.startTest
        path Routes.Tests.finishTest >=> Tests.TestEnvironment.finishTest
        path Routes.Tests.answerTest >=> Tests.TestEnvironment.answerTest

        pathRegex "(.*)\.(css|jpg|svg|png|gif|js)" >=> Files.browseHome

        path Routes.databaseBackup >=> loggedAdmin (Files.file "database.sqlite")

        (Session.session (fun sess -> Views.NotFound sess.Authenticated))
    ]

let serverConfig = 
    let envport = System.Environment.GetEnvironmentVariable "port"
    let port = (if not (isNull envport) then envport else "8000") |> int
    { defaultConfig 
        with
            bindings = [ HttpBinding.createSimple HTTP "127.0.0.1" port ]
            logger = Targets.create Debug [| "Suave" |]
            homeFolder = Some (System.IO.Directory.GetCurrentDirectory())
    }

startWebServer serverConfig app