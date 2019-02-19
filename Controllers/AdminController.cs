using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PLQRefereeApp
{
    public class AdminController : Controller
    {
        private UserRepository UserRepository { get; }
        private TestRepository TestRepository { get; }
        private QuestionRepository QuestionRepository { get; }
        public AdminController(UserRepository userRepository, TestRepository testRepository, QuestionRepository questionRepository)
        {
            UserRepository = userRepository;
            TestRepository = testRepository;
            QuestionRepository = questionRepository;
        }

        private bool Allow()
        {
            return HttpContext.Session.GetUser(UserRepository).Administrator;
        }

        [HttpGet("/admin")]
        [Authorize]
        public IActionResult Panel(string error)
        {
            if (!Allow())
                return LocalRedirect("/");
            var questions = QuestionRepository.GetAllQuestions().Select(q => new QuestionModel
            {
                Question = q,
                Answers = QuestionRepository.GetAnswersFor(q)
            });
            var model = new AdminViewModel
            {
                Tests = TestRepository.GetAllTest(),
                Questions = questions
            };
            ViewBag.Error = error;
            return View("Admin", model);
        }
    }

    public class QuestionModel
    {
        public Question Question { get; set; }
        public IEnumerable<Answer> Answers { get; set; }
    }
    public class AdminViewModel
    {
        public IEnumerable<QuestionModel> Questions { get; set; }
        public IEnumerable<Test> Tests { get; set; }
    }
}