module Helpers

open Suave
open Suave.Logging

[<AutoOpen>]
module ChoiceOption =
    let optionOfChoice c =
        match c with
        | Choice1Of2 x -> Some x
        | Choice2Of2 _ -> None        

    (** returns a if a.IsSome else b **)
    let (>>=>) a b =
        match a with
        | Some x -> Some x
        | None ->
            match b with
            | Some x -> Some x
            | None -> None

    let postData (ctx:HttpRequest) key =
        ctx.formData key |> optionOfChoice
        >>=> (ctx.fieldData key |> optionOfChoice)

[<AutoOpen>]
module Logging =
    let logMessage level message ctx =
        ctx.runtime.logger.log level (fun _ -> Message.event level message) |> Async.RunSynchronously

    let verboseLog message = logMessage Verbose message
    let debugLog message = logMessage Debug message
    let withVerboseLog (message:string) =
        context (function ctx ->
                                fun ctx2 -> async {
                                    do verboseLog message ctx
                                    return Some ctx2
                                }
                )
    let withDebugLog (message:string) =
        context (function ctx ->
                                fun ctx2 -> async {
                                    do debugLog message ctx
                                    return Some ctx2
                                }
                )

let makeCSRFinput csrftoken =
    sprintf "<input type=\"hidden\" name=\"csrftoken\" value=\"%s\">" csrftoken

module Seq =
    let unzip seq =
        let rec unzipper list acc1 acc2 =
            match list with
            | (p1, p2) :: tail -> unzipper tail (p1::acc1) (p2::acc2)
            | [] -> List.rev acc1 |> Seq.ofList, List.rev acc2 |> Seq.ofList
        unzipper (List.ofSeq seq) [] []

    (* From http://fssnip.net/16 *)
    let scramble (sqn : seq<'T>) = 
        let rnd = System.Random()
        let rec scramble2 (sqn : seq<'T>) = 
            let remove y sqn = sqn |> Seq.filter (fun x -> x <> y)
     
            seq {
                if not <| Seq.isEmpty sqn then 
                    let x = sqn |> Seq.item (rnd.Next(0, sqn |> Seq.length))
                    yield x
                    yield! scramble2 (remove x sqn)
            }
        scramble2 sqn