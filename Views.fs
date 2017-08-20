module Views

open Suave
open Suave.Successful
open Suave.RequestErrors

DotLiquid.setTemplatesDir "./templates"

let NotFound = NOT_FOUND "Not found"

let Unauthorized = UNAUTHORIZED "You must log in!"
let Forbidden = FORBIDDEN "Only for admin"

let CSRFValidationFailed = BAD_REQUEST "The request didn't pass CQRS validation."


let loginPage error =
    DotLiquid.page "login.html" error