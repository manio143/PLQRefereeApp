module Controllers

open Suave
open Suave.Filters
open Suave.Successful
open Suave.Operators

open Authentication
open Helpers
open Session
open Db
open Domain

module Index =
    let page = Views.indexPage

module Materials =
    let page = Views.genericPage "" "Materials"

module Login =
    open Authentication
    let page = 
        choose [
            GET >=> (notLoggedOn <| withCSRF (fun csrf -> 
                Views.loginPage { Error = None; Csrfinput = makeCSRFinput csrf }))
            POST << notLoggedOn <| request (fun httpCtx ->
                        let email = postData httpCtx "email"
                        let password = postData httpCtx "password" 
                        match Db.verifyUser email password with
                        | Some user -> authenticateUser user
                        | None ->
                            withCSRF (fun csrf -> 
                            Views.loginPage { Error = Some "Invalid login credentials"; Csrfinput = makeCSRFinput csrf })
                            >=> withDebugLog "Invalid login credentials"
                    )
        ]
    let reset =
        request (fun r ->
            Cookies.removeSessionCookie >=> Routes.returnPathOrHome)

module Register =
    open Authentication
    let page = Views.genericPage "" "Register"

module Directory =
    let page = Views.genericPage "" "Directory"

module Profile =
    let page id = Views.genericPage "" (sprintf "Profile of %d" id)

module Account =
    let page = Views.genericPage "" "Account details"

module Tests =
    (* The summary of present certificates and a list of taken tests [Date; Type; Time; Mark] *)
    let page = Views.genericPage "" "Tests"
    
    module TestEnvironment =
        let page = Views.genericPage "" "Test Environment"

    module TestPage =
        open Views
        let page (testType:QuestionType) (testSummary:string) =
            session (fun sess ->
                match sess.User with
                | Some usr ->
                    let usrData = getUserData usr
                    let viewModel = 
                        let usrCanTakeTest = match testType with
                                             | AR -> 
                                                 if usrData.CanTakeAR then Choice1Of2 true
                                                 else if usrData.HasARCooldown then Choice2Of2 (Some usrData.ARCooldown)
                                                      else Choice1Of2 false
                                             | SR -> 
                                                 if usrData.CanTakeSR then Choice1Of2 true
                                                 else if usrData.HasSRCooldown then Choice2Of2 (Some usrData.SRCooldown)
                                                      else Choice1Of2 false
                                             | HR -> 
                                                 if usrData.CanTakeHR then Choice1Of2 true
                                                 else if usrData.HasHRCooldown then Choice2Of2 (Some usrData.HRCooldown)
                                                      else if usrData.HrIrdp then Choice1Of2 false
                                                           else Choice2Of2 None
                        let testValue = testType.ToString().ToLower()
                        let link = 
                            match usrCanTakeTest with
                            | Choice1Of2 true ->
                                "<form action='/test' method='POST'><input type='submit' value='Przejdź do testu'>" + (makeCSRFinput sess.Csrf) + "<input type='hidden' name=\"test\" value=\"" + testValue + "\"></form>"
                            | Choice1Of2 false ->
                                "<p>Ten test został przez ciebie już zaliczony.</p>"
                            | Choice2Of2 None -> "<p>Nie spełniasz wszystkich wymagań, aby być dopuszczonym do tego testu</p>"
                            | Choice2Of2 (Some date) ->
                                "<p>Wygląda na to, że nie możesz pisać tego testu do <strong>" + date.ToString() + "</strong></p>"
                        {Summary = testSummary; TestButton = link; Title = ("Test " + testType.ToString()); }
                    Views.testPage viewModel
                | None -> Views.BadRequest )
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
        let page = TestPage.page SR "<p>Test na sędziego zniczowego ma za zadanie sprawdzić twoją wiedzę z zakresu zasad gry w Quidditcha obejmujących zadania sędziego zniczowego. Ponieważ obowiązki sędziego zniczowego częściowo pokrywają się z obowiązkami sędziego pomocniczego oraz często się zdarza, że sędzia zniczowy jest sędzią pomocniczym dopóki znicz nie wejdzie na boisko, to ten test zawiera kilka pytań jakie znajdują się w testach na sędziego pomocnicznego. Zalecamy, aby najpierw zdać test na sędziego pomocniczego. Zanim przystąpisz do tego testu koniecznie odśwież swoją wiedzę z następujących rozdziałów:</p>
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
        let page = TestPage.page HR "<p>Test na sędziego głównego ma za zadanie sprawdzić twoją wiedzę z całego zakresu zasad gry w Quidditcha. Obowiązki sędziego głównego są opisane w rozdziale 8.1. Sędzia główny. Zanim przystąpisz do tego testu koniecznie odśwież swoją wiedzę z całego Rulebooka!</p><p>Aby podejść do testu na sędziego głównego należy uiścić opłatę za test [detale...]. Trzeba również mieć zaliczone testy na sędziego pomocniczego i zniczowego.</p>"