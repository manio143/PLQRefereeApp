module Routes

type IntPath = PrintfFormat<(int -> string),unit,string,string,int>

let index = "/"
let materials = "/materials"
let payment = "/payment"

let login = "/login"
let logout = "/logout"
let register = "/register"

let directory = "/directory"
let profile : IntPath = "/profile/%i"

module Account =
    let private prefix = "/account"
    let myTests = prefix + "/tests"
    let myAccount = prefix + "/details"

module Tests =
    let private prefix = "/test"
    let AR = prefix + "/AR"
    let SR = prefix + "/SR"
    let HR = prefix + "/HR"
    let Test = prefix
    let startTest = prefix + "-start"
    let finishTest = prefix + "-finish"
    let answerTest = prefix + "-answer"

let databaseBackup = "/dbbackup"

open Suave

let withParam (key,value) path = sprintf "%s?%s=%s" path key value


let redirectWithReturnPath redirection =
    request (fun x ->
        let path = x.url.AbsolutePath
        Redirection.FOUND (redirection |> withParam ("returnPath", path)))

let returnPathOrHome = 
    request (fun x -> 
        let path = 
            match (x.queryParam "returnPath") with
            | Choice1Of2 path -> 
                if path.StartsWith("/") then path else index
            | _ -> index
        Redirection.FOUND path)
