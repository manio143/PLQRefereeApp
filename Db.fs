module Db

open Domain

let getUser id =
    {Id = id; Username = "TestUser"; Email = "test@example.com"; Administrator = true}

let getTest id =
    {Id = 0; Questions = [||]; Answers = [||]; StartedTime = None; FinishedTime = None; User = getUser id}