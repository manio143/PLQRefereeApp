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

type UserData =
    {
        Id : int64
        Name : string
        Surname : string
        Team : string
        Ar : int option
        Sr : int option
        Hr : int option
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

type QuestionType = AR | SR | HR
let questionType (s:string) =
    match s with
    | "ar" | "AR" -> AR
    | "sr" | "SR" -> SR
    | "hr" | "HR" -> HR
    | _ -> raise (System.ArgumentException())


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
        Answers : Answer array
        Type : QuestionType
    }

type Test =
    {
        Id : int64
        Questions : Question array
        Answers : Answer array
        StartedTime : System.DateTime option
        FinishedTime : System.DateTime option
        User : User
    }
    with
        member this.Duration = 
            match this.FinishedTime, this.StartedTime with
            | Some x, Some y -> x - y
            | _ -> System.TimeSpan()


type CSRF = string
type SessionId = string
type Session =
    | NotLoggedIn of SessionId * CSRF
    | LoggedIn of SessionId * User * CSRF * Test option
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
        member this.Test =
            match this with
            | NotLoggedIn _ -> None
            | LoggedIn (_, _, _, test) -> test