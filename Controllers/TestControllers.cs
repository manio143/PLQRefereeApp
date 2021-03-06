using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PLQRefereeApp
{
    public class TestController : Controller
    {
        private UserRepository UserRepository { get; }
        private TestRepository TestRepository { get; }
        private QuestionRepository QuestionRepository { get; }
        public TestController(UserRepository userRepository, TestRepository testRepository, QuestionRepository questionRepository)
        {
            UserRepository = userRepository;
            TestRepository = testRepository;
            QuestionRepository = questionRepository;
        }

        private IActionResult TestPage(QuestionType type) {
            ViewBag.User = HttpContext.Session.GetUser(UserRepository);
            UserRepository.CleanUserCooldowns(ViewBag.User);
            ViewBag.UserData = UserRepository.GetUserData(ViewBag.User);
            ViewBag.AlreadyStarted = TestRepository.HasStartedATest(ViewBag.User);
            return View("TestPage", type);
        }

        [Authorize]
        [HttpGet("/test/AR")]
        public IActionResult TestPageAr()
        {
            return TestPage(QuestionType.AR);
        }

        [Authorize]
        [HttpGet("/test/SR")]
        public IActionResult TestPageSr()
        {
            return TestPage(QuestionType.SR);
        }

        [Authorize]
        [HttpGet("/test/HR")]
        public IActionResult TestPageHr()
        {
            return TestPage(QuestionType.HR);
        }

        [Authorize]
        [HttpPost("/test")]
        [ValidateAntiForgeryToken]
        public IActionResult TestEnvironment([FromForm]string test)
        {
            var type = test.ToQuestionType();
            var user = HttpContext.Session.GetUser(UserRepository);
            var userData = UserRepository.GetUserData(user);

            if (!userData.CanTakeTestOf(type) || TestRepository.HasStartedATest(user))
                return BadRequest();

            var test_ = CreateTest(type, user);

            HttpContext.Session.SetInt32("TestId", test_.Id);

            return View(type);
        }

        [Authorize]
        [HttpPost("/test-answer")]
        [ValidateAntiForgeryToken]
        public IActionResult TestAnswer([FromBody]TestAnswerClass answer)
        {
            var test = HttpContext.Session.GetTest(TestRepository);
            TestRepository.MarkAnswer(test.Id, answer.q, answer.a);
            return Ok();
        }

        public class TestAnswerClass { public int q { get; set; } public int a { get; set; } }

        [Authorize]
        [HttpPost("/test-start")]
        [ValidateAntiForgeryToken]
        public IActionResult TestStart()
        {
            var test = HttpContext.Session.GetTest(TestRepository);
            TestRepository.StartTest(test);
            return Json(PrepareQuestions(test, markWrongAnswer: false));
        }

        [Authorize]
        [HttpPost("/test-finish")]
        [ValidateAntiForgeryToken]
        public IActionResult TestFinish()
        {
            var test = HttpContext.Session.GetTest(TestRepository);
            var user = HttpContext.Session.GetUser(UserRepository);
            FinishTest(test, user);
            HttpContext.Session.Remove("TestId");
            return Json(PrepareQuestions(test, markWrongAnswer: true));
        }

        [Authorize]
        [HttpPost("/test-iqa")]
        [ValidateAntiForgeryToken]
        public IActionResult TestIQA(string type, int userId)
        {
            var currentUser = HttpContext.Session.GetUser(UserRepository);
            if(!currentUser.Administrator)
                return Forbid();
            try
            {
                var user = UserRepository.GetUser(userId);
                var test = new Test();
                test.IQA = true;
                test.UserId = userId;
                test.Rulebook = CurrentRulebookVersion;
                test.Type = type.ToQuestionType().ToString();
                test.Questions = Enumerable.Empty<Question>();
                TestRepository.AddTest(test);
                UserRepository.AddCertificate(user, test.Type.ToQuestionType(), test);
                return LocalRedirect("/admin");
            } catch(Exception e) {
                return LocalRedirect("/admin?error="+ System.Net.WebUtility.UrlEncode(e.Message));
            }
        }

        private object PrepareQuestions(Test test, bool markWrongAnswer)
        {
            return new
            {
                mark = test.Mark,
                questions = TestRepository.GetQuestionsFor(test, all: !markWrongAnswer).Select(q => new
                {
                    question = q.Value,
                    id = q.Id,
                    information = q.Information,
                    answers = QuestionRepository.GetAnswersFor(q).Select(a => new
                    {
                        id = a.Id,
                        correct = markWrongAnswer ? TestRepository.GetTestQuestionsFor(test).Where(tq => tq.QuestionId == q.Id).Select(tq => tq.AnswerId == a.Id && a.Correct == false).First() : false,
                        answer = a.Value
                    }).Scramble()
                }).Scramble(),
                started = test.Started,
                time = test.Type.ToQuestionType().Duration().TotalMinutes
            };
        }

        private void FinishTest(Test test, User user)
        {
            var mark = TestRepository.MarkTest(test);
            if (mark >= 80)
            {
                UserRepository.AddCertificate(user, test.Type.ToQuestionType(), test);
            }
            else
            {
                UserRepository.SetCooldown(user, test.Type.ToQuestionType());
            }
        }

        private Test CreateTest(QuestionType type, User user)
        {
            TestRepository.RemoveUnusedTests();
            TestRepository.MarkUnmarkedTests();

            var test = NewTest(type);
            test.Started = null;
            test.UserId = user.Id;
            TestRepository.AddTest(test);
            return test;
        }

        public const RulebookVersion CurrentRulebookVersion = RulebookVersion.RB1820;

        private Test NewTest(QuestionType type)
        {
            List<Question> questions = null;
            switch (type)
            {
                case QuestionType.AR:
                    questions = QuestionRepository.GetARQuestions(CurrentRulebookVersion).Scramble().Take(25).ToList();
                    break;
                case QuestionType.SR:
                    questions = QuestionRepository.GetSRQuestions(CurrentRulebookVersion).Scramble().Take(20).ToList();
                    break;
                case QuestionType.HR:
                    questions = QuestionRepository.GetHRQuestions(CurrentRulebookVersion).Scramble().Take(50).ToList();
                    break;
            }
            var test = new Test();
            test.Questions = questions;
            test.Type = type.ToString();
            test.IQA = false;
            test.Rulebook = CurrentRulebookVersion;
            return test;
        }
    }
}
