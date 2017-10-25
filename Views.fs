module Views

open Suave
open Suave.Successful
open Suave.Operators
open Suave.RequestErrors
open Suave.Writers

open System

open Domain

DotLiquid.setTemplatesDir "./templates"

type BaseViewModel(title:string, isAuthenticated:AuthenticationState) =
    let isAuth = isAuthenticated = Authenticated
    member val Title = title
    member val IsAuthenticated = isAuth

type GenericPageViewModel(title:string, content:string, isAuthenticated:AuthenticationState) =
    inherit BaseViewModel(title, isAuthenticated)
    member val Content = content
let genericPage title content isAuth =
    DotLiquid.page "generic.html" (GenericPageViewModel(title, content, isAuth))

let simplePage fileName isAuth =
    DotLiquid.page fileName (BaseViewModel("", isAuth))

let error code title isAuth =
    genericPage title (sprintf "<h1>%s</h1>" title) isAuth >=> setStatus code

let NotFound = error HttpCode.HTTP_404 "Not found"

let Unauthorized = error HttpCode.HTTP_401 "You must log in!"
let Forbidden = error HttpCode.HTTP_403 "Only for admin"

let BadRequest = error HttpCode.HTTP_400 "An unexpected error occured."

let CSRFValidationFailed = error HttpCode.HTTP_400 "The request didn't pass CQRS validation."

let indexPage =
    simplePage "main.html" 

type AuthorizationPageViewModel(error:string option, csrf:CSRF) =
    inherit BaseViewModel("", NotAuthenticated)
    member val Error = error
    member val Csrfinput = Helpers.makeCSRFinput csrf

let loginPage error csrf =
    DotLiquid.page "login.html" (AuthorizationPageViewModel(error, csrf))

let registrationPage error csrf =
    DotLiquid.page "register.html" (AuthorizationPageViewModel(error, csrf))

(* TestButton is either <a ..> or explanation why user can't take test *)
type TestPageViewModel(title:string, summary:string, testButton:string) =
    inherit BaseViewModel(title, Authenticated)
    member val Summary = summary
    member val TestButton = testButton

let testPage title summary testButton =
    DotLiquid.page "test_page.html" (TestPageViewModel(title, summary, testButton))

type DirectoryPageViewModel(users:UserData seq, isAuthenticated:AuthenticationState) =
    inherit BaseViewModel("", isAuthenticated)
    member val Users = users

let directoryPage users isAuth =
    DotLiquid.page "directory.html" (DirectoryPageViewModel(users, isAuth))

type TestEnvironmentViewModel(testTitle:string, testTime:TimeSpan, questionCount:int) =
    inherit BaseViewModel(testTitle, Authenticated)
    member val TestTitle = testTitle
    member val TestTime = testTime
    member val QuestionCount = questionCount

let testEnvironment (testType:Domain.QuestionType) =
    let time = testTime testType
    let title, questionCount =
        match testType with
        | AR -> "Test na sędziego pomocniczego", 25
        | SR -> "Test na sędziego zniczowego", 25
        | HR -> "Test na sędziego głównego", 50
    DotLiquid.page "test.html" (TestEnvironmentViewModel(title, time, questionCount))

DotLiquid.Template.RegisterSafeType(typeof<BaseViewModel>, [|"Title"; "IsAuthenticated"|])
DotLiquid.Template.RegisterSafeType(typeof<GenericPageViewModel>, [|"Title"; "IsAuthenticated"; "Content"|])
DotLiquid.Template.RegisterSafeType(typeof<AuthorizationPageViewModel>, [|"Title"; "IsAuthenticated"; "Error"; "Csrfinput"|])
DotLiquid.Template.RegisterSafeType(typeof<TestPageViewModel>, [|"Title"; "IsAuthenticated"; "Summary"; "TestButton"|])
DotLiquid.Template.RegisterSafeType(typeof<DirectoryPageViewModel>, [|"Title"; "IsAuthenticated"; "Users"|])
DotLiquid.Template.RegisterSafeType(typeof<TestEnvironmentViewModel>, [|"Title"; "IsAuthenticated"; "TestTitle"; "TestTime"; "QuestionCount"|])
DotLiquid.Impl.tryRegisterTypeTree (typeof<UserData>)
