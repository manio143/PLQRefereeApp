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
            var testQuestions = test.TestQuestion;
            foreach (var question in test.Questions)
            {
                if (!testQuestions.Any(tq => tq.QuestionId == question.Id))
                    testQuestions.Add(new TestQuestion { TestId = test.Id, QuestionId = question.Id, AnswerId = null });
            }
            Context.SaveChanges();
        }
        public void RemoveUnusedTests()
        {
            Context.Tests.RemoveRange(Context.Tests.Where(t => t.Started == null && t.Created < DateTime.Now.AddDays(-1)));
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
    }
}