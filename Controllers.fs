module Controllers

open Suave
open Suave.Filters
open Suave.Successful
open Suave.Operators
open Suave.RequestErrors

open Authentication
open Helpers
open Session
open Db
open Domain
open Action

module Index =
    let page = Views.indexPage

module Materials =
    let page = Views.simplePage "materials.html"

module Payment =
    let page = Views.simplePage "payment.html"

module Login =
    let page (session:Session) (ctx:HttpContext) = 
        choose [
            GET >=> ((notLoggedOn ?> Views.loginPage None session.Csrf) session ctx)
            (POST >+=> (notLoggedOn ?> fun session ctx ->
                        let httpReq = ctx.request
                        let email = postData httpReq "email"
                        let password = postData httpReq "password"
                        match Db.verifyUser email password with
                        | Some user -> 
                                    cleanUser user
                                    authenticateUser user session ctx
                        | None ->
                            Views.loginPage (Some "Niepoprawny email lub hasło.") session.Csrf session ctx
                            >=> withDebugLog "Invalid login credentials"
                       )
                ) session ctx
        ]
    let reset =
        request (fun _ ->
            Cookies.removeSessionCookie >=> Routes.returnPathOrHome)

module Register =
    let page (session:Session) ctx =
        choose [
            GET >=> ((notLoggedOn ?> Views.registrationPage None session.Csrf) session ctx)
            (POST >+=> (notLoggedOn ?> fun session ctx ->
                let httpReq = ctx.request
                let email = postData httpReq "email"
                let password = postData httpReq "password"
                let name = postData httpReq "name"
                let surname = postData httpReq "surname"
                let team = postData httpReq "team"
                match email, password, name, surname, team with
                | Some email_, Some password_, Some name_, Some surname_, Some team_ ->
                    match Db.registerUser email_ password_ name_ surname_ team_ with
                    | Choice2Of2 err -> 
                        Views.registrationPage (Some err) session.Csrf session ctx
                        >=> withDebugLog err
                    | Choice1Of2 user -> authenticateUser user session ctx
                | _ -> Views.BadRequest session ctx
                )) session ctx
        ]

module Directory =
    let page session ctx = Views.directoryPage (getAllUserData()) session ctx

module Profile =
    let page id =
        match getUser id with
        | Some user ->
            let usrData = getUserData user
            Views.profilePage usrData
        | _ -> Views.NotFound

module Account =
    let page =
        loggedOn ?> (fun session ctx ->
                        let usrData = getUserData session.User.Value
                        Views.accountPage usrData session ctx)

module Tests =    
    module TestEnvironment =
        let createTest testType user =
            let questions = match testType with
                            | AR -> getARQuestions() |> Seq.scramble |> Seq.take 25
                            | SR -> 
                                let ar = getARQuestions() |> Seq.scramble |> Seq.take 5|> Seq.cache
                                let sr = getSRQuestions() |> Seq.scramble |> Seq.take 20 |> Seq.cache
                                Seq.append ar sr |> Seq.scramble
                            | HR -> getHRQuestions() |> Seq.scramble |> Seq.take 50
            newTest user testType questions
        let action (session:Session) ctx =
            let req = ctx.request
            (
                match postData req "test" with
                | Some testType_ -> 
                    let testType = questionType testType_
                    if session.User.IsNone then Views.BadRequest
                    else
                        let usrData = getUserData session.User.Value
                        match usrData.CanTakeTest testType with
                        | Choice1Of2 true ->
                            let test = createTest testType session.User.Value
                            sessionWithTest (Some test.Value.Id) >+=> Views.testEnvironment testType
                        | _ -> Views.BadRequest
                | None -> Views.BadRequest
            ) session ctx
        let page = loggedOn ?> POST >+=> action
        let jsonResponse (questions:Question array) (started:System.DateTime) (time:System.TimeSpan) custom =
            OK <| (sprintf "{\"questions\":%s, \"started\":%s, \"time\":{\"minutes\":%d, \"seconds\":%d}%s}"
                    (Json.toJson questions |> utf8) (Json.toJson started |> utf8) time.Minutes time.Seconds
                    (Option.orDefault "" custom)
                  )
        let prepareQuestions questions =
            questions |> Seq.map (fun (q:Question) -> 
                                            {q with Answers = 
                                                    q.Answers 
                                                    |> Seq.map (fun a -> {a with Correct = false})
                                                    |> Seq.scramble 
                                                    |> Array.ofSeq })
                      |> Seq.scramble |> Array.ofSeq

        let startTest = 
            loggedOn ?> POST >+=> fun sess ctx ->
                        match sess.Test with
                        | Some test -> 
                            let test = startTest test
                            jsonResponse (prepareQuestions test.Questions) test.StartedTime.Value (testTime test.Type) None
                        | _ -> BAD_REQUEST ""

        let finishTest =
            loggedOn ?> POST >+=> fun sess ctx ->
                        match sess.Test with
                        | Some test ->
                            let test, mark = finishTest test
                            let badQuestions = getIncorrectAnsweredQuestions test
                            let additionalJson = ", \"mark\":" + (Json.toJson mark |> utf8)
                            jsonResponse badQuestions test.StartedTime.Value test.Duration (Some additionalJson)
                            >=> sessionWithTest None sess ctx (* Remove the test from session since it's finished. *)
                        | _ -> BAD_REQUEST ""

        let answerTest =
            loggedOn ?> POST >+=> fun sess ctx ->
                        match sess.TestId with
                        | Some testId ->
                            request (fun req ->
                                            match req.rawForm |> utf8 with
                                            | Sscanf "q:%d;a:%d;" (qid, aid) ->
                                                markAnswer testId qid (Some aid)
                                                OK ""
                                            | _ -> BAD_REQUEST "Malformed request"
                                    ) 
                        | _ -> BAD_REQUEST ""

    module TestPage =
        let page (testType:QuestionType) (testSummary:string) =
            loggedOn ?> 
            fun (sess:Session) ctx ->
                (
                    match sess.User with
                    | Some usr ->
                        let usrData = getUserData usr
                        let testValue = testType.ToString().ToLower()
                        let link = 
                            match usrData.CanTakeTest testType with
                            | Choice1Of2 true ->
                                "<form action='/test' method='POST'><input type='submit' value='Przejdź do testu'>" + (makeCSRFinput sess.Csrf) + "<input type='hidden' name=\"test\" value=\"" + testValue + "\"></form>"
                            | Choice1Of2 false ->
                                "<p>Ten test został przez ciebie już zaliczony.</p>"
                            | Choice2Of2 None -> "<p>Nie spełniasz wszystkich wymagań, aby być dopuszczonym do tego testu</p>"
                            | Choice2Of2 (Some date) ->
                                "<p>Wygląda na to, że nie możesz pisać tego testu do <strong>" + date.ToString("dd-MM-yyyy H:mm") + "</strong></p>"
                        Views.testPage ("Test " + testType.ToString()) testSummary link
                    | None -> Views.BadRequest
                ) sess ctx
    module AR =
        let page = TestPage.page AR "<p>Test na sędziego pomocniczego ma za zadanie sprawdzić twoją wiedzę z zakresu zasad gry w Quidditcha obejmujących zadania sędziego pomocniczego. Zanim przystąpisz do tego testu koniecznie odśwież swoją wiedzę z następujących rozdziałów:</p>
        <ul>
        <li>2. Wyposażenie i wymiary boiska</li>
        <li>5. Zbicie</li>
        <li>6. Zasady zachowania i interakcji</li>
        <li>7. Zawodnicy</li>
        <li>8.2.2. Sędziowie pomocniczy</li>
        </ul>"
    
    module SR =
        let page = TestPage.page SR "<p>Test na sędziego zniczowego ma za zadanie sprawdzić twoją wiedzę z zakresu zasad gry w Quidditcha obejmujących zadania sędziego zniczowego. Ponieważ obowiązki sędziego zniczowego częściowo pokrywają się z obowiązkami sędziego pomocniczego oraz często się zdarza, że sędzia zniczowy jest sędzią pomocniczym dopóki znicz nie wejdzie na boisko, to <u>ten test zawiera 5 pytań jakie znajdują się w testach na sędziego pomocnicznego</u>. Zalecamy, aby najpierw zdać test na sędziego pomocniczego. Zanim przystąpisz do tego testu koniecznie odśwież swoją wiedzę z następujących rozdziałów:</p>
        <ul>
        <li>2. Wyposażenie i wymiary boiska</li>
        <li>3.3. Zatrzymanie gry</li>
        <li>4.5. Złapanie znicza</li>
        <li>6. Zasady zachowania i interakcji</li>
        <li>7.5. Zasady dotyczące szukających</li>
        <li>8.2.3. Sędzia zniczowy</li>
        <li>8.3. Ludzki znicz</li>
        </ul>"

    module HR =
        let page = TestPage.page HR "<p>Test na sędziego głównego ma za zadanie sprawdzić twoją wiedzę z całego zakresu zasad gry w Quidditcha. Obowiązki sędziego głównego są opisane w rozdziale 8.1. Sędzia główny. Zanim przystąpisz do tego testu koniecznie odśwież swoją wiedzę z całego Rulebooka!</p><p>Aby podejść do testu na sędziego głównego należy uiścić opłatę za test w wysokości 35zł. Szczegółowe informacje dotyczące płatności znajdziesz <strong><a href=\"/payment\">tutaj</a></strong>.</p><p>Trzeba również mieć zaliczone testy na sędziego pomocniczego i zniczowego.</p>"
