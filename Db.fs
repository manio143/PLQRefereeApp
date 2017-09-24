module Db

open FSharp.Data.Sql
open System.Linq

open Domain
open Helpers

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
                    CaseSensitivityChange = Common.CaseSensitivityChange.ORIGINAL,
                    UseOptionTypes = true>

let db = SqlProvider.GetDataContext()

let getUser id =
    query {
        for user in db.Main.User do
        where (user.Id = Some(id))
        select user
    } |> Seq.first |> Option.map (fun usr -> usr.MapTo<User>())

let getUserData (usr:User) =
    query {
        for userData in db.Main.UserData do
        where (userData.Id = Some(usr.Id))
        select userData
    } |> Seq.first |> Option.map (fun usrData -> usrData.MapTo<UserData>()) |> fun opt -> opt.Value

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
        let user = db.Main.User.``Create(administrator, email, passphrase)``(true, email, createPassphrase password)
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

let getDbAnswer id = 
    query { 
        for answ in db.Main.Answer do
        where(answ.Id = Some(id))
        select answ
    } |> Seq.first

let getQuestions (filter:SqlProvider.dataContext.``main.QuestionEntity`` seq -> SqlProvider.dataContext.``main.QuestionEntity`` seq) =
    let questions = query {
        for q in db.Main.Question do
        select q
    }
    questions |> filter |> Seq.map (fun q ->
                            let questionsAnswers = 
                                query {
                                    for qa in db.Main.QuestionsAnswer do
                                        where(qa.QuestionId = q.Id.Value)
                                        select qa
                                } |> Seq.map (fun qa -> qa.AnswerId) |> Array.ofSeq
                            let answers = 
                                query {
                                    for a in db.Main.Answer do
                                        where(questionsAnswers.Contains(a.Id.Value))
                                        select a
                                } |> Seq.map (fun a -> a.MapTo<Answer>())
                            Question (q.Id.Value) (q.Question) (Array.ofSeq answers) (questionType q.Type)
                         )
    |> Seq.cache

let getAllQuestions () = getQuestions id
let getARQuestions () = getQuestions (Seq.filter (fun q -> q.Type.ToUpper() = "AR"))
let getSRQuestions () = getQuestions (Seq.filter (fun q -> q.Type.ToUpper() = "SR"))
let getHRQuestions () = getQuestions (Seq.filter (fun q -> q.Type.ToUpper() = "HR"))

let getQuestionsForTest (dbTest:SqlProvider.dataContext.``main.TestEntity``) =
    query {
        for tq in db.Main.TestQuestion do
            where(tq.TestId = dbTest.Id.Value)
            select tq
    }
    |> Seq.map (fun tq ->
        let question = 
            getQuestions (Seq.filter (fun q -> q.Id.Value = tq.QuestionId)) 
            |> Seq.exactlyOne
        let answer = (getDbAnswer tq.AnswerId.Value).Value.MapTo<Answer>()
        question, answer
    )

let getDbTest id =
    query {
        for test in db.Main.Test do
            where (test.Id = Some(id))
            select test
    } |> Seq.first
   
let getTest id =
    let test = getDbTest id
    test |> Option.map (fun (t:SqlProvider.dataContext.``main.TestEntity``) ->
        let questions, answers = getQuestionsForTest t |> Seq.unzip
        let user = getUser (t.UserId)
        {Id = t.Id.Value; Questions = Array.ofSeq questions; Answers = Array.ofSeq answers; StartedTime = Some t.Started; FinishedTime = t.Finished; User = user.Value})

let newTest (user:User) (testType:QuestionType) (questions:Question seq) =
    let test = db.Main.Test.Create()
    test.Started <- System.DateTime.Now
    test.UserId <- user.Id
    test.Type <- Some(testType.ToString())
    do db.SubmitUpdates()
    for q in questions do
        let tq = db.Main.TestQuestion.Create()
        tq.QuestionId <- q.Id
        tq.TestId <- test.Id.Value
    done
    do db.SubmitUpdates()
    getTest (test.Id.Value)

let startTest (test:Test) =
    let dbTest = (getDbTest test.Id).Value
    dbTest.Started <- System.DateTime.Now
    db.SubmitUpdates()


let getSessionDbObj sessionId =
    query {
        for session in db.Main.Session do
        where (session.SessionId = sessionId)
        select session
    } |> Seq.first

let removeOldSessions () =
    query {
        for session in db.Main.Session do
        where (session.Expires <= System.DateTime.Now)
        select session
    } |> Seq.iter (fun s -> s.Delete())
    db.SubmitUpdates()

let getSession sessionId =
    removeOldSessions()
    let ses = getSessionDbObj sessionId
    match ses with
    | None -> None
    | Some ses ->
        match ses.UserId with
        | Some usr ->
            Some(LoggedIn(ses.SessionId, (getUser usr).Value, ses.Csrftoken, Option.bind getTest ses.TestId))
        | None -> Some(NotLoggedIn(ses.SessionId, ses.Csrftoken))
let saveSession (session:Session) =
    removeOldSessions()
    let tomorrow = System.DateTime.Now.AddHours(12.0)
    match getSessionDbObj session.SessionId with
    | Some ses ->
        ses.Csrftoken <- session.Csrf
        ses.Expires <- tomorrow
        if session.Test.IsSome then ses.TestId <- Some session.Test.Value.Id
        else ses.TestId <- None
    | None ->
        let ses = db.Main.Session.Create()
        ses.SessionId <- session.SessionId
        ses.Csrftoken <- session.Csrf
        ses.Expires <- tomorrow
        if session.User.IsSome then ses.UserId <- Some session.User.Value.Id
        if session.Test.IsSome then ses.TestId <- Some session.Test.Value.Id
    db.SubmitUpdates()
        