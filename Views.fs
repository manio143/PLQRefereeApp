module Views

open Suave
open Suave.Successful
open Suave.Operators
open Suave.RequestErrors
open Suave.Writers

open System

open Domain

DotLiquid.setTemplatesDir "./templates"

type GenericPageViewModel = {Title : string; Content : string}
let genericPage title content =
    DotLiquid.page "generic.html" {Title = title; Content = content}

let error code title =
    genericPage title (sprintf "<h1>%s</h1>" title) >=> setStatus code

let NotFound = error HttpCode.HTTP_404 "Not found"

let Unauthorized = error HttpCode.HTTP_401 "You must log in!"
let Forbidden = error HttpCode.HTTP_403 "Only for admin"

let BadRequest = error HttpCode.HTTP_400 "An unexpected error occured."

let CSRFValidationFailed = error HttpCode.HTTP_400 "The request didn't pass CQRS validation."

let indexPage = DotLiquid.page "main.html" []

type LoginPageViewModel = {Error : string option; Csrfinput : string}
let loginPage (viewModel:LoginPageViewModel) =
    DotLiquid.page "login.html" viewModel

let registrationPage (viewModel:LoginPageViewModel) =
    DotLiquid.page "register.html" viewModel

(* TestButton is either <a ..> or explanation why user can't take test *)
type TestPageViewModel = {Title : string; Summary : string; TestButton : string}
let testPage (viewModel:TestPageViewModel) =
    DotLiquid.page "test_page.html" viewModel

let simplePage fileName =
    DotLiquid.page fileName ()

type TestEnvironmentViewModel = {TestTitle : string; TestTime : TimeSpan; QuestionCount : int}
let testEnvironment (testType:Domain.QuestionType) =
    let time = testTime testType
    let title, questionCount =
        match testType with
        | AR -> "Test na sędziego pomocniczego", 25
        | SR -> "Test na sędziego zniczowego", 25
        | HR -> "Test na sędziego głównego", 50
    DotLiquid.page "test.html" {TestTitle = title; TestTime = time; QuestionCount = questionCount}
