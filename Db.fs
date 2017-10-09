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

let mapUserData (dbUserData:SqlProvider.dataContext.``main.UserDataEntity``) =
    {
        Id = dbUserData.Id.Value
        Name = dbUserData.Name.Value
        Surname = dbUserData.Name.Value
        Team = dbUserData.Team.Value
        Ar = dbUserData.Ar
        Sr = dbUserData.Sr
        Hr = dbUserData.Hr
        Arcooldown = dbUserData.Arcooldown
        Srcooldown = dbUserData.Srcooldown
        Hrcooldown = dbUserData.Hrcooldown
        ArIrdp = dbUserData.ArIrdp
        SrIrdp = dbUserData.SrIrdp
        HrIrdp = dbUserData.HrIrdp
        HrPayment = dbUserData.HrPayment
    }

let getDbUserData (usr:User) =
    query {
        for userData in db.Main.UserData do
        where (userData.Id = Some(usr.Id))
        select userData
    } |> Seq.first 

let getUserData (usr:User) =
    getDbUserData usr |> Option.map mapUserData 
    |> Option.get

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
                                db.Main.QuestionsAnswer |> Seq.cache
                                |> Seq.map (fun qa -> (qa.QuestionId, qa.AnswerId))
                                |> Seq.filter (fun (qq, a) -> qq = q.Id.Value)
                                |> Seq.map (fun (qq, a) -> a) |> Array.ofSeq
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

let getQuestionsForTest testId =
    query {
        for tq in db.Main.TestQuestion do
            where(tq.TestId = testId)
            select tq
    }
    |> Seq.map (fun tq ->
        let question = 
            getQuestions (Seq.filter (fun q -> q.Id.Value = tq.QuestionId)) 
            |> Seq.exactlyOne
        let answer = tq.AnswerId |> Option.map (fun aid -> (getDbAnswer aid).Value.MapTo<Answer>())
        question, answer
    ) |> Seq.cache

let getDbTest id =
    query {
        for test in db.Main.Test do
            where (test.Id = Some(id))
            select test
    } |> Seq.first
   
let getTest id =
    let dbTest = getDbTest id
    let test = dbTest |> Option.map (fun (t:SqlProvider.dataContext.``main.TestEntity``) ->
        let questions, answers = getQuestionsForTest t.Id.Value |> Seq.unzip
        let user = getUser (t.UserId)
        {Id = t.Id.Value; Questions = Array.ofSeq questions; Answers = Array.ofSeq answers; StartedTime = t.Started; FinishedTime = t.Finished; Type = questionType t.Type; User = user.Value})
    //printfn "GET TEST: %A" test
    test

type Session with
    member this.Test =
        match this.TestId with
        | Some tid -> getTest tid
        | None -> None

let newTest (user:User) (testType:QuestionType) (questions:Question seq) =
    let test = db.Main.Test.Create()
    test.Started <- None
    test.UserId <- user.Id
    test.Type <- testType.ToString()
    do db.SubmitUpdates()
    for q in questions do
        let tq = db.Main.TestQuestion.Create()
        tq.QuestionId <- q.Id
        tq.TestId <- test.Id.Value
    done
    do db.SubmitUpdates()
    getTest (test.Id.Value)

let markAnswer testId questionId answerId =
    query {
        for tq in db.Main.TestQuestion do
        where(tq.TestId = testId && tq.QuestionId = questionId)
        select tq
    } |> Seq.exactlyOne
    |> fun tq ->
        tq.AnswerId <- answerId
        db.SubmitUpdates()

let finishTest (test:Test) =
    let dbTest = (getDbTest test.Id).Value
    match dbTest.Finished with
    | Some _ -> test
    | None ->
        let correctlyAnswered = 
            test.Answers 
            |> Seq.fold (fun acc answer -> 
                             acc + Option.orDefault 0 (Option.map (fun a -> if a.Correct then 1 else 0) answer)) 0
        let mark = (correctlyAnswered |> decimal) / (testQuestionCount test.Type |> decimal) > 0.80m
        let usrData = (getDbUserData test.User).Value
        if mark then
            match test.Type with
            | AR -> usrData.Ar <- Some test.Id
            | SR -> usrData.Sr <- Some test.Id
            | HR -> usrData.Hr <- Some test.Id
        else
            match test.Type with
            | AR -> usrData.Arcooldown <- Some (System.DateTime.Now.AddDays(6.0))
            | SR -> usrData.Srcooldown <- Some (System.DateTime.Now.AddDays(6.0))
            | HR -> usrData.Hrcooldown <- Some (System.DateTime.Now.AddDays(6.0))
        let now = Some System.DateTime.Now
        dbTest.Finished <- now
        db.SubmitUpdates()
        {test with FinishedTime = now}

let getIncorrectAnsweredQuestions (test:Test) =
    seq {
        for i in [0..test.Answers.Length-1] do
            match test.Answers.[i] with
            | Some answer -> if not answer.Correct then
                                yield {test.Questions.[i] with Answers =
                                                               test.Questions.[i].Answers 
                                                               |> Seq.map (fun a -> {a with Correct = a.Id = answer.Id})
                                                               |> Seq.scramble
                                                               |> Array.ofSeq}
            | None -> ()
    } |> Seq.scramble |> Array.ofSeq

let startTest (test:Test) =
    let dbTest = (getDbTest test.Id).Value
    match dbTest.Started with
    | None -> 
            dbTest.Started <- Some (System.DateTime.Now.AddSeconds(2.0))
            db.SubmitUpdates()
            async {
                let time = testTime test.Type + System.TimeSpan(0,0,2)
                let miliseconds = time.TotalMilliseconds |> int
                do! Async.Sleep miliseconds
                finishTest test |> ignore
            } |> Async.Start
            {test with StartedTime = dbTest.Started}
    | _ -> test

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
            Some(LoggedIn(ses.SessionId, (getUser usr).Value, ses.Csrftoken, ses.TestId))
        | None -> Some(NotLoggedIn(ses.SessionId, ses.Csrftoken))
let saveSession (session:Session) =
    removeOldSessions()
    let tomorrow = System.DateTime.Now.AddHours(12.0)
    match getSessionDbObj session.SessionId with
    | Some ses ->
        ses.Csrftoken <- session.Csrf
        ses.Expires <- tomorrow
        ses.TestId <- session.TestId
    | None ->
        let ses = db.Main.Session.Create()
        ses.SessionId <- session.SessionId
        ses.Csrftoken <- session.Csrf
        ses.Expires <- tomorrow
        if session.User.IsSome then ses.UserId <- Some session.User.Value.Id
        if session.TestId.IsSome then ses.TestId <- session.TestId
    db.SubmitUpdates()
        