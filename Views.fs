module Views

open Suave
open Suave.Successful
open Suave.Operators
open Suave.RequestErrors
open Suave.Writers

open System

open Domain
open Action

DotLiquid.setTemplatesDir "./templates"

let page filename (model:'a) =
    Action.ofWebPart (DotLiquid.page filename model)

type BaseViewModel(title:string, isAuthenticated:AuthenticationState) =
    let mutable isAuth = isAuthenticated
    member val Title = title
    member this.IsAuthenticated with get() = isAuth = Authenticated
    member this.SetAuth auth = isAuth <- auth
    new() = BaseViewModel("", NotAuthenticated)
    new(title) = BaseViewModel(title, NotAuthenticated)
    new(auth) = BaseViewModel("", auth)

let mkPage filename (model:BaseViewModel) =
    fun (session:Session) (ctx:HttpContext) ->
        model.SetAuth(session.Authenticated)
        page filename model session ctx

type GenericPageViewModel(title:string, content:string) =
    inherit BaseViewModel(title)
    member val Content = content
let genericPage title content =
    mkPage "generic.html" (GenericPageViewModel(title, content))

let simplePage fileName =
    mkPage fileName (BaseViewModel(""))

let error code title =
    genericPage title (sprintf "<h1>%s</h1>" title) 
    >+=> Action.ofWebPart (setStatus code)

let NotFound = error HttpCode.HTTP_404 "Not found"

let Unauthorized = error HttpCode.HTTP_401 "You must log in!"
let Forbidden = error HttpCode.HTTP_403 "Only for admin"

let BadRequest = error HttpCode.HTTP_400 "An unexpected error occured."

let CSRFValidationFailed = error HttpCode.HTTP_400 "The request didn't pass CQRS validation."

let indexPage =
    simplePage "main.html" 

type AuthorizationPageViewModel(error:string option, csrf:CSRF) =
    inherit BaseViewModel("")
    member val Error = error
    member val Csrfinput = Helpers.makeCSRFinput csrf

let loginPage error csrf =
    mkPage "login.html" (AuthorizationPageViewModel(error, csrf))

let registrationPage error csrf =
    mkPage "register.html" (AuthorizationPageViewModel(error, csrf))

(* TestButton is either <a ..> or explanation why user can't take test *)
type TestPageViewModel(title:string, summary:string, testButton:string) =
    inherit BaseViewModel(title)
    member val Summary = summary
    member val TestButton = testButton

let testPage title summary testButton =
    mkPage "test_page.html" (TestPageViewModel(title, summary, testButton))

type DirectoryPageViewModel(users:UserData seq) =
    inherit BaseViewModel("")
    member val Users = users

let directoryPage users =
    mkPage "directory.html" (DirectoryPageViewModel(users))

type TestEnvironmentViewModel(testTitle:string, testTime:TimeSpan, questionCount:int) =
    inherit BaseViewModel(testTitle)
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
    mkPage "test.html" (TestEnvironmentViewModel(title, time, questionCount))

type ProfileViewModel(user:UserData) =
    inherit BaseViewModel(sprintf "%s %s" user.Name user.Surname)
    member val User = user

let profilePage user =
    mkPage "profile.html" (ProfileViewModel(user))
let accountPage user =
    mkPage "account.html" (ProfileViewModel(user))

DotLiquid.Template.RegisterSafeType(typeof<BaseViewModel>, [|"Title"; "IsAuthenticated"|])
DotLiquid.Template.RegisterSafeType(typeof<GenericPageViewModel>, [|"Title"; "IsAuthenticated"; "Content"|])
DotLiquid.Template.RegisterSafeType(typeof<AuthorizationPageViewModel>, [|"Title"; "IsAuthenticated"; "Error"; "Csrfinput"|])
DotLiquid.Template.RegisterSafeType(typeof<TestPageViewModel>, [|"Title"; "IsAuthenticated"; "Summary"; "TestButton"|])
DotLiquid.Template.RegisterSafeType(typeof<DirectoryPageViewModel>, [|"Title"; "IsAuthenticated"; "Users"|])
DotLiquid.Template.RegisterSafeType(typeof<TestEnvironmentViewModel>, [|"Title"; "IsAuthenticated"; "TestTitle"; "TestTime"; "QuestionCount"|])
DotLiquid.Template.RegisterSafeType(typeof<ProfileViewModel>, [|"Title"; "IsAuthenticated"; "User"|])
DotLiquid.Impl.tryRegisterTypeTree (typeof<UserData>)
