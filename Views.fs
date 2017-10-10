module Views

open Suave
open Suave.Successful
open Suave.RequestErrors

open System

open Domain

DotLiquid.setTemplatesDir "./templates"

let NotFound = NOT_FOUND "Not found"

let Unauthorized = UNAUTHORIZED "You must log in!"
let Forbidden = FORBIDDEN "Only for admin"

let BadRequest = BAD_REQUEST "An unexpected error occured."

let CSRFValidationFailed = BAD_REQUEST "The request didn't pass CQRS validation."

let indexPage = DotLiquid.page "main.html" []

type LoginPageViewModel = {Error : string option; Csrfinput : string}
let loginPage (viewModel:LoginPageViewModel) =
    DotLiquid.page "login.html" viewModel

(* TestButton is either <a ..> or explanation why user can't take test *)
type TestPageViewModel = {Title : string; Summary : string; TestButton : string}
let testPage (viewModel:TestPageViewModel) =
    DotLiquid.page "test_page.html" viewModel

let simplePage fileName =
    DotLiquid.page fileName ()

type GenericPageViewModel = {Title : string; Content : string}
let genericPage title content =
    DotLiquid.page "generic.html" {Title = title; Content = content}

type TestEnvironmentViewModel = {TestTitle : string; TestTime : TimeSpan; QuestionCount : int}
let testEnvironment (testType:Domain.QuestionType) =
    let time = testTime testType
    let title, questionCount =
        match testType with
        | AR -> "Test na sędziego pomocniczego", 25
        | SR -> "Test na sędziego zniczowego", 25
        | HR -> "Test na sędziego głównego", 50
    DotLiquid.page "test.html" {TestTitle = title; TestTime = time; QuestionCount = questionCount}
