module Views

open Suave
open Suave.Successful
open Suave.RequestErrors

DotLiquid.setTemplatesDir "./templates"

let NotFound = NOT_FOUND "Not found"

let Unauthorized = UNAUTHORIZED "You must log in!"
let Forbidden = FORBIDDEN "Only for admin"

let CSRFValidationFailed = BAD_REQUEST "The request didn't pass CQRS validation."

type LoginPageViewModel = {Error : string option; Csrfinput : string}
open System.IO
let loginPage (viewModel:LoginPageViewModel) =
    // Template.FileSystem <- { new DotLiquid.FileSystems.IFileSystem with
    //     member this.ReadTemplateFile(context, templateName) =
    //       let templatePath = context.[templateName] :?> string
    //       let fullPath = Path.Combine("./templates", templatePath)
    //       if not (File.Exists(fullPath)) then failwithf "File not found: %s" fullPath
    //       File.ReadAllText(fullPath) }
    // let template = Template.Parse(System.IO.File.ReadAllText("templates/login.html"))
    // OK <| template.Render(Hash.FromAnonymousObject(viewModel))
    DotLiquid.page "login.html" viewModel