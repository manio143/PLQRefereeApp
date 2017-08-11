module Db

open FSharp.Data.Sql
open System.Linq

open Domain

[<Literal>]
let connectionString = 
    "Data Source=" + 
    __SOURCE_DIRECTORY__ + @"/database.sqlite;" + 
    "Version=3;foreign keys=true"
[<Literal>]
let resolutionPath = __SOURCE_DIRECTORY__ + @"/packages/System.Data.SQLite.Core/lib/net46"

type SqlProvider = SqlDataProvider<
                    Common.DatabaseProviderTypes.SQLITE,
                    ConnectionString = connectionString,
                    ResolutionPath = resolutionPath,
                    CaseSensitivityChange = Common.CaseSensitivityChange.ORIGINAL>

let db = SqlProvider.GetDataContext()

let getUser id =
    query {
        for user in db.Main.User do
        where (user.Id = id)
        select user
    } |> Seq.first |> Option.map (fun usr -> usr.MapTo<User>())

let getDbUser email =
    query {
        for user in db.Main.User do
            where (user.Email = email)
            select user
    } |> Seq.first

let emailExists email = 
    db.Main.User.Any(fun u -> u.Email = email)


let createPassphrase passwd =
    let salt = BCrypt.Net.BCrypt.GenerateSalt(13)
    BCrypt.Net.BCrypt.HashPassword(passwd, salt)

let registerUser email password =
    if emailExists email then Choice2Of2 "Istnieje już konto o podanym adresie email."
    else
        (* open transaction *)
        let user = db.Main.User.``Create(email, passphrase)``(email, createPassphrase password)
        db.SubmitUpdates()
        Choice1Of2 user


let goodPassword usersPassphrase passwd =
    BCrypt.Net.BCrypt.Verify(passwd, usersPassphrase)

let verifyUser email password =
    match email, password with
    | Some email, Some password ->
        let dbUser = getDbUser email
        match dbUser with
        | Some usr -> 
            if goodPassword usr.Passphrase password then
                Some (usr.MapTo<User>())
            else None
        | _ -> None
    | _, _ -> None

let getTest id =
    {Id = 0; Questions = [||]; Answers = [||]; StartedTime = None; FinishedTime = None; User = (getUser id).Value}