open System
open System.IO

let readAQuestion typ (lines:string array) i qid aid =
    if lines.Length - i < 6 then (None, qid, aid)
    else
    let q = lines.[i]
    let ca = lines.[i+1]
    let inca = [lines.[i+2], aid+4; lines.[i+3], aid+3; lines.[i+4], aid+2]
    let info = lines.[i+5]
    let qsql = sprintf "INSERT INTO Question (id, question, information, type) VALUES (%d, '%s', '%s', '%s');\n" (qid+1) q info typ
    let casql = sprintf "INSERT INTO Answer (id, correct, answer) VALUES (%d, TRUE, '%s');\n" (aid+1) ca
    let incasql = inca |> List.fold (fun acc (x,id) -> sprintf "INSERT INTO Answer (id, correct, answer) VALUES (%d, FALSE, '%s');\n" id x + acc) ""
    let qasql = [1..4] |> List.fold (fun acc i -> (+) acc <| sprintf "INSERT INTO QuestionsAnswer VALUES (%d, %d);\n" (qid+1) (aid+i)) ""
    (Some (qsql+casql+incasql+qasql), qid+1, aid+4)

[<EntryPoint>]
let main argv =
    let filename = argv.[0]
    let typ = argv.[1]
    let startQid = int <| argv.[2]
    let startAid = int <| argv.[3]
    let lines = File.ReadAllLines(filename)
    let mutable out = ""
    let rec loop i qid aid =
        let sql, qid2, aid2 = readAQuestion typ lines i qid aid
        if sql.IsNone then ()
        else
            out <- out + sql.Value
            loop (i+7) qid2 aid2
    do loop 0 startQid startAid
    printfn "%s" out
    0
