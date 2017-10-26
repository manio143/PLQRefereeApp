module Domain

open System

type User =
    {
        Id : int64
        Email : string
        Administrator : bool
    }
    with
        member this.IsAdmin () = this.Administrator

type QuestionType = AR | SR | HR
let questionType (s:string) =
    match s with
    | "ar" | "AR" -> AR
    | "sr" | "SR" -> SR
    | "hr" | "HR" -> HR
    | _ -> raise (System.ArgumentException())

type UserData =
    {
        Id : int64
        Name : string
        Surname : string
        Team : string
        Ar : int64 option
        Sr : int64 option
        Hr : int64 option
        Arcooldown : DateTime option
        Srcooldown : DateTime option
        Hrcooldown : DateTime option
        ArIrdp : bool
        SrIrdp : bool
        HrIrdp : bool
        HrPayment : bool
    }
    with
        member this.CanTakeAR = this.Ar.IsNone && this.Arcooldown.IsNone && (not this.ArIrdp)
        member this.CanTakeSR = this.Sr.IsNone && this.Srcooldown.IsNone && (not this.SrIrdp)
        member this.CanTakeHR = this.Hr.IsNone && this.Hrcooldown.IsNone && (not this.HrIrdp) 
                                && this.HrPayment && (this.Ar.IsSome || this.ArIrdp) && (this.Sr.IsSome || this.SrIrdp)

        member this.HasARCooldown = this.Arcooldown.IsSome
        member this.HasSRCooldown = this.Srcooldown.IsSome
        member this.HasHRCooldown = this.Hrcooldown.IsSome

        member this.ARCooldown = this.Arcooldown
        member this.SRCooldown = this.Srcooldown
        member this.HRCooldown = this.Hrcooldown

        member this.CanTakeTest testType =
            match testType with
             | AR -> 
                 if this.CanTakeAR then Choice1Of2 true
                 else if this.HasARCooldown then Choice2Of2 this.ARCooldown
                      else Choice1Of2 false
             | SR -> 
                 if this.CanTakeSR then Choice1Of2 true
                 else if this.HasSRCooldown then Choice2Of2 this.SRCooldown
                      else Choice1Of2 false
             | HR -> 
                 if this.CanTakeHR then Choice1Of2 true
                 else if this.HasHRCooldown then Choice2Of2 this.HRCooldown
                      else if this.HrIrdp then Choice1Of2 false
                           else Choice2Of2 None


(* Id, correct, contents *)
type Answer = 
    {
        Id : int64
        Correct : bool
        Answer : string
    }

type Question =
    {
        Id : int64
        Question : string
        Information : string
        Answers : Answer array
        Type : QuestionType
    }
let Question id question information answers ``type`` = {Id = id; Question = question; Information = information; Answers = answers; Type = ``type``}

type Test =
    {
        Id : int64
        Questions : Question array
        Answers : Answer option array
        StartedTime : System.DateTime option
        FinishedTime : System.DateTime option
        Type : QuestionType
        User : User
    }
    with
        member this.Duration = 
            match this.FinishedTime, this.StartedTime with
            | Some x, Some y -> x - y
            | _ -> System.TimeSpan()

let testTime testType =
    match testType with
    | AR | SR -> TimeSpan(0, 20, 0)
    | HR -> TimeSpan(0, 35, 0)

let testQuestionCount testType =
    match testType with
    | AR | SR -> 25
    | HR -> 50

type AuthenticationState = Authenticated | NotAuthenticated

type CSRF = string
type SessionId = string
type TestId = int64
type Session =
    | NotLoggedIn of SessionId * CSRF
    | LoggedIn of SessionId * User * CSRF * TestId option
    with
        member this.Csrf =
            match this with
            | NotLoggedIn (_, csrf) -> csrf
            | LoggedIn (_, _, csrf, _) -> csrf
        member this.SessionId =
            match this with
            | NotLoggedIn (id, _) -> id
            | LoggedIn (id, _, _, _) -> id
        member this.User =
            match this with
            | NotLoggedIn _ -> None
            | LoggedIn (_, user, _, _) -> Some user
        member this.TestId =
            match this with
            | NotLoggedIn _ -> None
            | LoggedIn (_, _, _, test) -> test
        member this.Authenticated =
            match this with
            | LoggedIn _ -> Authenticated
            | NotLoggedIn _ -> NotAuthenticated