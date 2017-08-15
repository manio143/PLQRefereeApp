module Generators

let createRandomByteArray size =
    let bytes = [|for i in 1..size -> 0uy|]
    let randomizer = System.Security.Cryptography.RandomNumberGenerator.Create()
    randomizer.GetBytes(bytes)
    bytes

let toBase64String bytes =
    System.Convert.ToBase64String bytes

let trimBase64String (str:string) =
    str.Substring(0, str.Length - 2)

let createRandomString bytesSize =
    createRandomByteArray bytesSize 
    |> toBase64String
    |> trimBase64String


let createCSRFToken () = createRandomString 64
let createSessionId () = createRandomString 128