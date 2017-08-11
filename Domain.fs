module Domain

type User =
    {
        Id : int
        Email : string
        Administrator : bool
    }
    with
        member this.IsAdmin () = this.Administrator

(* Id, correct, contents *)
type QuestionsAnwser = int * bool * string

type Question =
    {
        Question : string
        Answers : QuestionsAnwser array
    }

type Answer =
    {
        Question : Question
        AnswerId : int
    }

type Test =
    {
        Id : int
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