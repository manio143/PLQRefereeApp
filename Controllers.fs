module Controllers

open Suave
open Suave.Filters
open Suave.Successful
open Suave.Operators

open Authentication
open Helpers
open Session

module Index =
    let page = OK "Index"

module Materials =
    let page = OK "Materials"

module Login =
    open Authentication
    let page = 
        choose [
            GET >=> (notLoggedOn <| Views.loginPage None)
            POST >> notLoggedOn <| request (fun httpCtx ->
                        let email = postData httpCtx "email"
                        let password = postData httpCtx "password" 
                        match Db.verifyUser email password with
                        | Some user -> authenticateUser user
                        | None -> Views.loginPage (Some "Invalid login credentials")
                    )
        ]
    let reset =
        request (fun r ->
            Cookies.removeSessionCookie >=> Routes.returnPathOrHome)

module Register =
    open Authentication
    let page = OK "Register"

module Directory =
    let page = OK "Directory"

module Profile =
    let page id = OK (sprintf "Profile of %d" id)

module Account =
    let page = OK "Account details"

module Tests =
    let page = OK "Tests"
    
    module AR =
        let page = OK "AR"
    
    module SR =
        let page = OK "SR"

    module HR =
        let page = OK "HR"