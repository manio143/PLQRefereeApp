using System;
using System.Collections.Generic;
using System.Linq;

namespace PLQRefereeApp
{
    public class TestRepository
    {
        public TestRepository(MainContext mainCtx)
        {
            Context = mainCtx;
        }

        private MainContext Context { get; }

        public Test GetTest(int testId) => Context.Tests.First(t => t.Id == testId);

        public void AddTest(Test test)
        {
            if (Context.Tests.Any(t => t.Id == test.Id))
                throw new ArgumentException("Cannot add an object for the seconds time.");
            Context.Tests.Add(test);
            Context.SaveChanges();
            var testQuestions = GetTestQuestionsFor(test);
            foreach (var question in test.Questions)
            {
                if (!testQuestions.Any(tq => tq.TestId == test.Id && tq.QuestionId == question.Id))
                    Context.TestQuestions.Add(new TestQuestion { TestId = test.Id, QuestionId = question.Id, AnswerId = null });
            }
            Context.SaveChanges();
        }
        public void RemoveUnusedTests()
        {
            Context.Tests.RemoveRange(Context.Tests.Where(t => t.Started == null && t.Created < DateTime.Now.AddDays(-1)));
            Context.SaveChanges();
        }

        public IEnumerable<Test> GetTestsFor(User user)
        {
            return Context.Tests.Where(t => t.UserId == user.Id);
        }

        public void MarkAnswer(int testId, int questionId, int answerId)
        {
            if (!Context.Tests.Any(t => t.Id == testId))
                throw new ArgumentException("No such object", nameof(testId));
            if (!Context.Questions.Any(q => q.Id == questionId))
                throw new ArgumentException("No such object", nameof(questionId));
            if (!Context.Answers.Any(a => a.Id == answerId))
                throw new ArgumentException("No such object", nameof(answerId));

            var testQuestion = Context.TestQuestions.First(tq => tq.TestId == testId && tq.QuestionId == questionId);
            testQuestion.AnswerId = answerId;

            Context.SaveChanges();
        }

        public void StartTest(Test test)
        {
            test.Started = DateTime.Now;
            Context.SaveChanges();
        }

        public int MarkTest(Test test)
        {
            if(test.Finished != null && test.Mark.HasValue)
                return test.Mark.Value;

            if(test.Finished == null) 
                test.Finished = DateTime.Now;

            var testQuestions = GetTestQuestionsFor(test);
            var total = testQuestions.Count();
            var correct = testQuestions.ToList().Select(tq => Context.Answers.FirstOrDefault(a => a.Id == tq.AnswerId)).Count(a => a?.Correct ?? false);
            var percentage = 100 * correct / total;
            
            test.Mark = percentage;
            Context.SaveChanges();

            return percentage;
        }

        internal bool HasStartedATest(User user)
        {
            return Context.Tests.Any(t => t.UserId == user.Id && t.Started != null && t.Finished == null);
        }

        public void MarkUnmarkedTests()
        {
            foreach (var t in Context.Tests.Where(t => t.Mark == null && t.Started != null && t.Started < DateTime.Now.AddHours(-1)))
                MarkTest(t);
        }

        internal IEnumerable<TestQuestion> GetTestQuestionsFor(Test test)
        {
            return Context.TestQuestions.Where(tq => tq.TestId == test.Id);
        }

        internal IEnumerable<Question> GetQuestionsFor(Test test, bool all = true)
        {
            var testQuestions = GetTestQuestionsFor(test).ToList();
            var questions = 
                all 
                ? testQuestions.Select(tq => Context.Questions.First(q => q.Id == tq.QuestionId))
                : testQuestions.Where(tq => Context.Answers.First(a => tq.AnswerId == a.Id).Correct == false).Select(tq => Context.Questions.First(q => q.Id == tq.QuestionId));
            return questions;
        }
    }
}