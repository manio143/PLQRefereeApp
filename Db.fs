module Db

open FSharp.Data.Sql
open System.Linq

open Domain
open Helpers

[<Literal>]
let connectionString = "Host=docker;Database=Main;Username=root;Password=plqroot1029"
[<Literal>]
let resolutionPath = @"packages/MySql.Data/lib/net452" //SET

type SqlProvider = SqlDataProvider<
                    Common.DatabaseProviderTypes.MYSQL,
                    ConnectionString = connectionString,
                    ResolutionPath = resolutionPath,
                    CaseSensitivityChange = Common.CaseSensitivityChange.ORIGINAL,
                    UseOptionTypes = true>

let db = 
    let dbhost = System.Environment.GetEnvironmentVariable "dbhost"
    if isNull dbhost then SqlProvider.GetDataContext()
    else SqlProvider.GetDataContext(sprintf "Host=%s;Database=Main;Username=root;Password=plqroot1029" dbhost)

let getUser id =
    query {
        for user in db.Main.Users do
        where (user.Id = id)
        select user
    } |> Seq.first |> Option.map (fun usr -> usr.MapTo<User>())

let bool (sbyte:sbyte) = not ((=) sbyte 0y)

let mapUserData (dbUserData:SqlProvider.dataContext.``Main.UserDataEntity``) =
    {
        Id = dbUserData.Id
        Name = dbUserData.Name
        Surname = dbUserData.Surname
        Team = dbUserData.Team
        Ar = dbUserData.Ar
        Sr = dbUserData.Sr
        Hr = dbUserData.Hr
        Arcooldown = dbUserData.Arcooldown
        Srcooldown = dbUserData.Srcooldown
        Hrcooldown = dbUserData.Hrcooldown
        ArIrdp = dbUserData.ArIrdp |> bool
        SrIrdp = dbUserData.SrIrdp |> bool
        HrIrdp = dbUserData.HrIrdp |> bool
        HrPayment = dbUserData.HrPayment |> bool
    }

let userMap (user:SqlProvider.dataContext.``Main.UsersEntity``) =
    {
        Id = user.Id
        Email = user.Email
        Administrator = user.Administrator |> bool
        Reset = user.Reset
    }

let getDbUserData (usr:User) =
    query {
        for userData in db.Main.UserData do
        where (userData.Id = usr.Id)
        select userData
    } |> Seq.first 

let getUserData (usr:User) =
    getDbUserData usr |> Option.map mapUserData 
    |> Option.get

let getAllUserData() =
    query {
        for userData in db.Main.UserData do
        sortBy userData.Team
        select userData
    } |> Seq.map mapUserData |> Seq.cache

let getDbUser email =
    query {
        for user in db.Main.Users do
            where (user.Email = email)
            select user
    } |> Seq.first

let emailExists email = 
    db.Main.Users.Any(fun u -> u.Email = email)


let createPassphrase passwd =
    let salt = BCrypt.Net.BCrypt.GenerateSalt(13)
    BCrypt.Net.BCrypt.HashPassword(passwd, salt)

let registerUser email password name surname team =
    if emailExists email then Choice2Of2 "Istnieje już konto o podanym adresie email."
    else if System.String.IsNullOrWhiteSpace(email) || System.String.IsNullOrWhiteSpace(password) then Choice2Of2 "Nieprawidłowe dane logowania."
    else
        db.ClearUpdates() |> ignore
        let user = db.Main.Users.``Create(administrator, email, passphrase)``(0y, email, createPassphrase password)
        db.SubmitUpdates()
        let userData = db.Main.UserData.Create()
        userData.Id <- user.Id
        userData.Name <- name
        userData.Surname <- surname
        userData.Team <- team
        db.SubmitUpdates()
        Choice1Of2 (userMap user)


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

let cleanUser user =
    let dbUser = getDbUserData user |> Option.get
    if dbUser.Arcooldown.IsSome && dbUser.Arcooldown.Value < System.DateTime.Now then dbUser.Arcooldown <- None
    if dbUser.Srcooldown.IsSome && dbUser.Srcooldown.Value < System.DateTime.Now then dbUser.Srcooldown <- None
    if dbUser.Hrcooldown.IsSome && dbUser.Hrcooldown.Value < System.DateTime.Now then dbUser.Hrcooldown <- None
    db.SubmitUpdates()

let getDbAnswer id = 
    query { 
        for answ in db.Main.Answer do
        where(answ.Id = id)
        select answ
    } |> Seq.first

let getQuestions (filter:SqlProvider.dataContext.``Main.QuestionEntity`` seq -> SqlProvider.dataContext.``Main.QuestionEntity`` seq) =
    let questions = query {
        for q in db.Main.Question do
        select q
    }
    questions |> filter |> Seq.map (fun q ->
                            let questionsAnswers =
                                db.Main.QuestionsAnswer |> Seq.cache
                                |> Seq.map (fun qa -> (qa.QuestionId, qa.AnswerId))
                                |> Seq.filter (fun (qq, a) -> qq = q.Id)
                                |> Seq.map (fun (qq, a) -> a) |> Array.ofSeq
                            let answers = 
                                query {
                                    for a in db.Main.Answer do
                                        where(questionsAnswers.Contains(a.Id))
                                        select a
                                } |> Seq.map (fun a -> a.MapTo<Answer>())
                            Question (q.Id) (q.Question) (q.Information) (Array.ofSeq answers) (questionType q.Type)
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
            getQuestions (Seq.filter (fun q -> q.Id = tq.QuestionId)) 
            |> Seq.exactlyOne
        let answer = tq.AnswerId |> Option.map (fun aid -> (getDbAnswer aid).Value.MapTo<Answer>())
        question, answer
    ) |> Seq.cache

let getDbTest id =
    query {
        for test in db.Main.Test do
            where (test.Id = id)
            select test
    } |> Seq.first
   
let getTest id =
    let dbTest = getDbTest id
    let test = dbTest |> Option.map (fun (t:SqlProvider.dataContext.``Main.TestEntity``) ->
        let questions, answers = getQuestionsForTest t.Id |> Seq.unzip
        let user = getUser (t.UserId)
        {Id = t.Id; Questions = Array.ofSeq questions; Answers = Array.ofSeq answers; StartedTime = t.Started; FinishedTime = t.Finished; Created = t.Created; Type = questionType t.Type; User = user.Value})
    //printfn "GET TEST: %A" test
    test

type Session with
    member this.Test =
        match this.TestId with
        | Some tid -> getTest tid
        | None -> None

let cleanUnusedTests () =
    query {
         for test in db.Main.Test do
             where(
                 test.Finished.IsNone &&
                 test.Created < System.DateTime.Now.AddDays(-1.0)
             )
             select test
    } |> Seq.iter (fun x -> x.Delete())
    db.SubmitUpdates()

let newTest (user:User) (testType:QuestionType) (questions:Question seq) =
    do cleanUnusedTests()
    let test = db.Main.Test.Create()
    test.Started <- None
    test.UserId <- user.Id
    test.Type <- testType.ToString()
    do db.SubmitUpdates()
    for q in questions do
        let tq = db.Main.TestQuestion.Create()
        tq.QuestionId <- q.Id
        tq.TestId <- test.Id
    done
    do db.SubmitUpdates()
    getTest (test.Id)

let markAnswer testId questionId answerId =
    query {
        for tq in db.Main.TestQuestion do
        where(tq.TestId = testId && tq.QuestionId = questionId)
        select tq
    } |> Seq.exactlyOne
    |> fun tq ->
        tq.AnswerId <- answerId
        db.SubmitUpdates()

type Mark = {CorrectlyAnswered : int; Count : int}
let finishTest (test:Test) =
    let dbTest = (getDbTest test.Id).Value
    let correctlyAnswered = 
        test.Answers 
        |> Seq.fold (fun acc answer -> 
                         acc + Option.orDefault 0 (Option.map (fun a -> if a.Correct then 1 else 0) answer)) 0
    match dbTest.Finished with
    | Some _ -> (test, {CorrectlyAnswered = correctlyAnswered; Count = testQuestionCount test.Type})
    | None ->
        let mark = (correctlyAnswered |> decimal) / (testQuestionCount test.Type |> decimal) >= 0.80m
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
        ({test with FinishedTime = now}, {CorrectlyAnswered = correctlyAnswered; Count = testQuestionCount test.Type})

let getIncorrectAnsweredQuestions (test:Test) =
    let changeQuestion i f = {test.Questions.[i] with Answers =
                                                       test.Questions.[i].Answers 
                                                       |> Seq.map (fun a -> {a with Correct = f a})
                                                       |> Seq.scramble
                                                       |> Array.ofSeq}

    seq {
        for i in [0..test.Answers.Length-1] do
            match test.Answers.[i] with
            | Some answer -> if not answer.Correct then
                                yield changeQuestion i (fun a -> a.Id = answer.Id)
            | None -> yield changeQuestion i (fun _ -> false)
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
        for session in db.Main.Sessions do
        where (session.SessionId = sessionId)
        select session
    } |> Seq.first

let removeOldSessions () =
    query {
        for session in db.Main.Sessions do
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
        let ses = db.Main.Sessions.Create()
        ses.SessionId <- session.SessionId
        ses.Csrftoken <- session.Csrf
        ses.Expires <- tomorrow
        if session.User.IsSome then ses.UserId <- Some session.User.Value.Id
        if session.TestId.IsSome then ses.TestId <- session.TestId
    db.SubmitUpdates()
